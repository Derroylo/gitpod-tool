using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Gitpod.Tool.Helper;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Gitpod.Tool.Commands.Config
{
    class VerifyConfigCommand : Command<VerifyConfigCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("-p|--php")]
            [Description("Verify php settings")]
            [DefaultValue(false)]
            public bool VerifyPhp { get; set; }

            [CommandOption("-s|--services")]
            [Description("Verify services settings")]
            [DefaultValue(false)]
            public bool VerifyServices { get; set; }

            [CommandOption("-S|--shell")]
            [Description("Verify shell script settings")]
            [DefaultValue(false)]
            public bool VerifyShellScripts { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            var configFile = Directory.GetCurrentDirectory() + "/.gpt.yml";

            if (!File.Exists(configFile)) {
                AnsiConsole.MarkupLine("[red]Config file not found. Make sure the file .gpt.yml exists in the current folder.[/]");

                return 0;
            }

            AnsiConsole.WriteLine("Trying to open and parse the config file...");

            GptConfigHelper.ReadConfigFile(true, true);

            if (GptConfigHelper.Config == null) {
                AnsiConsole.MarkupLine("[red]The config object is empty, either the config file has no content or an error appeared during parsing of its content.[/]");

                return 0;
            }

            bool showSingleOutput = false;

            if (settings.VerifyPhp || settings.VerifyServices || settings.VerifyShellScripts) {
                showSingleOutput = true;
            }

            if (!showSingleOutput || settings.VerifyPhp) {
                this.OutputPhpSettings();
            }
            
            if (!showSingleOutput || settings.VerifyServices) {
                this.OutputServiceSettings();
            }

            if (!showSingleOutput || settings.VerifyShellScripts) {
                this.OutputShellScriptSettings();
            }

            return 0;
        }

        private void OutputPhpSettings()
        {
            var rule = new Rule("[red]PHP[/]");
            rule.Justification = Justify.Left;
            AnsiConsole.Write(rule);

            if (GptConfigHelper.Config.Php.Version != String.Empty) {
                // Show php configuration
                AnsiConsole.WriteLine("Version: " + GptConfigHelper.Config.Php.Version);
            }
            
            if (GptConfigHelper.Config.Php.Config.Count > 0) {
                AnsiConsole.WriteLine("Overrides (CLI and Web):");

                // Create a table
                var settingsTable = new Table();

                // Add columns
                settingsTable.AddColumn("Name");
                settingsTable.AddColumn("Value");

                foreach(KeyValuePair<string, string> item in GptConfigHelper.Config.Php.Config) {
                    settingsTable.AddRow(item.Key, item.Value);
                }
                
                // Render the table to the console
                AnsiConsole.Write(settingsTable);
            }

            if (GptConfigHelper.Config.Php.ConfigCLI.Count > 0) {
                AnsiConsole.WriteLine("Overrides CLI:");

                // Create a table
                var settingsTable = new Table();

                // Add columns
                settingsTable.AddColumn("Name");
                settingsTable.AddColumn("Value");

                foreach(KeyValuePair<string, string> item in GptConfigHelper.Config.Php.ConfigCLI) {
                    settingsTable.AddRow(item.Key, item.Value);
                }
                
                // Render the table to the console
                AnsiConsole.Write(settingsTable);
            }

            if (GptConfigHelper.Config.Php.ConfigWeb.Count > 0) {
                AnsiConsole.WriteLine("Overrides Web:");

                // Create a table
                var settingsTable = new Table();

                // Add columns
                settingsTable.AddColumn("Name");
                settingsTable.AddColumn("Value");

                foreach(KeyValuePair<string, string> item in GptConfigHelper.Config.Php.ConfigWeb) {
                    settingsTable.AddRow(item.Key, item.Value);
                }
                
                // Render the table to the console
                AnsiConsole.Write(settingsTable);
            }

            if (GptConfigHelper.Config.Php.Packages.Count > 0) {
                AnsiConsole.WriteLine("Packages:");

                // Create a table
                var settingsTable = new Table();

                // Add columns
                settingsTable.AddColumn("Name");

                foreach(string item in GptConfigHelper.Config.Php.Packages) {
                    settingsTable.AddRow(item);
                }
                
                // Render the table to the console
                AnsiConsole.Write(settingsTable);
            }
        }

        private void OutputServiceSettings()
        {
            var rule = new Rule("[red]Services[/]");
            rule.Justification = Justify.Left;
            AnsiConsole.Write(rule);

            if (GptConfigHelper.Config?.Services?.Active?.Count > 0) {
                AnsiConsole.WriteLine("Settings:");

                // Create a table
                var settingsTable = new Table();

                // Add columns
                settingsTable.AddColumn("Name");

                foreach(string item in GptConfigHelper.Config.Services.Active) {
                    settingsTable.AddRow(item);
                }
                
                // Render the table to the console
                AnsiConsole.Write(settingsTable);
            }
        }

        private void OutputShellScriptSettings()
        {
            var rule = new Rule("[red]Shell scripts[/]");
            rule.Justification = Justify.Left;
            AnsiConsole.Write(rule);

            if (GptConfigHelper.Config.ShellScripts.AdditionalDirectories.Count > 0) {
                AnsiConsole.WriteLine("Additional directories:");

                // Create a table
                var directoriesTable = new Table();

                // Add columns
                directoriesTable.AddColumn("Directory");
                directoriesTable.AddColumn("Exists");
                directoriesTable.AddColumn("Scripts found");

                var currentDir = Directory.GetCurrentDirectory() + "/";

                foreach(string item in GptConfigHelper.Config.ShellScripts.AdditionalDirectories) {
                    bool dirExists = Directory.Exists(currentDir + item);
                    int scriptsFound = 0;

                    if (dirExists) {
                        scriptsFound = Directory.GetFiles(currentDir + item, "*.sh", SearchOption.AllDirectories).Count();
                    }

                    directoriesTable.AddRow(item, dirExists ? "[green1]Yes[/]" : "[red]No[/]", scriptsFound.ToString());
                }
                
                // Render the table to the console
                AnsiConsole.Write(directoriesTable);
            }
        }
    }   
}
