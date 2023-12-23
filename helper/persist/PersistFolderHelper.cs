using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gitpod.Tool.Helper.Internal.Config.Sections;
using Spectre.Console;
using System.Text.RegularExpressions;
using System.IO.Compression;
using Gitpod.Tool.Helper.Internal.Config.Sections.Types;

namespace Gitpod.Tool.Helper.Persist
{
    partial class PersistFolderHelper
    {
        [GeneratedRegex(@"^([a-z0-9-_]+)$")]
        private static partial Regex EnvNameMatchRegex();

        public static void AddFolder(PersistFolderType folderType = null)
        {
            bool finished = false;

            bool isExistingEntry = folderType != null;

            if (folderType == null) {
                folderType = new PersistFolderType();
            }

            do {
                bool inputInvalid = false;

                if (folderType.Name == null) {
                    do {
                        inputInvalid = false;

                        folderType.Name = AnsiConsole.Ask<string>("Shortname for the folder? (Will only be used as entry name in .gpt.yml.): ");

                        if (!EnvNameMatchRegex().Match(folderType.Name).Success) {
                            AnsiConsole.MarkupLine("[red]The name should only consist of the following characters: a-z 0-9 - _[/]");
                            inputInvalid = true;
                        }

                        if (PersistConfig.Folders.Keys.Contains(folderType.Name, StringComparer.OrdinalIgnoreCase)) {
                            if (!AnsiConsole.Confirm("An entry with that name already exists. Do you want to replace it?", false)) {
                                inputInvalid = true;
                            }
                        }
                    } while (inputInvalid);
                }
                
                do {
                    inputInvalid = false;

                    if (folderType.Folder == null) {
                        folderType.Folder = AnsiConsole.Ask<string>("Enter the full path to the folder:");
                    }

                    if (!Directory.Exists(folderType.Folder)) {
                        AnsiConsole.MarkupLine("[red]The folder could not be found![/]");
                        inputInvalid = true;
                    }

                    long size = 0;

                    var files = Directory.GetFiles(folderType.Folder);

                    foreach (string file in files) {
                        size += new FileInfo(file).Length;
                    }

                    if (size > 32768) {
                        AnsiConsole.MarkupLine("[red]The sum of all file sizes needs to be less then 32kb.[/]");
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

                            folderType.GpVarName = AnsiConsole.Ask<string>("What is the name of the variable?");

                            if (!EnvNameMatchRegex().Match(folderType.GpVarName).Success) {
                                AnsiConsole.MarkupLine("[red]The variable name should only consist of the following characters: a-z 0-9 - _[/]");
                                inputInvalid = true;
                            }
                        } while (inputInvalid);
                    }

                    folderType.Overwrite = false;

                    if (AnsiConsole.Confirm("Should the folder been overwritten if it exists?", false)) {
                        folderType.Overwrite = true;
                    }
                }

                try {
                    ZipFile.CreateFromDirectory(folderType.Folder, folderType.Name + ".zip");

                    var fileContent = File.ReadAllText(folderType.Name + ".zip");
                    folderType.Content = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileContent));

                    File.Delete(folderType.Name + ".zip");
                } catch (Exception e) {
                    AnsiConsole.WriteException(e);

                    return;
                }

                if (isExistingEntry || PersistConfig.Folders.ContainsKey(folderType.Name)) {
                    PersistConfig.Folders[folderType.Name] = folderType.ToDictionary();
                } else {
                    PersistConfig.Folders.Add(folderType.Name, folderType.ToDictionary());
                }

                if (saveLocation != ".gpt.yml") {
                    ExecCommand.Exec("gp env " + folderType.Name + "=" + folderType.Content, 10);
                }

                // Mark the config as updated, so it will be saved on exiting the application
                PersistConfig.ConfigUpdated = true;

                if (!isExistingEntry && !AnsiConsole.Confirm("Do you want to add more folders?", false)) {
                    finished = true;
                } else if (isExistingEntry) {
                    finished = true;
                } else {
                    folderType = new PersistFolderType();
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

        public static void DeleteFolder()
        {
            if (PersistConfig.Folders.Count == 0) {
                AnsiConsole.WriteLine("No persisted folders have been set via gpt.yml");

                return;
            }

            var folders = PersistConfig.Folders.Keys.ToArray<string>();

            var folder = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the folder you want to delete")
                    .PageSize(10)
                    .AddChoices(folders)
            );

            PersistConfig.Folders.Remove(folder);
            PersistConfig.ConfigUpdated = true;

            AnsiConsole.MarkupLine("[green]The changes have been saved.[/]");
        }

        public static void UpdateFolder()
        {
            if (PersistConfig.Folders.Count == 0) {
                AnsiConsole.WriteLine("No persisted folders have been set via gpt.yml");

                return;
            }

            var folders = PersistConfig.Folders.Keys.ToArray<string>();

            var folder = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the folder you want to update")
                    .PageSize(10)
                    .AddChoices(folders)
            );

            var folderType = PersistFolderType.FromDictionary(folder, PersistConfig.Folders[folder]);

            PersistFolderHelper.AddFolder(folderType);
        }
    }
}