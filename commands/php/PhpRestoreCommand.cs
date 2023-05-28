using System;
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
            AnsiConsole.Write("Checking if php version has been set via config....");

            if (GptConfigHelper.Config.Php.Version == String.Empty) {
                AnsiConsole.MarkupLine("[cyan3]Not found[/]");

                return;
            }

            AnsiConsole.MarkupLine("[green1]Found[/]");

            PhpHelper.SetNewPhpVersion(GptConfigHelper.Config.Php.Version, this.settings.Debug);
        }

        private void RestorePhpIni()
        {
            AnsiConsole.Write("Checking if php settings has been set via config....");

            if (GptConfigHelper.Config.Php.Config.Count == 0 && GptConfigHelper.Config.Php.ConfigCLI.Count == 0 && GptConfigHelper.Config.Php.ConfigWeb.Count == 0) {
                AnsiConsole.MarkupLine("[cyan3]Not found[/]");

                return;
            }

            AnsiConsole.MarkupLine("[green1]Found[/]");

            PhpHelper.UpdatePhpIniFiles(this.settings.Debug);
        }
    }
}