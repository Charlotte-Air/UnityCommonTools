using UnityEngine;
using UnityEditor;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;

public class ShellHelper
{
    private static string shellApp
    {
		get
        {
            string app = "";
#if UNITY_EDITOR_WIN
			app = "cmd.exe";
#elif UNITY_EDITOR_OSX
			app = "bash";
#endif
			return app;
		}
	}
    
    private static bool ProcessCmd(string cmd, string workDirectory, List<string> environmentVars = null)
    {
        StringBuilder output = new StringBuilder();
        StringBuilder error = new StringBuilder();
        bool success = false;
        Process p = null;
        try
        {
            p = new System.Diagnostics.Process();

            p.StartInfo.FileName = shellApp;

#if UNITY_EDITOR_OSX
			string splitChar = ":";
			p.StartInfo.Arguments = "-c";
#elif UNITY_EDITOR_WIN
            string splitChar = ";";
            p.StartInfo.Arguments = "/c";
#endif

            if (environmentVars != null)
            {
                foreach (string var in environmentVars)
                {
                    p.StartInfo.EnvironmentVariables["PATH"] += (splitChar + var);
                }
            }

            p.StartInfo.Arguments += (" \"" + cmd + " \"");
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.ErrorDialog = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.WorkingDirectory = workDirectory;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.StandardOutputEncoding = System.Text.UTF8Encoding.UTF8;
            p.StartInfo.StandardErrorEncoding = System.Text.UTF8Encoding.UTF8;

            using (var outputWaitHandle = new System.Threading.AutoResetEvent(false))
            using (var errorWaitHandle = new System.Threading.AutoResetEvent(false))
            {
                p.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data == null)
                    {
                        outputWaitHandle.Set();
                    }
                    else
                    {
                        output.AppendLine(e.Data);
                    }
                };
                p.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data == null)
                    {
                        errorWaitHandle.Set();
                    }
                    else
                    {
                        error.AppendLine(e.Data);
                    }
                };

                p.Start();

                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                const int timeout = 300 * 1000;
                if (p.WaitForExit(timeout) &&
                    outputWaitHandle.WaitOne(timeout) &&
                    errorWaitHandle.WaitOne(timeout))
                {
                    success = (p.ExitCode == 0);
                }
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogException(e);
            if (p != null)
            {
                p.Close();
            }
        }

        ShowShellLog("@>", output.ToString());
        if (!success)
        {
            ShowShellLog("@>", error.ToString(), true);
        }

        return success;
    }

    private static void ShowShellLog(string prefix, string s, bool error = false)
    {
        if (string.IsNullOrEmpty(s))
        {
            return;
        }
        string[] line = s.Split('\n');
        for (int i = 0; i < line.Length; i++)
        {
            if (!string.IsNullOrEmpty(line[i]))
            {
                if (error)
                {
                    UnityEngine.Debug.LogErrorFormat("  {0}: {1}", prefix, line[i]);
                }
                else
                {
                    UnityEngine.Debug.LogFormat("  {0}: {1}", prefix, line[i]);
                }
            }
        }
    }

	public static bool ProcessCommand(string cmd,string workDirectory,List<string> environmentVars = null)
    {
        return ProcessCmd(cmd, workDirectory, environmentVars);
	}

    private static void CommandShell(string cmd, string workDirectory, List<string> environmentVars)
    {
        Process p = new System.Diagnostics.Process();

        p.StartInfo.FileName = shellApp;

#if UNITY_EDITOR_OSX
		string splitChar = ":";
		p.StartInfo.Arguments = "-c";
#elif UNITY_EDITOR_WIN
        string splitChar = ";";
        p.StartInfo.Arguments = "/c";
#endif

        if (environmentVars != null)
        {
            foreach (string var in environmentVars)
            {
                p.StartInfo.EnvironmentVariables["PATH"] += (splitChar + var);
            }
        }

        p.StartInfo.Arguments += (" \"" + cmd + " \"");
        p.StartInfo.CreateNoWindow = false;
        p.StartInfo.ErrorDialog = true;
        p.StartInfo.UseShellExecute = true;
        p.StartInfo.WorkingDirectory = workDirectory;

        p.Start();

        p.WaitForExit();
        p.Close();
    }

    public static void ProcessCommandShell(string cmd, string workDirectory, List<string> environmentVars = null)
    {
        var thread = new System.Threading.Thread(delegate ()
        {
            CommandShell(cmd, workDirectory, environmentVars);
        });
        thread.Start();
        thread.Join();
    }

    // private static void CommandShell(string cmd, string workDirectory, string args)
    // {
    //     Process p = new System.Diagnostics.Process();

    //     p.StartInfo.FileName = shellApp;

    //     p.StartInfo.Arguments = args;
    //     p.StartInfo.CreateNoWindow = false;
    //     p.StartInfo.ErrorDialog = true;
    //     p.StartInfo.UseShellExecute = true;
    //     p.StartInfo.WorkingDirectory = workDirectory;

    //     p.Start();

    //     p.WaitForExit();
    //     p.Close();
    // }

    private static void CommandShell(string cmd, string workDirectory, string args)
    {
        var pInfo = new System.Diagnostics.ProcessStartInfo(cmd);
        pInfo.Arguments = args;
        pInfo.CreateNoWindow = false;
        pInfo.UseShellExecute = true;
        pInfo.ErrorDialog = true;
        pInfo.WorkingDirectory = workDirectory;

        Process p = System.Diagnostics.Process.Start(pInfo);
        p.WaitForExit();
        p.Close();
    }

    public static void ProcessCommandShell(string cmd, string workDirectory, string args)
    {
        var thread = new System.Threading.Thread(delegate ()
        {
            CommandShell(cmd, workDirectory, args);
        });
        thread.Start();
        thread.Join();
    } 
}
