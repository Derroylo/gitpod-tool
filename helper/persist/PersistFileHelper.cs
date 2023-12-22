using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gitpod.Tool.Helper.Internal.Config.Sections;
using Spectre.Console;
using System.Text.RegularExpressions;

namespace Gitpod.Tool.Helper.Persist
{
    partial class PersistFileHelper
    {
        [GeneratedRegex(@"^([a-z0-9-_]+)$")]
        private static partial Regex EnvNameMatchRegex();

        public static void AddFile()
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

                    if (PersistConfig.Files.Keys.Contains(envFileShortname, StringComparer.OrdinalIgnoreCase)) {
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

                var overwriteFile = false;

                if (AnsiConsole.Confirm("Should the file been overwritten if it exists?", false)) {
                    overwriteFile = true;
                }

                string encodedFileContent = string.Empty;

                try {
                    var fileContent = File.ReadAllText(envFilePath);
                    encodedFileContent = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileContent));
                } catch (Exception e) {
                    AnsiConsole.WriteException(e);

                    return;
                }

                PersistConfig.Files.TryGetValue(envFileShortname, out Dictionary<string, string> existingEntry);

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

                    if (existingEntry.ContainsKey("overwrite")) {
                        existingEntry["overwrite"] = overwriteFile.ToString().ToLower();
                    } else {
                        existingEntry.Add("overwrite", overwriteFile.ToString().ToLower());
                    }
                } else {
                    var newEntry = new Dictionary<string, string>() {
                        {"file", envFilePath},
                        {"overwrite", overwriteFile.ToString().ToLower()}
                    };

                    if (saveLocation == ".gpt.yml") {
                        newEntry.Add("content", encodedFileContent);
                    } else {
                        newEntry.Add("var", gpVariable);
                    }

                    PersistConfig.Files.Add(envFileShortname, newEntry);
                }

                if (saveLocation != ".gpt.yml") {
                    ExecCommand.Exec("gp env " + gpVariable + "=" + encodedFileContent, 10);
                }

                // Mark the config as updated, so it will be saved on exiting the application
                PersistConfig.ConfigUpdated = true;

                if (!AnsiConsole.Confirm("Do you want to add more files?", false)) {
                    finished = true;
                }
            } while (!finished);

            AnsiConsole.MarkupLine("[green]The changes have been saved.[/]");
        }

        public static void RestorePersistedFiles(bool debug = false)
        {
            if (PersistConfig.Files.Count == 0) {
                return;
            }

            foreach (KeyValuePair<string, Dictionary<string, string>> entry in PersistConfig.Files) {
                entry.Value.TryGetValue("file", out string outputFileName);

                if (outputFileName == null || outputFileName == string.Empty) {
                    AnsiConsole.MarkupLine("[red]Missing value \"file\" for restoring " + entry.Key + "[/]");
                    
                    continue;
                }

                entry.Value.TryGetValue("overwrite", out string overwriteExistingFile);

                if (File.Exists(outputFileName) && overwriteExistingFile != null && overwriteExistingFile == "true") {
                    AnsiConsole.MarkupLine("[red]File \"" + outputFileName + "\" already exists and overwriting is disabled for restoring " + entry.Key + "[/]");
                }

                entry.Value.TryGetValue("content", out string encodedFileContent);
                entry.Value.TryGetValue("gpVariable", out string gpVariable);

                if (encodedFileContent == null && gpVariable != null) {
                    encodedFileContent = Environment.GetEnvironmentVariable(gpVariable);
                }

                if (encodedFileContent != null && encodedFileContent != string.Empty) {
                    try {
                        string decodedFileString = Encoding.UTF8.GetString(Convert.FromBase64String(encodedFileContent));

                        File.WriteAllText(outputFileName, decodedFileString);
                    } catch (Exception e) {
                        AnsiConsole.WriteException(e);

                        continue;
                    }
                }
            }
        }
    }
}