using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gitpod.Tool.Helper.Internal.Config.Sections;
using Spectre.Console;
using System.Text.RegularExpressions;
using Gitpod.Tool.Helper.Internal.Config.Sections.Types;

namespace Gitpod.Tool.Helper.Persist
{
    partial class PersistFileHelper
    {
        [GeneratedRegex(@"^([a-z0-9-_]+)$")]
        private static partial Regex EnvNameMatchRegex();

        public static void AddFile(PersistFileType fileType = null)
        {
            bool finished = false;
            bool isExistingEntry = fileType != null;

            if (fileType == null) {
                fileType = new PersistFileType();
            }

            do {
                bool inputInvalid = false;

                if (fileType.Name == null) {
                    do {
                        inputInvalid = false;

                        fileType.Name = AnsiConsole.Ask<string>("Shortname for the file? (Will only be used as entry name in .gpt.yml.): ");

                        if (!EnvNameMatchRegex().Match(fileType.Name).Success) {
                            AnsiConsole.MarkupLine("[red]The name should only consist of the following characters: a-z 0-9 - _[/]");
                            inputInvalid = true;
                        }

                        if (PersistConfig.Files.Keys.Contains(fileType.Name, StringComparer.OrdinalIgnoreCase)) {
                            if (!AnsiConsole.Confirm("An entry with that name already exists. Do you want to replace it?", false)) {
                                inputInvalid = true;
                            }
                        }
                    } while (inputInvalid);
                }
                
                FileInfo fileInfo = null;

                do {
                    inputInvalid = false;

                    if (fileType.File == null) {
                        fileType.File = AnsiConsole.Ask<string>("Enter the full path to the file:");
                    }

                    if (!File.Exists(fileType.File)) {
                        AnsiConsole.MarkupLine("[red]The file could not be found![/]");
                        inputInvalid = true;
                    }

                    fileInfo = new FileInfo(fileType.File);

                    if (fileInfo.Length > 32768) {
                        AnsiConsole.MarkupLine("[red]The file needs to be less then 32kb in size.[/]");
                        inputInvalid = true;
                    }
                } while (inputInvalid);

                string saveLocation = "";

                if (!isExistingEntry) {
                    var saveLocations = new string[] {".gpt.yml", "gitpod"};

                    saveLocation = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Where should the content of the file being saved?")
                            .PageSize(10)
                            .AddChoices(saveLocations)
                    );

                    if (saveLocation == "gitpod") {
                        do {
                            inputInvalid = false;

                            fileType.GpVarName = AnsiConsole.Ask<string>("What is the name of the variable?");

                            if (!EnvNameMatchRegex().Match(fileType.GpVarName).Success) {
                                AnsiConsole.MarkupLine("[red]The variable name should only consist of the following characters: a-z 0-9 - _[/]");
                                inputInvalid = true;
                            }
                        } while (inputInvalid);
                    }

                    fileType.Overwrite = false;

                    if (AnsiConsole.Confirm("Should the file been overwritten if it exists?", false)) {
                        fileType.Overwrite = true;
                    }
                } else {
                    saveLocation = fileType.GpVarName == null ? ".gpt.yml" : "gitpod";
                }

                try {
                    var fileContent = File.ReadAllText(fileType.File);
                    fileType.Content = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileContent));
                } catch (Exception e) {
                    AnsiConsole.WriteException(e);

                    return;
                }

                if (isExistingEntry || PersistConfig.Files.ContainsKey(fileType.Name)) {
                    PersistConfig.Files[fileType.Name] = fileType.ToDictionary();
                } else {
                    PersistConfig.Files.Add(fileType.Name, fileType.ToDictionary());
                }

                if (saveLocation != ".gpt.yml") {
                    ExecCommand.Exec("gp env " + fileType.GpVarName + "=" + fileType.Content, 10);
                }

                // Mark the config as updated, so it will be saved on exiting the application
                PersistConfig.ConfigUpdated = true;

                if (!isExistingEntry && !AnsiConsole.Confirm("Do you want to add more files?", false)) {
                    finished = true;
                } else if (isExistingEntry) {
                    finished = true;
                } else {
                    fileType = new PersistFileType();
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

        public static void DeleteFile()
        {
            if (PersistConfig.Files.Count == 0) {
                AnsiConsole.WriteLine("No persisted files have been set via gpt.yml");

                return;
            }

            var files = PersistConfig.Files.Keys.ToArray<string>();

            var file = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the file you want to delete")
                    .PageSize(10)
                    .AddChoices(files)
            );

            PersistConfig.Files.Remove(file);
            PersistConfig.ConfigUpdated = true;

            AnsiConsole.MarkupLine("[green]The changes have been saved.[/]");
        }

        public static void UpdateFile()
        {
            if (PersistConfig.Files.Count == 0) {
                AnsiConsole.WriteLine("No persisted files have been set via gpt.yml");

                return;
            }

            var files = PersistConfig.Files.Keys.ToArray<string>();

            var file = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the variable you want to update")
                    .PageSize(10)
                    .AddChoices(files)
            );

            var fileType = PersistFileType.FromDictionary(file, PersistConfig.Files[file]);

            PersistFileHelper.AddFile(fileType);
        }
    }
}