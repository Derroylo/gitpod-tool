using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gitpod.Tool.Helper.Internal.Config.Sections;
using Spectre.Console;
using System.Text.RegularExpressions;
using System.IO.Compression;

namespace Gitpod.Tool.Helper.Persist
{
    partial class PersistFolderHelper
    {
        [GeneratedRegex(@"^([a-z0-9-_]+)$")]
        private static partial Regex EnvNameMatchRegex();

        public static void AddFolder()
        {
            bool finished = false;

            do {
                var envFolderShortname = string.Empty;
                bool inputInvalid = false;

                do {
                    inputInvalid = false;

                    envFolderShortname = AnsiConsole.Ask<string>("Shortname for the folder? (Will only be used as entry name in .gpt.yml.): ");

                    if (!EnvNameMatchRegex().Match(envFolderShortname).Success) {
                        AnsiConsole.MarkupLine("[red]The name should only consist of the following characters: a-z 0-9 - _[/]");
                        inputInvalid = true;
                    }

                    if (PersistConfig.Folders.Keys.Contains(envFolderShortname, StringComparer.OrdinalIgnoreCase)) {
                        if (!AnsiConsole.Confirm("An entry with that name already exists. Do you want to replace it?", false)) {
                            inputInvalid = true;
                        }
                    }
                } while (inputInvalid);
                
                var envFolderPath = string.Empty;

                do {
                    inputInvalid = false;

                    envFolderPath = AnsiConsole.Ask<string>("Enter the full path to the folder:");

                    if (!Directory.Exists(envFolderPath)) {
                        AnsiConsole.MarkupLine("[red]The folder could not be found![/]");
                        inputInvalid = true;
                    }

                    long size = 0;

                    var files = Directory.GetFiles(envFolderPath);

                    foreach (string file in files) {
                        size += new FileInfo(file).Length;
                    }

                    if (size > 32768) {
                        AnsiConsole.MarkupLine("[red]The sum of all file sizes needs to be less then 32kb.[/]");
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

                var overwriteFolder = false;

                if (AnsiConsole.Confirm("Should the folder been overwritten if it exists?", false)) {
                    overwriteFolder = true;
                }

                string encodedFolderContent = string.Empty;

                try {
                    ZipFile.CreateFromDirectory(envFolderPath, envFolderShortname + ".zip");

                    var fileContent = File.ReadAllText(envFolderShortname + ".zip");
                    encodedFolderContent = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileContent));

                    File.Delete(envFolderShortname + ".zip");
                } catch (Exception e) {
                    AnsiConsole.WriteException(e);

                    return;
                }

                PersistConfig.Folders.TryGetValue(envFolderShortname, out Dictionary<string, string> existingEntry);

                if (existingEntry != null) {
                    if (existingEntry.ContainsKey("folder")) {
                        existingEntry["folder"] = envFolderPath;
                    } else {
                        existingEntry.Add("folder", envFolderPath);
                    }

                    if (existingEntry.ContainsKey("content") && saveLocation == ".gpt.yml") {
                        existingEntry["content"] = encodedFolderContent;
                    } else if (!existingEntry.ContainsKey("content") && saveLocation == ".gpt.yml") {
                        existingEntry.Add("content", encodedFolderContent);
                    } else if (existingEntry.ContainsKey("content") && saveLocation != ".gpt.yml") {
                        existingEntry.Remove("content");
                    }

                    if (existingEntry.ContainsKey("var") && saveLocation != ".gpt.yml") {
                        existingEntry.Remove("var");
                    }

                    if (existingEntry.ContainsKey("overwrite")) {
                        existingEntry["overwrite"] = overwriteFolder.ToString().ToLower();
                    } else {
                        existingEntry.Add("overwrite", overwriteFolder.ToString().ToLower());
                    }
                } else {
                    var newEntry = new Dictionary<string, string>() {
                        {"folder", envFolderPath},
                        {"overwrite", overwriteFolder.ToString().ToLower()}
                    };

                    if (saveLocation == ".gpt.yml") {
                        newEntry.Add("content", encodedFolderContent);
                    } else {
                        newEntry.Add("var", gpVariable);
                    }

                    PersistConfig.Folders.Add(envFolderShortname, newEntry);
                }

                if (saveLocation != ".gpt.yml") {
                    ExecCommand.Exec("gp env " + gpVariable + "=" + encodedFolderContent, 10);
                }

                // Mark the config as updated, so it will be saved on exiting the application
                PersistConfig.ConfigUpdated = true;

                if (!AnsiConsole.Confirm("Do you want to add more files?", false)) {
                    finished = true;
                }
            } while (!finished);

            AnsiConsole.MarkupLine("[green]The changes have been saved.[/]");
        }

        public static void RestorePersistedFolders(bool debug = false)
        {
            if (PersistConfig.Files.Count == 0) {
                return;
            }

            foreach (KeyValuePair<string, Dictionary<string, string>> entry in PersistConfig.Files) {
                entry.Value.TryGetValue("folder", out string outputFolderName);

                if (outputFolderName == null || outputFolderName == string.Empty) {
                    AnsiConsole.MarkupLine("[red]Missing value \"folder\" for restoring " + entry.Key + "[/]");
                    
                    continue;
                }

                entry.Value.TryGetValue("overwrite", out string overwriteExistingFolder);

                if (Directory.Exists(outputFolderName) && overwriteExistingFolder != null && overwriteExistingFolder == "true") {
                    AnsiConsole.MarkupLine("[red]Folder \"" + overwriteExistingFolder + "\" already exists and overwriting is disabled for restoring " + entry.Key + "[/]");
                }

                entry.Value.TryGetValue("content", out string encodedFileContent);
                entry.Value.TryGetValue("gpVariable", out string gpVariable);

                if (encodedFileContent == null && gpVariable != null) {
                    encodedFileContent = Environment.GetEnvironmentVariable(gpVariable);
                }

                if (encodedFileContent != null && encodedFileContent != string.Empty) {
                    try {
                        string decodedFileString = Encoding.UTF8.GetString(Convert.FromBase64String(encodedFileContent));

                        File.WriteAllText(outputFolderName + ".zip", decodedFileString);

                        ZipFile.ExtractToDirectory(outputFolderName + ".zip", outputFolderName);

                        File.Delete(outputFolderName + ".zip");                        
                    } catch (Exception e) {
                        AnsiConsole.WriteException(e);

                        continue;
                    }
                }
            }
        }
    }
}