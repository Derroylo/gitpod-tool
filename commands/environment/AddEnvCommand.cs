using System.Collections.Generic;
using Spectre.Console;
using Spectre.Console.Cli;
using Gitpod.Tool.Helper.Internal.Config.Sections;
using System.Linq;
using System.Text.RegularExpressions;
using System;
using System.IO;
using System.Text;
using Gitpod.Tool.Helper;

namespace Gitpod.Tool.Commands.Environment
{
    partial class AddEnvCommand : Command<AddEnvCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            var envTypes = new string[] {"Variable", "File", "Folder"};

            var envType = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the type you want to add")
                    .PageSize(10)
                    .AddChoices(envTypes)
            );

            if (envType == "Variable") {
                AddVariable();
            } else if (envType == "File") {
                AddFile();
            } else if (envType == "Folder") {
                AddFolder();
            }

            return 0;
        }

        [GeneratedRegex(@"^([a-z0-9-_]+)$")]
        private static partial Regex EnvNameMatchRegex();

        private void AddFolder()
        {
            bool finished = false;

            do {
                var envFileShortname = string.Empty;
                bool inputInvalid = false;

                do {
                    inputInvalid = false;

                    envFileShortname = AnsiConsole.Ask<string>("Shortname for the folder? (Will only be used as entry name in .gpt.yml.): ");

                    if (!EnvNameMatchRegex().Match(envFileShortname).Success) {
                        AnsiConsole.MarkupLine("[red]The name should only consist of the following characters: a-z 0-9 - _[/]");
                        inputInvalid = true;
                    }

                    if (EnvironmentConfig.Folders.Keys.Contains(envFileShortname, StringComparer.OrdinalIgnoreCase)) {
                        if (!AnsiConsole.Confirm("An entry with that name already exists. Do you want to replace it?", false)) {
                            inputInvalid = true;
                        }
                    }
                } while (inputInvalid);
                
                var envFolderPath = string.Empty;
                string[] files = null;

                do {
                    inputInvalid = false;

                    envFolderPath = AnsiConsole.Ask<string>("Enter the full path to the folder:");

                    if (!Directory.Exists(envFolderPath)) {
                        AnsiConsole.MarkupLine("[red]The folder could not be found![/]");
                        inputInvalid = true;
                    }

                    files = Directory.GetFiles(envFolderPath, "*", SearchOption.AllDirectories);

                    if (files.Length == 0) {
                        AnsiConsole.MarkupLine("[red]No files found in the folder.[/]");
                        inputInvalid = true;
                    }
                } while (inputInvalid);

                var saveLocations = new string[] {".gpt.yml", "gitpod"};

                var saveLocation = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Where should the content of the file being saved?")
                        .PageSize(10)
                        .AddChoices(saveLocations)
                );

                var gpVariable = string.Empty;

                if (saveLocation == "gitpod") {
                    do {
                        inputInvalid = false;

                        gpVariable = AnsiConsole.Ask<string>("What is the name of the variable?");

                        if (!EnvNameMatchRegex().Match(gpVariable).Success) {
                            AnsiConsole.MarkupLine("[red]The variable name should only consist of the following characters: a-z 0-9 - _[/]");
                            inputInvalid = true;
                        }
                    } while (inputInvalid);
                }

                EnvironmentConfig.Folders.TryGetValue(envFileShortname, out Dictionary<string, Dictionary<string, string>> existingEntry);

                foreach (string file in files) {
                    string encodedFileContent = string.Empty;

                    try {
                        var fileContent = File.ReadAllText(file);
                        encodedFileContent = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileContent));
                    } catch (Exception e) {
                        AnsiConsole.WriteException(e);
                    }

                    /*if (existingEntry != null) {
                        if (existingEntry.ContainsKey("file")) {
                            existingEntry["file"] = envFilePath;
                        } else {
                            existingEntry.Add("file", envFilePath);
                        }

                        if (existingEntry.ContainsKey("content") && saveLocation == ".gpt.yml") {
                            existingEntry["content"] = encodedFileContent;
                        } else if (!existingEntry.ContainsKey("content") && saveLocation == ".gpt.yml") {
                            existingEntry.Add("content", encodedFileContent);
                        } else if (existingEntry.ContainsKey("content") && saveLocation != ".gpt.yml") {
                            existingEntry.Remove("content");
                        }

                        if (existingEntry.ContainsKey("var") && saveLocation != ".gpt.yml") {
                            existingEntry.Remove("var");
                        }
                    } else {
                        var newEntry = new Dictionary<string, string>() {
                            {"file", envFilePath}
                        };

                        if (saveLocation == ".gpt.yml") {
                            newEntry.Add("content", encodedFileContent);
                        } else {
                            newEntry.Add("var", gpVariable);
                        }

                        EnvironmentConfig.Files.Add(envFileShortname, newEntry);
                    }

                    if (saveLocation != ".gpt.yml") {
                        ExecCommand.Exec("gp env " + gpVariable + "=" + encodedFileContent, 10);
                    }*/
                }

                // Mark the config as updated, so it will be saved on exiting the application
                EnvironmentConfig.ConfigUpdated = true;

                if (!AnsiConsole.Confirm("Do you want to add more files?", false)) {
                    finished = true;
                }
            } while (!finished);

            AnsiConsole.MarkupLine("[green]The changes have been saved.[/]");
        }

        private void AddFile()
        {
            bool finished = false;

            do {
                var envFileShortname = string.Empty;
                bool inputInvalid = false;

                do {
                    inputInvalid = false;

                    envFileShortname = AnsiConsole.Ask<string>("Shortname for the file? (Will only be used as entry name in .gpt.yml.): ");

                    if (!EnvNameMatchRegex().Match(envFileShortname).Success) {
                        AnsiConsole.MarkupLine("[red]The name should only consist of the following characters: a-z 0-9 - _[/]");
                        inputInvalid = true;
                    }

                    if (EnvironmentConfig.Files.Keys.Contains(envFileShortname, StringComparer.OrdinalIgnoreCase)) {
                        if (!AnsiConsole.Confirm("An entry with that name already exists. Do you want to replace it?", false)) {
                            inputInvalid = true;
                        }
                    }
                } while (inputInvalid);
                
                var envFilePath = string.Empty;
                FileInfo fileInfo = null;

                do {
                    inputInvalid = false;

                    envFilePath = AnsiConsole.Ask<string>("Enter the full path to the file:");

                    if (!File.Exists(envFilePath)) {
                        AnsiConsole.MarkupLine("[red]The file could not be found![/]");
                        inputInvalid = true;
                    }

                    fileInfo = new FileInfo(envFilePath);

                    if (fileInfo.Length > 32768) {
                        AnsiConsole.MarkupLine("[red]The file needs to be less then 32kb in size.[/]");
                        inputInvalid = true;
                    }
                } while (inputInvalid);

                var saveLocations = new string[] {".gpt.yml", "gitpod"};

                var saveLocation = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Where should the content of the file being saved?")
                        .PageSize(10)
                        .AddChoices(saveLocations)
                );

                var gpVariable = string.Empty;

                if (saveLocation == "gitpod") {
                    do {
                        inputInvalid = false;

                        gpVariable = AnsiConsole.Ask<string>("What is the name of the variable?");

                        if (!EnvNameMatchRegex().Match(gpVariable).Success) {
                            AnsiConsole.MarkupLine("[red]The variable name should only consist of the following characters: a-z 0-9 - _[/]");
                            inputInvalid = true;
                        }
                    } while (inputInvalid);
                }

                string encodedFileContent = string.Empty;

                try {
                    var fileContent = File.ReadAllText(envFilePath);
                    encodedFileContent = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileContent));
                } catch (Exception e) {
                    AnsiConsole.WriteException(e);
                }

                EnvironmentConfig.Files.TryGetValue(envFileShortname, out Dictionary<string, string> existingEntry);

                if (existingEntry != null) {
                    if (existingEntry.ContainsKey("file")) {
                        existingEntry["file"] = envFilePath;
                    } else {
                        existingEntry.Add("file", envFilePath);
                    }

                    if (existingEntry.ContainsKey("content") && saveLocation == ".gpt.yml") {
                        existingEntry["content"] = encodedFileContent;
                    } else if (!existingEntry.ContainsKey("content") && saveLocation == ".gpt.yml") {
                        existingEntry.Add("content", encodedFileContent);
                    } else if (existingEntry.ContainsKey("content") && saveLocation != ".gpt.yml") {
                        existingEntry.Remove("content");
                    }

                    if (existingEntry.ContainsKey("var") && saveLocation != ".gpt.yml") {
                        existingEntry.Remove("var");
                    }
                } else {
                    var newEntry = new Dictionary<string, string>() {
                        {"file", envFilePath}
                    };

                    if (saveLocation == ".gpt.yml") {
                        newEntry.Add("content", encodedFileContent);
                    } else {
                        newEntry.Add("var", gpVariable);
                    }

                    EnvironmentConfig.Files.Add(envFileShortname, newEntry);
                }

                if (saveLocation != ".gpt.yml") {
                    ExecCommand.Exec("gp env " + gpVariable + "=" + encodedFileContent, 10);
                }

                // Mark the config as updated, so it will be saved on exiting the application
                EnvironmentConfig.ConfigUpdated = true;

                if (!AnsiConsole.Confirm("Do you want to add more files?", false)) {
                    finished = true;
                }
            } while (!finished);

            AnsiConsole.MarkupLine("[green]The changes have been saved.[/]");
        }

        private void AddVariable()
        {
            AnsiConsole.MarkupLine("[red]Don´t save any sensible informations via this command![/]");
            AnsiConsole.MarkupLine("[red]For sensible data, use the gp env command[/]");

            bool finished = false;

            do {
                var envVarName = string.Empty;
                bool varNameInvalid = false;

                do {
                    varNameInvalid = false;

                    envVarName = AnsiConsole.Ask<string>("What is the name of the variable?");

                    if (!EnvNameMatchRegex().Match(envVarName).Success) {
                        AnsiConsole.MarkupLine("[red]The variable name should only consist of the following characters: a-z 0-9 - _[/]");
                        varNameInvalid = true;
                    }
                } while (varNameInvalid);
                
                var envVarContent = AnsiConsole.Ask<string>("What is the content of the variable?");

                if (EnvironmentConfig.Variables.Keys.Contains(envVarName, StringComparer.OrdinalIgnoreCase)) {
                    if (AnsiConsole.Confirm("A variable with that name already exists. Do you want to replace it?", false)) {
                        var key = EnvironmentConfig.Variables.Keys.FirstOrDefault(x => x.Equals(envVarName, StringComparison.OrdinalIgnoreCase));

                        EnvironmentConfig.Variables[key] = envVarContent;
                        EnvironmentConfig.ConfigUpdated = true;

                        AnsiConsole.MarkupLine("[green]The variable has been update.[/]");
                    }
                } else {
                    EnvironmentConfig.Variables.Add(envVarName, envVarContent);
                    EnvironmentConfig.ConfigUpdated = true;

                    AnsiConsole.MarkupLine("[green]The variable has been added.[/]");
                }
                
                if (!AnsiConsole.Confirm("Do you want to add more variables?", false)) {
                    finished = true;
                }
            } while (!finished);
        }
    }
}