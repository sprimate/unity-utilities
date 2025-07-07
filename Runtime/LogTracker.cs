using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogTracker : AMonoSingleton<LogTracker>
{
    public static List<Log> logs { get; private set; } = new List<Log>();
    static bool initialized = false;
    protected override void Awake()
    {
        base.Awake();
        if (!initialized)
        {
            Application.logMessageReceived += OnLog;
            initialized = true;
        }
    }

    void OnLog(string condition, string stackTrace, LogType type)
    {
        logs.Add(new Log
        {
            condition = condition,
            stackTrace = stackTrace,
            type = type
        });
    }

    public static string GetLogString(int maxChars = 4800000)
    {
        string ret = "";
        for (int i = logs.Count - 1; i >= 0; i--)
        {
            var l = logs[i];
            var toAppend = l.type + ": " + l.condition + "\n" + l.stackTrace + "\n\n";
            if (ret.Length + toAppend.Length > maxChars)
            {
                Debug.Log("Trimmed " + (logs.Count - (logs.Count - i)) + " log entries");
                break;
            }

            ret = toAppend + ret;
        }


        return ret;
    }
}

public struct Log
{
    public string condition;
    public string stackTrace;
    public LogType type;
}