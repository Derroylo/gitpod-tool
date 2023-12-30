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
                if (!isExistingEntry) {
                    AnsiConsole.MarkupLine("This tool provides different options to persist files.");
                    AnsiConsole.MarkupLine("yaml:");
                    AnsiConsole.MarkupLine("The content of the file will be added to .gpt.yml");
                    AnsiConsole.MarkupLine("Works only for content smaller then 32kb, can be reused in different workspaces and must be manually updated.");
                    AnsiConsole.WriteLine("");
                    AnsiConsole.MarkupLine("gitpod:");
                    AnsiConsole.MarkupLine("The content of the file will be saved as gitpod env variable");
                    AnsiConsole.MarkupLine("Works only for content smaller then 32kb, can be reused in different workspaces and must be manually updated.");
                    AnsiConsole.WriteLine("");
                    AnsiConsole.MarkupLine("symlink:");
                    AnsiConsole.MarkupLine("The original content of the file will be moved to the /workspace/.gpt folder and a symlink wil be created.");
                    AnsiConsole.MarkupLine("Works for any size of contents but the content is only available in this workspace.");
                    AnsiConsole.WriteLine("");

                    var saveLocations = new string[] {"yaml", "gitpod", "symlink"};

                    fileType.Method = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("How do you want to persist the file?")
                            .PageSize(10)
                            .AddChoices(saveLocations)
                    );
                }

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

                    if (!inputInvalid && fileType.Method == "symlink" && fileInfo.FullName.StartsWith("/workspace/")) {
                        AnsiConsole.MarkupLine("[red]All files within the workspace path are already being persisted between restarts![/]");
                        inputInvalid = true;
                    }

                    if (!inputInvalid && fileType.Method != "symlink" && fileInfo.Length > 32768) {
                        AnsiConsole.MarkupLine("[red]The file needs to be less then 32kb in size.[/]");
                        inputInvalid = true;
                    }
               } while (inputInvalid);

                if (!isExistingEntry) {
                    if (fileType.Method == "gitpod") {
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

                    if (fileType.Method != "symlink" && AnsiConsole.Confirm("Should the file been overwritten if it exists?", false)) {
                        fileType.Overwrite = true;
                    } else if (fileType.Method == "symlink") {
                        fileType.Overwrite = null;
                    }
                }

                if (fileType.Method != "symlink") {
                    try {
                        var fileContent = File.ReadAllText(fileType.File);
                        fileType.Content = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileContent));
                    } catch (Exception e) {
                        AnsiConsole.WriteException(e);

                        return;
                    }
                }

                if (isExistingEntry || PersistConfig.Files.ContainsKey(fileType.Name)) {
                    PersistConfig.Files[fileType.Name] = fileType.ToDictionary();
                } else {
                    PersistConfig.Files.Add(fileType.Name, fileType.ToDictionary());
                }

                if (fileType.Method == "gitpod") {
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

            var persistedFileType = new PersistFileType();

            foreach (KeyValuePair<string, Dictionary<string, string>> entry in PersistConfig.Files) {
                persistedFileType = PersistFileType.FromDictionary(entry.Key, entry.Value);

                if (persistedFileType.File == null || persistedFileType.File == string.Empty) {
                    AnsiConsole.MarkupLine("[red]Missing value \"file\" for restoring " + entry.Key + "[/]");
                    
                    continue;
                }

                if (persistedFileType.Method != "symlink" && File.Exists(persistedFileType.File) && persistedFileType.Overwrite != true) {
                    AnsiConsole.MarkupLine("[red]File \"" + persistedFileType.File + "\" already exists and overwriting is disabled for restoring " + entry.Key + "[/]");

                    continue;
                }

                if (persistedFileType.Content == null && persistedFileType.GpVarName != null) {
                    persistedFileType.Content = Environment.GetEnvironmentVariable(persistedFileType.GpVarName);
                }

                if (persistedFileType.Method != "symlink" && persistedFileType.Content != null && persistedFileType.Content != string.Empty) {
                    try {
                        string decodedFileString = Encoding.UTF8.GetString(Convert.FromBase64String(persistedFileType.Content));

                        File.WriteAllText(persistedFileType.File, decodedFileString);
                    } catch (Exception e) {
                        AnsiConsole.WriteException(e);

                        continue;
                    }
                }  else if (persistedFileType.Method == "symlink") {
                    var sourceFileInfo = new FileInfo(persistedFileType.File);
                    var targetFileInfo = new FileInfo("/workspace/.gpt/persisted/files/" + entry.Key + "/" + sourceFileInfo.Name);
                    var targetDirInfo = new DirectoryInfo("/workspace/.gpt/persisted/files/" + entry.Key + "/");
                   
                    if (sourceFileInfo.LinkTarget != null && sourceFileInfo.LinkTarget != "") {
                        AnsiConsole.MarkupLine("[red]The given file for " + entry.Key + " is already a symlink.[/]");
                    
                        continue;
                    }

                    if (sourceFileInfo.LinkTarget == null && !sourceFileInfo.Exists) {
                        AnsiConsole.MarkupLine("[red]The given file for " + entry.Key + " doesn´t exists.[/]");
                    
                        continue;
                    }

                    if (!targetDirInfo.Exists) {
                        targetDirInfo.Create();
                    }

                    if (!targetFileInfo.Exists) {
                        try {
                            sourceFileInfo.CopyTo(targetFileInfo.FullName);
                        } catch (Exception e) {
                            AnsiConsole.MarkupLine("[red]An error occurred during copying file \"" + sourceFileInfo.FullName + "\" to " + "\"" + targetFileInfo.FullName + "\"[/]");
                            AnsiConsole.WriteException(e);

                            continue;
                        }
                    }

                    try {
                        sourceFileInfo.Delete();
                    } catch (Exception e) {
                        AnsiConsole.MarkupLine("[red]An error occurred deletion of file \"" + sourceFileInfo.FullName + "\"[/]");
                        AnsiConsole.WriteException(e);

                        continue;
                    }
                    
                    try {
                        sourceFileInfo.CreateAsSymbolicLink(targetFileInfo.FullName);
                    } catch (Exception e) {
                        AnsiConsole.MarkupLine("[red]An error occurred trying to create a symlink of file \"" + sourceFileInfo.FullName + "\" to \"" + targetDirInfo.FullName + "\"[/]");
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