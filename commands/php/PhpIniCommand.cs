using System.ComponentModel;
using System.IO;
using System.Linq;
using Gitpod.Tool.Helper;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Gitpod.Tool.Commands.Php
{
    class PhpIniCommand : Command<PhpIniCommand.Settings>
    {
        private Settings settings;

        public class Settings : CommandSettings
        {
            [CommandArgument(0, "[Mode]")]
            [DefaultValue("")]
            public string Mode { get; set; }

            [CommandOption("-d|--debug")]
            [Description("Outputs debug information")]
            [DefaultValue(false)]
            public bool Debug { get; set; }
        }
        
        public override ValidationResult Validate(CommandContext context, Settings settings)
        {
            string[] allowedModes = {"update"};

            if (!allowedModes.Contains(settings.Mode))
            {
                return ValidationResult.Error($"Mode not allowed - {settings.Mode}");
            }

            return base.Validate(context, settings);
        }


        public override int Execute(CommandContext context, Settings settings)
        {
            this.settings = settings;

            switch (settings.Mode)
            {
                case "update":
                    this.UpdatePhpIniFiles();

                    return 0;
            }

            AnsiConsole.WriteLine(this.GetPhpIniPath());

            return 0;
        }

        private string GetPhpIniPath()
        {
            return ExecCommand.Exec("php -i | grep 'Configuration File'");
        }

        private void UpdatePhpIniFiles()
        {
            string currentPhpVersion = PhpHelper.GetCurrentPhpVersion();

            if (this.settings.Debug) {
                AnsiConsole.WriteLine("Active PHP Version " + currentPhpVersion);
            }

            AnsiConsole.Write("Checking if we have a folder with ini files for the active version....");

            if (!Directory.Exists("./.devEnv/gitpod/php/config/" + currentPhpVersion)) {
                AnsiConsole.MarkupLine("[cyan3]Directory Not found[/]");

                return;
            }

            string[] additionalIniFiles = Directory.GetFiles("./.devEnv/gitpod/php/config/" + currentPhpVersion, "*.ini", SearchOption.AllDirectories);

            if (additionalIniFiles.Length == 0) {
                AnsiConsole.MarkupLine("[cyan3]No files found[/]");

                return;
            }

            AnsiConsole.MarkupLine("[green1]Found " + additionalIniFiles.Length + " File(s)[/]");

            if (this.settings.Debug) {
                AnsiConsole.WriteLine("Found the following files: ");

                foreach (string file in additionalIniFiles) {
                    AnsiConsole.WriteLine(file);
                }
            }
            
            AnsiConsole.WriteLine("Copy the files to the target directory....");

            foreach (string file in additionalIniFiles) {
                string targetFile = "/etc/php/" + currentPhpVersion + "/" + file.Replace("./.devEnv/gitpod/php/config/" + currentPhpVersion  + "/", "");
                
                if (this.settings.Debug) {
                    AnsiConsole.WriteLine("Copying from \"" + file + "\" to \"" + targetFile + "\"");
                }
                
                ExecCommand.Exec("sudo cp " + file + " " + targetFile);
            }

            AnsiConsole.MarkupLine("[green1]Done[/]");
        }
    }
}