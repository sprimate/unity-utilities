#if UNITY_EDITOR

using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class PLogger
{
    //add whatever is clogging up a Debug Log
    private static readonly string[] _toStrip = new string[]
    {
        "com.cysharp.unitask",
        "com.unity.addressables",
        $"{nameof(PLogger)}."
    };


    [HideInCallstack]

    public static void Log(string message, params string[] toStrip)
    {
        Log(message, null, toStrip);
    }

    [HideInCallstack]
    public static void Log(string message, UnityEngine.Object context = null)
    {
        Log(message, context, null);
    }


    [HideInCallstack]
    public static void Log(string message, UnityEngine.Object context = null, params string[] toStrip)
    {
        message = "<color=magenta>P: </color>" + message;
        try
        {
            _ = LogStripped(message, context, toStrip);
        }
        catch
        {
            Debug.Log(message, context);
        }
    }

    [HideInCallstack]
    private async static Awaitable LogStripped(string message, UnityEngine.Object context, string[] toStrip)
    {

        var stackTrace = EnhancedStackTrace.Current();//new StackTrace(true);
        var sb = new StringBuilder();
        sb.AppendLine(message + "\n");

        var stripThings = toStrip == null ? _toStrip : toStrip.Concat(_toStrip);

        foreach (var frame in stackTrace.GetFrames())
        {
            var method = frame.GetMethod();
            var filePath = frame.GetFileName();
            var line = frame.GetFileLineNumber();

            if (filePath == null)
            {
                continue;
            }

            bool shouldStrip = false;
            foreach(var ts in stripThings)
            {
                if (filePath.Contains(ts))
                {
                    shouldStrip = true;
                    break;
                }
            }

            if (shouldStrip)
            {
                continue;
            }

            //FullName?
            var declaringType = $"{method.DeclaringType?.Namespace}.{method.DeclaringType?.Name}" ?? "UnknownType";
            declaringType = Regex.Replace(declaringType, @"<[^<>]+?>", m => $"<color=green>{m.Value}</color>");
            declaringType = Regex.Replace(declaringType, @"</color>.*", "</color>");

            var methodName = System.Text.RegularExpressions.Regex.Replace(method.Name, @"^<(.+?)>d__\d+", "$1");

            var assetsIndex = filePath.IndexOf("Assets");
            var relativePath = assetsIndex >= 0 ? filePath.Substring(assetsIndex) : filePath;


            var toAppend = $"{declaringType}:<color=cyan>{method.Name}</color> - <color=yellow>{relativePath}:{line}</color>)";
            sb.AppendLine(toAppend);
        }

        await Awaitable.BackgroundThreadAsync();//printing after this makes the printed stack trace minimal (since we're manually printing out the stack trace for brevity)
        await Awaitable.MainThreadAsync();
        Debug.Log(sb.ToString(), context);
    }
}

#endif