using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Gitpod.Tool.Classes;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Gitpod.Tool.Commands.Shell
{
    class ShellFileCommand : Command<ShellFileCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandArgument(0, "[args]")]
            public string[] Arguments { get; set; }

            [CommandOption("-a|--args")]
            [Description("Show available arguments")]
            [DefaultValue(false)]
            public bool ShowArguments { get; set; }
        }

        private void ShowShellFileArguments(CustomCommand cmd)
        {
            if (cmd.Arguments.Count == 0) {
                AnsiConsole.WriteLine("The Script has no defined Arguments");

                return;
            }

            AnsiConsole.WriteLine("The Script has the following Arguments:");

            foreach (string arg in cmd.Arguments) {
                AnsiConsole.WriteLine(arg);
            }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            CustomCommand cmd = (CustomCommand) context.Data;

            if (settings.ShowArguments) {
                this.ShowShellFileArguments(cmd);

                return 0;
            }

            try
            {
                // Make sure the file is executable
                var proc = new Process();
                var procStartInfo = new ProcessStartInfo()
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = $"/bin/bash",
                    WorkingDirectory = "./",
                    Arguments = $"-c \" chmod +x " + cmd.File + "\""
                };

                proc.StartInfo = procStartInfo;
                proc.Start();
                proc.WaitForExit();

                var args = string.Empty;

                if (settings.Arguments != null && settings.Arguments.Length > 0) {
                    for (int i = 0; i < settings.Arguments.Length; i++) {
                        settings.Arguments[i] = "\\\"" + settings.Arguments[i] + "\\\"";
                    }

                    args = string.Join(' ', settings.Arguments);
                }

                // Execute the shell script
                var process = new Process();

                var processStartInfo = new ProcessStartInfo()
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = $"/bin/bash",
                    WorkingDirectory = "./",
                    Arguments = $"-c \"" + cmd.File + (args != String.Empty ? " " + args : "") + "\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };

                process.StartInfo = processStartInfo;
                process.Start();

                Task.WaitAll(Task.Run(() =>
                {
                    while (!process.StandardOutput.EndOfStream)
                    {
                        var line = process.StandardOutput.ReadLine();
                        Console.WriteLine(line);
                    }
                }), Task.Run(() =>
                {
                    while (!process.StandardError.EndOfStream)
                    {
                        var line = process.StandardError.ReadLine();
                        Console.WriteLine(line);
                    }
                }));

                process.WaitForExit();
            }
            catch(Exception ex)
            {
                AnsiConsole.WriteException(ex);
            }

            return 0;
        }
    }   
}
