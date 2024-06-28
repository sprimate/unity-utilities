using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class CommandExecutor
{
#if !UNITY_WSA || UNITY_EDITOR
    public void StandardIn(string value)
    {
        if (process == null)
        {
            UnityEngine.Debug.LogError("Cannot send data to standard in - process does not exist: [" + value + "]");
        }
    }

    public void KillProcess()
    {
        if (process != null)
        {
            KillChildProcesses();
            KillThisProcess();
        }
    }

    void KillChildProcesses()
    {
        ProcessStartInfo processInfo;
        Process p;
        processInfo = new ProcessStartInfo("cmd.exe", "/c wmic process where (ParentProcessId=" + process.Id + ") get ProcessId");
        processInfo.CreateNoWindow = true;
        processInfo.UseShellExecute = false;
        // *** Redirect the output ***
        processInfo.RedirectStandardError = true;
        processInfo.RedirectStandardOutput = true;

        p = Process.Start(processInfo);
        p.WaitForExit();

        // *** Read the streams ***
        // Warning: This approach can lead to deadlocks, see Edit #2
        string output = p.StandardOutput.ReadToEnd();
        string error = p.StandardError.ReadToEnd();
        p.Close();

        foreach(var l in output.Split('\n'))
        {
            int id;
            if (int.TryParse(l, out id))
            {
                Process.GetProcessById(id)?.Kill();
            }
        }
    }

    void KillThisProcess()
    {
        process.StandardInput.Close();
        process.Kill();
        process = null;
    }

    public Process process { get; protected set; }
    Action<int> OnExit;
    bool verbose;
    public void Execute(string command, Action<string> OnOutput, Action<string> OnError, Action<int> _OnExit, bool waitForExit = false, int? waitForExitTimeout = null, string workingDirectory = null, bool _verbose = false)
    {
        OnExit = _OnExit;
        verbose = _verbose;
        process = new Process();
        // Log?.Invoke("CommandPrompt: " + commandPromptApplication);
        process.StartInfo.FileName = "cmd.exe";
        //process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.Arguments = $@"/C " + command;

        if (workingDirectory != null)
        {
            process.StartInfo.WorkingDirectory = workingDirectory;
        }

        process.Start();

        //var cmd = bashCommand ? "C:\\Windows\\System32\\bash.exe -c \"" + command + "\"" : command;
        var cmd = command;
        //process.StandardInput.WriteLine(cmd);// commandRequest.command);
        //process.StandardInput.Flush();
        UnityEngine.Debug.Log("Processing Request (" + process.Id + "): " + cmd);

        //  commandRequest.processId = process.Id;
        //processes[process.Id] = process;

        Action<string> OnOutputData = (data) =>
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                foreach(var d in data.Split('\n'))
                {
                    if (!string.IsNullOrWhiteSpace(d))
                    {
                        if (verbose)
                        {
                            Debug.Log(d);
                        }

                        OnOutput?.Invoke(d);
                    }
                }
            }
        };

        Action<string> OnErrorData = (data) =>
        {
            if (!string.IsNullOrEmpty(data))
            {
                foreach (var d in data.Split('\n'))
                {
                    if (!string.IsNullOrEmpty(d))
                    {
                        if (verbose)
                        {
                            Debug.Log("|Possible Error| " + d);
                        }

                        OnError?.Invoke(d);
                    }
                }
            }
        };
        /*process.OutputDataReceived += (object sender, System.Diagnostics.DataReceivedEventArgs e) =>
        {
            OnOutputData(e.Data);
        };

        process.BeginOutputReadLine();
        process.ErrorDataReceived += (object sender, System.Diagnostics.DataReceivedEventArgs e) =>
        {
            OnErrorData(e.Data);
        };
        process.BeginErrorReadLine();
        */
        OnOutputData(process.StandardOutput.ReadToEnd());
        OnErrorData(process.StandardError.ReadToEnd());
        if (waitForExit)
        {
            if (waitForExitTimeout.HasValue)
            {
                if (process.WaitForExit(waitForExitTimeout.Value))
                {
                    Exit();
                }
                else
                {
                    Debug.Log("Exited now? " + process.HasExited);
                    string err = "Timed out while waiting for exit [~" + Mathf.RoundToInt(waitForExitTimeout.Value / 1000f) + " seconds]";
                    if (verbose)
                    {
                        Debug.LogError(err);
                    }
                    OnError?.Invoke(err);
                    Exit(-1);
                }
            }
            else
            {
                process.WaitForExit();
                Exit();
            }
        }
    }

    public void Update()
    {
        if (process != null && process.HasExited)
        {
            Exit();
        }
    }

    void Exit(int? code = null)
    {
        CloseProcess(code);
    }

    void CloseProcess(int? code = null)
    {
        if (process != null)
        {
            //   process.StandardInput.Close();
            code = code.HasValue ? code.Value : process.ExitCode;
            if (verbose)
            {
                if (code == 0)
                {
                    Debug.Log("ExitCode: " + code);
                }
                else
                {
                    Debug.LogError("ExitCode: " + code);
                }
            }

            ((Action<int>)OnExit)?.Invoke(code.Value);
            process.Close();
            process = null;
        }
    }

    void OnDestroy()
    {
        KillProcess();
    }
#endif
}