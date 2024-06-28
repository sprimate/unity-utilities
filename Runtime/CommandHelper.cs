using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
using UnityEditor;
#endif
using UnityEngine;

public static class CommandHelper
{
#if UNITY_EDITOR
    public const string EMPTY_SCENE_PATH = "Assets/Sprimate Studios/Scenes/EmptyScene.unity";
    static string queuedCommandString = "";
    static EditorCoroutine toWrite;
    public static bool startOtherPlayers { get => EditorPrefs.GetBool("startOtherPlayers"); set => EditorPrefs.SetBool("startOtherPlayers", value); }
    public static bool forceSameLobby { get => EditorPrefs.GetBool("forceSameLobby"); set => EditorPrefs.SetBool("forceSameLobby", value); }
    public static bool waitToStartClient { get => EditorPrefs.GetBool("waitToStartClient", true); set => EditorPrefs.SetBool("waitToStartClient", value); }

    public static bool focusCloneOnCommands { get => EditorPrefs.GetBool("focusCloneOnCommands"); set => EditorPrefs.SetBool("focusCloneOnCommands", value); }
    public static bool assignGamepadToClone { get => EditorPrefs.GetBool("assignGamepadToClone"); set => EditorPrefs.SetBool("assignGamepadToClone", value); }
    public static void SendCommand(string command, bool showWarning = true, Action<Command, string> response = null)
    {
        if (!DoesCloneExist())
        {
            return;
        }

        if (!IsClone())
        {
            var id = UnityEngine.Random.Range(0, 999999999);
            //Debug.Log("Sending command " + command);  
            queuedCommandString += id + " " + command + "\n";
            if (Application.isPlaying && toWrite == null)
            {
                toWrite = EditorCoroutineUtility.StartCoroutineOwnerless(WriteQueued());
            }
            else if (!Application.isPlaying)
            {
                WriteToCommandFile();
            }
            if (response != null)
            {
                EditorCoroutineUtility.StartCoroutineOwnerless(WaitForResponse(id, response));
            }
            if (showWarning && !IsCloneOpen())
            {
                Debug.LogError("Warning - the clone project is not currently open, and therefore will not run any commands. Make sure to start the clone project before sending commands");
            }
        }
        else
        {
            Debug.Log("Cannot send commands, since you are already the clone. (Only the main project can send the clones out)");
        }
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        if (!CommandHelper.IsClone())
        {
            try
            {
                File.WriteAllText(CommandHelper.GetResponsePath(), "");
            }
            catch (DirectoryNotFoundException) { }
        }
    }

    static IEnumerator WriteQueued()
    {
        while (true)
        {
            float toWait = 0.1f;
            if (!string.IsNullOrWhiteSpace(queuedCommandString))
            {
                WriteToCommandFile();
                if (!Application.isPlaying)
                {
                    toWrite = null;
                    yield break;
                }
                else
                {
                    toWait = 1f;
                }
            }

            yield return new EditorWaitForSeconds(toWait);
        }
    }

    static void WriteToCommandFile()
    {
        //Debug.Log("Writing Command(s): " + queuedCommandString);
        File.WriteAllText(GetCommandsPath(), queuedCommandString);
        queuedCommandString = "";
    }
    static IEnumerator WaitForResponse(int id, Action<Command, string> calback)
    {
        var waitForOneSecond = new EditorWaitForSeconds(1.0f);
        while (true)
        {
            var lines = File.ReadAllLines(GetResponsePath());
            for (int i = lines.Length - 1; i >= 0; i--)
            {
                var line = lines[i];

                var split = line.Split('|');
                try
                {
                    var command = new Command(split[0]);
                    if (command.id == id)
                    {
                        var response = split[1].Trim();
                        calback?.Invoke(command, response);
                        break;
                    }
                }
                catch { }
            }

            yield return waitForOneSecond;
        }
    }

    public static string GetCloneProjectPath()
    {
        string ret = Path.GetFileName(System.IO.Directory.GetCurrentDirectory());
        ret = Application.dataPath + "/../../" + ret;
        if (!IsClone())
        {
            ret += "_clone";
        }

        return ret;
    }

    public static string GetMainProjectPath()
    {
        string ret = Path.GetFileName(System.IO.Directory.GetCurrentDirectory());
        ret = Application.dataPath + "/../../" + ret;
        if (IsClone())
        {
            ret.Replace("_clone", "");
        }

        return ret;
    }

    public static string GetResponsePath()
    {
        return GetCloneProjectPath() + "/commandResponse.txt";
    }

    public static string GetCommandsPath()
    {
        return GetCloneProjectPath() + "/editorCommands.command";
    }

    public static string GetConsoleOutputPath()
    {
        return GetCloneProjectPath() + "/consoleOutput.log";
    }

    public static string GetClonePlayModePath()
    {
        return CommandHelper.GetCloneProjectPath() + "/playMode";
    }

    public static bool IsClone()
    {
        return Application.dataPath.Contains("_clone");
    }

    public static bool DoesCloneExist()
    {
        return Directory.Exists(GetCloneProjectPath());
    }

    public static bool IsCloneOpen()
    {
        return File.Exists(GetCloneProjectPath() + "/Temp/UnityLockFile");
    }

    public static bool IsCloneRunning()
    {
        return IsCloneOpen() && File.Exists(GetClonePlayModePath());
    }

    public static bool IsMasterOpen()
    {
        return File.Exists(GetMainProjectPath() + "/Temp/UnityLockFile");
    }
#endif
}

public class Command
{
    public int id;
    public string cmd;
    public string parameters;
    public Command(string incoming)
    {
        var split = incoming.Trim().Split(' ');
        id = int.Parse(split[0].Trim());
        cmd = split[1].ToLower();
        parameters = "";
        for (int i = 2; i < split.Length; i++)
        {
            parameters += " " + split[i];
        }

        parameters = parameters.Trim();
    }

    public override string ToString()
    {
        return "{" + id + "}" + " [" + cmd + "]: " + parameters;
    }
}
