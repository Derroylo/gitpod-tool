using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Gitpod.Tool.Helper.Internal.Config;
using Gitpod.Tool.Helper.Internal.Config.Sections;
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

            [CommandOption("-n|--nodejs")]
            [Description("Verify NodeJS settings")]
            [DefaultValue(false)]
            public bool VerifyNodeJs { get; set; }

            [CommandOption("-s|--services")]
            [Description("Verify services settings")]
            [DefaultValue(false)]
            public bool VerifyServices { get; set; }

            [CommandOption("-S|--shell")]
            [Description("Verify shell script settings")]
            [DefaultValue(false)]
            public bool VerifyShellScripts { get; set; }

            [CommandOption("-P|--persist")]
            [Description("Verify persist settings")]
            [DefaultValue(false)]
            public bool VerifyPersist { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            if (!ConfigHelper.ConfigFileExists) {
                AnsiConsole.MarkupLine("[red]Config file not found. Make sure the file .gpt.yml exists in the root folder of your project.[/]");

                return 0;
            }

            if (!ConfigHelper.IsConfigFileValid) {
                AnsiConsole.MarkupLine("[red]The config file is invalid. Correct the syntax errors and try again.[/]");

                return 0;
            }

            bool showSingleOutput = false;

            if (settings.VerifyPhp || settings.VerifyServices || settings.VerifyShellScripts || settings.VerifyNodeJs || settings.VerifyPersist) {
                showSingleOutput = true;
            }

            if (!showSingleOutput || settings.VerifyPhp) {
                OutputPhpSettings();
            }
            
            if (!showSingleOutput || settings.VerifyServices) {
                OutputServiceSettings();
            }

            if (!showSingleOutput || settings.VerifyShellScripts) {
                OutputShellScriptSettings();
            }

            if (!showSingleOutput || settings.VerifyNodeJs) {
                OutputNodeJsSettings();
            }

            if (!showSingleOutput || settings.VerifyPersist) {
                OutputPersistSettings();
            }

            return 0;
        }

        private void OutputPhpSettings()
        {
            Rule rule = new() {Title = "[red]PHP[/]", Justification = Justify.Left};
            
            AnsiConsole.Write(rule);

            if (PhpConfig.PhpVersion != string.Empty) {
                // Show php configuration
                AnsiConsole.WriteLine("Version: " + PhpConfig.PhpVersion);
            }
            
            if (PhpConfig.Config.Count > 0) {
                AnsiConsole.WriteLine("Overrides (CLI and Web):");

                // Create a table
                var settingsTable = new Table();

                // Add columns
                settingsTable.AddColumn("Name");
                settingsTable.AddColumn("Value");

                foreach(KeyValuePair<string, string> item in PhpConfig.Config) {
                    settingsTable.AddRow(item.Key, item.Value);
                }
                
                // Render the table to the console
                AnsiConsole.Write(settingsTable);
            }

            if (PhpConfig.ConfigCli.Count > 0) {
                AnsiConsole.WriteLine("Overrides CLI:");

                // Create a table
                var settingsTable = new Table();

                // Add columns
                settingsTable.AddColumn("Name");
                settingsTable.AddColumn("Value");

                foreach(KeyValuePair<string, string> item in PhpConfig.ConfigCli) {
                    settingsTable.AddRow(item.Key, item.Value);
                }
                
                // Render the table to the console
                AnsiConsole.Write(settingsTable);
            }

            if (PhpConfig.ConfigWeb.Count > 0) {
                AnsiConsole.WriteLine("Overrides Web:");

                // Create a table
                var settingsTable = new Table();

                // Add columns
                settingsTable.AddColumn("Name");
                settingsTable.AddColumn("Value");

                foreach(KeyValuePair<string, string> item in PhpConfig.ConfigWeb) {
                    settingsTable.AddRow(item.Key, item.Value);
                }
                
                // Render the table to the console
                AnsiConsole.Write(settingsTable);
            }

            if (PhpConfig.Packages.Count > 0) {
                AnsiConsole.WriteLine("Packages:");

                // Create a table
                var settingsTable = new Table();

                // Add columns
                settingsTable.AddColumn("Name");

                foreach(string item in PhpConfig.Packages) {
                    settingsTable.AddRow(item);
                }
                
                // Render the table to the console
                AnsiConsole.Write(settingsTable);
            }
        }

        private void OutputNodeJsSettings()
        {
            Rule rule = new() {Title = "[red]NodeJS[/]", Justification = Justify.Left};
            
            AnsiConsole.Write(rule);

            if (NodeJsConfig.NodeJsVersion != string.Empty) {
                // Show NodeJS configuration
                AnsiConsole.WriteLine("Version: " + NodeJsConfig.NodeJsVersion);
            }
        }

        private void OutputServiceSettings()
        {
            Rule rule = new() {Title = "[red]Services[/]", Justification = Justify.Left};
            
            AnsiConsole.Write(rule);

            if (ServicesConfig.ActiveServices.Count > 0) {
                AnsiConsole.WriteLine("Settings:");

                // Create a table
                var settingsTable = new Table();

                // Add columns
                settingsTable.AddColumn("Name");

                foreach(string item in ServicesConfig.ActiveServices) {
                    settingsTable.AddRow(item);
                }
                
                // Render the table to the console
                AnsiConsole.Write(settingsTable);
            }
        }

        private void OutputShellScriptSettings()
        {
            Rule rule = new() {Title = "[red]Shell scripts[/]", Justification = Justify.Left};

            AnsiConsole.Write(rule);

            if (ShellScriptConfig.AdditionalDirectories.Count > 0) {
                AnsiConsole.WriteLine("Additional directories:");

                // Create a table
                var directoriesTable = new Table();

                // Add columns
                directoriesTable.AddColumn("Directory");
                directoriesTable.AddColumn("Exists");
                directoriesTable.AddColumn("Scripts found");

                var currentDir = Directory.GetCurrentDirectory() + "/";

                foreach(string item in ShellScriptConfig.AdditionalDirectories) {
                    bool dirExists = Directory.Exists(currentDir + item);
                    int scriptsFound = 0;

                    if (dirExists) {
                        scriptsFound = Directory.GetFiles(currentDir + item, "*.sh", SearchOption.AllDirectories).Length;
                    }

                    directoriesTable.AddRow(item, dirExists ? "[green1]Yes[/]" : "[red]No[/]", scriptsFound.ToString());
                }
                
                // Render the table to the console
                AnsiConsole.Write(directoriesTable);
            }
        }

        private void OutputPersistSettings()
        {
            Rule rule = new() {Title = "[red]Persisted variables/files/folders[/]", Justification = Justify.Left};

            AnsiConsole.Write(rule);

            if (PersistConfig.Variables.Count > 0) {
                AnsiConsole.WriteLine("Variables:");

                // Create a table
                var envTable = new Table();

                // Add columns
                envTable.AddColumn("Name");
                envTable.AddColumn("Value");

                foreach(KeyValuePair<string, string> item in PersistConfig.Variables) {
                    envTable.AddRow(item.Key, item.Value);
                }
                
                // Render the table to the console
                AnsiConsole.Write(envTable);
            }

            if (PersistConfig.Files.Count > 0) {
                AnsiConsole.WriteLine("Files:");

                // Create a table
                var envTable = new Table();

                // Add columns
                envTable.AddColumn("Name");
                envTable.AddColumn("Filename");
                envTable.AddColumn("Read from env var");
                envTable.AddColumn("Content");

                foreach(KeyValuePair<string, Dictionary<string, string>> item in PersistConfig.Files) {
                    string filename = string.Empty;
                    string envVariable = string.Empty;
                    string content = string.Empty;

                    foreach (KeyValuePair<string, string> file in item.Value) {
                        if (file.Key == "file") {
                            filename = file.Value;
                        }
                        
                        if (file.Key == "var") {
                            envVariable = file.Value;
                        }

                        if (file.Key == "content") {
                            content = file.Value;
                        }
                    }

                    envTable.AddRow(item.Key, filename, envVariable, content);
                }
                
                // Render the table to the console
                AnsiConsole.Write(envTable);
            }
        }
    }   
}
