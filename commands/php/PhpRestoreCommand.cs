using System.ComponentModel;
using System.IO;
using Gitpod.Tool.Helper;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Gitpod.Tool.Commands.Php
{
    class PhpRestoreCommand : Command<PhpRestoreCommand.Settings>
    {
        private Settings settings;

        public class Settings : CommandSettings
        {
            [CommandOption("-d|--debug")]
            [Description("Outputs debug information")]
            [DefaultValue(false)]
            public bool Debug { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            this.settings = settings;

            this.RestorePhpVersion();
            this.RestorePhpIni();

            return 0;
        }

        private void RestorePhpVersion()
        {
            AnsiConsole.Write("Checking if the active file exists....");

            if (!File.Exists("./.devEnv/gitpod/php/active")) {
                AnsiConsole.MarkupLine("[cyan3]Not found[/]");

                return;
            }

            AnsiConsole.MarkupLine("[green1]Found[/]");


            string[] fileContent = File.ReadAllLines("./.devEnv/gitpod/php/active");

            if (this.settings.Debug) {
                AnsiConsole.WriteLine("file content from ./.devEnv/gitpod/php/active: " + fileContent[0]);
            }

            PhpHelper.SetNewPhpVersion(fileContent[0], this.settings.Debug);
        }

        private void RestorePhpIni()
        {
            /*AnsiConsole.Write("Checking if the active file exists....");

            if (!File.Exists("./.devEnv/gitpod/php/active")) {
                AnsiConsole.MarkupLine("[cyan3]Not found[/]");

                return;
            }

            AnsiConsole.MarkupLine("[green1]Found[/]");


            string[] fileContent = File.ReadAllLines("./.devEnv/gitpod/php/active");

            this.SetNewPhpVersion(fileContent[0]);*/

            throw new System.Exception("Not implemented yet");
        }
    }
}