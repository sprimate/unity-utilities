using System.Diagnostics;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class Shortcuts
{
    const string TAILVIEWER_PATH = "C:/Program Files/Tailviewer/Tailviewer.exe";

    [MenuItem("P/Open Tailviewer")]
    static void OpenTailviewer()
    {
        Process.Start(TAILVIEWER_PATH);
    }

    [MenuItem("P/Force Recompile Scripts")]
    public static void ForceRecompile()
    {
        UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
    }

    [MenuItem("P/Clear Console %#c")]
    public static void ClearConsole()
    {
        var logEntries = System.Type.GetType("UnityEditor.LogEntries,UnityEditor.dll");
        var clearMethod = logEntries.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
        clearMethod.Invoke(null, null);
    }
}