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
                if (!isExistingEntry) {
                    AnsiConsole.MarkupLine("This tool provides different options to persist folders.");
                    AnsiConsole.MarkupLine("yaml:");
                    AnsiConsole.MarkupLine("The content of the folder will be zipped and it´s content will be added to .gpt.yml");
                    AnsiConsole.MarkupLine("Works only for content smaller then 32kb, can be reused in different workspaces and must be manually updated.");
                    AnsiConsole.WriteLine("");
                    AnsiConsole.MarkupLine("gitpod:");
                    AnsiConsole.MarkupLine("The content of the folder will be zipped and it´s content will be saved as gitpod env variable");
                    AnsiConsole.MarkupLine("Works only for content smaller then 32kb, can be reused in different workspaces and must be manually updated.");
                    AnsiConsole.WriteLine("");
                    AnsiConsole.MarkupLine("symlink:");
                    AnsiConsole.MarkupLine("The original content of the folder will be moved to the /workspace/.gpt folder and a symlink wil be created.");
                    AnsiConsole.MarkupLine("Works for any size of contents but the content is only available in this workspace.");
                    AnsiConsole.WriteLine("");

                    var saveLocations = new string[] {"yaml", "gitpod", "symlink"};

                    folderType.Method = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("How do you want to persist the folder?")
                            .PageSize(10)
                            .AddChoices(saveLocations)
                    );
                }

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

                    if (!inputInvalid && folderType.Method == "symlink") {
                        var dirInfo = new DirectoryInfo(folderType.Folder);

                        if (dirInfo.FullName.StartsWith("/workspace/")) {
                            AnsiConsole.MarkupLine("[red]All folders within the workspace path are already being persisted between restarts![/]");
                            inputInvalid = true;
                        }
                    }

                    if (!inputInvalid && folderType.Method != "symlink") {
                        long size = 0;

                        var files = Directory.GetFiles(folderType.Folder);

                        foreach (string file in files) {
                            size += new FileInfo(file).Length;
                        }

                        if (size > 32768) {
                            AnsiConsole.MarkupLine("[red]The sum of all file sizes needs to be less then 32kb.[/]");
                            inputInvalid = true;
                        }
                    }
                } while (inputInvalid);

                if (!isExistingEntry) {
                    if (folderType.Method == "gitpod") {
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

                    if (folderType.Method != "symlink" && AnsiConsole.Confirm("Should the folder been overwritten if it exists?", false)) {
                        folderType.Overwrite = true;
                    } else if (folderType.Method == "symlink") {
                        folderType.Overwrite = null;
                    }
                }

                if (folderType.Method != "symlink") {
                    try {
                        ZipFile.CreateFromDirectory(folderType.Folder, folderType.Name + ".zip");

                        var fileContent = File.ReadAllText(folderType.Name + ".zip");
                        folderType.Content = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileContent));

                        File.Delete(folderType.Name + ".zip");
                    } catch (Exception e) {
                        AnsiConsole.WriteException(e);

                        return;
                    }
                }

                if (isExistingEntry || PersistConfig.Folders.ContainsKey(folderType.Name)) {
                    PersistConfig.Folders[folderType.Name] = folderType.ToDictionary();
                } else {
                    PersistConfig.Folders.Add(folderType.Name, folderType.ToDictionary());
                }

                if (folderType.Method == "gitpod") {
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
            if (PersistConfig.Folders.Count == 0) {
                return;
            }

            var persistedFolderType = new PersistFolderType();

            foreach (KeyValuePair<string, Dictionary<string, string>> entry in PersistConfig.Folders) {
                persistedFolderType = PersistFolderType.FromDictionary(entry.Key, entry.Value);

                if (persistedFolderType.Folder == null || persistedFolderType.Folder == string.Empty) {
                    AnsiConsole.MarkupLine("[red]Missing value \"folder\" for restoring " + entry.Key + "[/]");
                    
                    continue;
                }

                if (persistedFolderType.Method != "symlink" && Directory.Exists(persistedFolderType.Folder) && persistedFolderType.Overwrite != true) {
                    AnsiConsole.MarkupLine("[red]Folder \"" + persistedFolderType.Folder + "\" already exists and overwriting is disabled for restoring " + entry.Key + "[/]");

                    continue;
                }

                if (persistedFolderType.Content == null && persistedFolderType.GpVarName != null) {
                    persistedFolderType.Content = Environment.GetEnvironmentVariable(persistedFolderType.GpVarName);
                }

                if (persistedFolderType.Method != "symlink" && persistedFolderType.Content != null && persistedFolderType.Content != string.Empty) {
                    try {
                        string decodedFileString = Encoding.UTF8.GetString(Convert.FromBase64String(persistedFolderType.Content));

                        File.WriteAllText(persistedFolderType.Folder + ".zip", decodedFileString);

                        ZipFile.ExtractToDirectory(persistedFolderType.Folder + ".zip", persistedFolderType.Folder);

                        File.Delete(persistedFolderType.Folder + ".zip");                        
                    } catch (Exception e) {
                        AnsiConsole.WriteException(e);

                        continue;
                    }
                } else if (persistedFolderType.Method == "symlink") {
                    var sourceDirInfo = new DirectoryInfo(persistedFolderType.Folder);
                    var targetDirInfo = new DirectoryInfo("/workspace/.gpt/persisted/" + sourceDirInfo.Name);

                    if (sourceDirInfo.LinkTarget != null && sourceDirInfo.LinkTarget != "") {
                        AnsiConsole.MarkupLine("[red]The given folder for " + entry.Key + " is already a symlink.[/]");
                    
                        continue;
                    }                   

                    if (sourceDirInfo.LinkTarget == null && !sourceDirInfo.Exists) {
                        AnsiConsole.MarkupLine("[red]The given folder for " + entry.Key + " doesn´t exists.[/]");
                    
                        continue;
                    }

                    if (!targetDirInfo.Exists) {
                        targetDirInfo.Create();

                        var files = sourceDirInfo.GetFiles();

                        foreach (FileInfo file in files) {
                            try {
                                file.CopyTo("/workspace/.gpt/persisted/" + sourceDirInfo.Name + "/" + file.Name);
                            } catch (Exception e) {
                                AnsiConsole.MarkupLine("[red]An error occurred during copying file \"" + file.FullName + "\" to " + "/workspace/.gpt/persisted/" + sourceDirInfo.Name + "/" + file.Name + "[/]");
                                AnsiConsole.WriteException(e);

                                continue;
                            }
                        }
                    }

                    try {
                        sourceDirInfo.Delete(true);
                    } catch (Exception e) {
                        AnsiConsole.MarkupLine("[red]An error occurred deletion of folder \"" + sourceDirInfo.FullName + "\"[/]");
                        AnsiConsole.WriteException(e);

                        continue;
                    }
                    
                    try {
                        sourceDirInfo.CreateAsSymbolicLink("/workspace/.gpt/persisted/" + sourceDirInfo.Name);
                    } catch (Exception e) {
                        AnsiConsole.MarkupLine("[red]An error occurred trying to create a symlink of folder \"" + sourceDirInfo.FullName + "\" to \"" + "/workspace/.gpt/persisted/" + sourceDirInfo.Name + "\"[/]");
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