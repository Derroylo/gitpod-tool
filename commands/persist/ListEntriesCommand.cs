using System.Collections.Generic;
using Spectre.Console;
using Spectre.Console.Cli;
using Gitpod.Tool.Helper.Internal.Config.Sections;
using System.Text;
using System;

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
                AnsiConsole.WriteLine("No persisted environment variables have been set via gpt.yml");
            }

            if (PersistConfig.Files.Count > 0) {
                AnsiConsole.WriteLine("Files:");

                // Create a table
                var envTable = new Table();

                // Add columns
                envTable.AddColumn("Name");
                envTable.AddColumn("Filename");
                envTable.AddColumn("Read from env var");
                envTable.AddColumn("Overwrite");
                envTable.AddColumn("Content");

                foreach(KeyValuePair<string, Dictionary<string, string>> item in PersistConfig.Files) {
                    string filename = string.Empty;
                    string envVariable = string.Empty;
                    string content = string.Empty;
                    string overwrite = string.Empty;

                    foreach (KeyValuePair<string, string> file in item.Value) {
                        if (file.Key == "file") {
                            filename = file.Value;
                        }
                        
                        if (file.Key == "var") {
                            envVariable = file.Value;
                        }

                        if (file.Key == "overwrite") {
                            overwrite = file.Value;
                        }

                        if (file.Key == "content") {
                            content = file.Value;
                        }
                    }

                    envTable.AddRow(item.Key, filename, envVariable, overwrite, Encoding.UTF8.GetString(Convert.FromBase64String(content)));
                }
                
                // Render the table to the console
                AnsiConsole.Write(envTable);
            } else {
                AnsiConsole.WriteLine("No persisted files have been set via gpt.yml");
            }

            if (PersistConfig.Folders.Count > 0) {
                AnsiConsole.WriteLine("Folders:");

                // Create a table
                var envTable = new Table();

                // Add columns
                envTable.AddColumn("Name");
                envTable.AddColumn("Folder");
                envTable.AddColumn("Read from env var");
                envTable.AddColumn("Overwrite");

                foreach(KeyValuePair<string, Dictionary<string, string>> item in PersistConfig.Folders) {
                    string folder = string.Empty;
                    string envVariable = string.Empty;
                    string overwrite = string.Empty;

                    foreach (KeyValuePair<string, string> file in item.Value) {
                        if (file.Key == "folder") {
                            folder = file.Value;
                        }
                        
                        if (file.Key == "var") {
                            envVariable = file.Value;
                        }

                        if (file.Key == "overwrite") {
                            overwrite = file.Value;
                        }
                    }

                    envTable.AddRow(item.Key, folder, envVariable, overwrite);
                }
                
                // Render the table to the console
                AnsiConsole.Write(envTable);
            } else {
                AnsiConsole.WriteLine("No persisted folders have been set via gpt.yml");
            }

            return 0;
        }
    }
}