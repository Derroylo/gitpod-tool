using System;
using Spectre.Console;

namespace Gitpod.Tool.Helper
{
    class ExecCommand
    {
        public static string Exec(string command, int timeoutInSeconds = 300)
        {
            string result = "";

            using (System.Diagnostics.Process proc = new())
            {
                proc.StartInfo.FileName = "/bin/bash";
                proc.StartInfo.Arguments = "-c \" " + command + " \"";
                proc.StartInfo.UseShellExecute = false;
                proc.EnableRaisingEvents = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardInput = true;
                proc.Start();

                proc.ErrorDataReceived += (sender, errorLine) => { if (errorLine != null) result += errorLine.Data + "\n"; };
                proc.OutputDataReceived += (sender, outputLine) => { if (outputLine != null) result += outputLine.Data + "\n"; };

                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();

                bool exited = proc.WaitForExit(timeoutInSeconds * 1000);

                if (!exited) {
                    proc.Kill();

                    throw new Exception("Command '" + command + "' took longer then the timeout of " + timeoutInSeconds + "s. Check the output for clues on what went wrong: " + result);
                }
            }

            return result.TrimEnd('\n');
        }

        public static string ExecWithDirectOutput(string command)
        {
            string result = "";

            using (System.Diagnostics.Process proc = new())
            {
                proc.StartInfo.FileName = "/bin/bash";
                proc.StartInfo.Arguments = "-c \" " + command + " \"";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardInput = true;
                proc.OutputDataReceived += (sendingProcess, dataLine) => {
                    if (dataLine.Data != null) {
                        AnsiConsole.WriteLine(dataLine.Data);
                    }
                };

                proc.ErrorDataReceived += (sendingProcess, errorLine) => {
                    if (errorLine.Data != null) {
                        AnsiConsole.WriteLine(errorLine.Data);
                    }
                };

                proc.Start();

                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();

                proc.WaitForExit();
            }

            return result;
        }
    }
}
