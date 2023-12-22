using System.Collections.Generic;
using Spectre.Console;
using Spectre.Console.Cli;
using Gitpod.Tool.Helper.Internal.Config.Sections;

namespace Gitpod.Tool.Commands.Persist
{
    class ListEntriesCommand : Command<ListEntriesCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            
        }

        public override int Execute(CommandContext context, Settings settings)
        {
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
            } else {
                AnsiConsole.WriteLine("No environment variables have been set via gpt.yml");
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
            } else {
                AnsiConsole.WriteLine("No environment files have been set via gpt.yml");
            }

            
            return 0;
        }
    }
}