using Spectre.Console;

namespace Gitpod.Tool.Helper
{
    class ExecCommand
    {
        public static string Exec(string command)
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
                proc.Start();

                result += proc.StandardOutput.ReadToEnd();
                result += proc.StandardError.ReadToEnd();

                proc.WaitForExit();
            }

            return result;
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
