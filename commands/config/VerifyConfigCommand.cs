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

            this.OutputPhpSettings();

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
                AnsiConsole.WriteLine("Settings:");

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
    }   
}
