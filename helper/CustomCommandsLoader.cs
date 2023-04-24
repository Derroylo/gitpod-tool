using System;
using System.IO;
using System.Collections.Generic;
using Gitpod.Tool.Classes;
using Spectre.Console;
using System.ComponentModel.Design.Serialization;
using System.Text.RegularExpressions;

namespace Gitpod.Tool.Helper
{
    class CustomCommandsLoader
    {
        public static Dictionary<string, CustomBranch> Load()
        {           
            try {
                return CustomCommandsLoader.SearchForShellScripts(".devEnv/gitpod/scripts");
            } catch(Exception ex) {
                AnsiConsole.MarkupLine("[red]An exception occured during loading of custom commands[/]");
                AnsiConsole.WriteException(ex);

                return null;
            }
        }

        private static Dictionary<string, CustomBranch> SearchForShellScripts(string folder, Dictionary<string, CustomBranch> commands = null)
        {
            if (commands == null) {
                var defaultBranch = new CustomBranch("default");
                commands = new Dictionary<string, CustomBranch>();

                // Add the default branch (commands that have no specified branch)
                commands.Add(defaultBranch.Name, defaultBranch);
            }

            if (!Directory.Exists(folder)) {
                return commands;
            }

            string[] files = Directory.GetFiles(folder);
            foreach (string file in files) {
                if (file.ToLower().Substring(file.Length - 3) != ".sh") {
                    continue;
                }

                var shellScriptSettings = CustomCommandsLoader.ProcessShellScript(file);
                
                var newCustomCommand = new CustomCommand(shellScriptSettings.Command, file, shellScriptSettings.Description, shellScriptSettings.Arguments);

                if (shellScriptSettings != null && shellScriptSettings.Branch != String.Empty) {
                    if (commands.ContainsKey(shellScriptSettings.Branch)) {
                        commands[shellScriptSettings.Branch].Commands.Add(newCustomCommand);
                    } else {
                        var newBranch = new CustomBranch(shellScriptSettings.Branch, shellScriptSettings.BranchDescription);
                        newBranch.Commands.Add(newCustomCommand);

                        commands.Add(newBranch.Name, newBranch);
                    }
                } else if (shellScriptSettings != null) {
                    commands["default"].Commands.Add(newCustomCommand);
                }
            }

            // Check for subdirectories
            string [] subDirectories = Directory.GetDirectories(folder);
            foreach(string subdirectory in subDirectories) {
                commands = SearchForShellScripts(subdirectory, commands);
            }
            
            return commands;
        }

        private static ShellScriptSettings ProcessShellScript(string fileWithPath)
        {
            string[] lines = File.ReadAllLines(fileWithPath);

            string command = String.Empty;
            string description = String.Empty;
            string branch = String.Empty;
            string branchDescription = String.Empty;
            List<string> args = new List<string>();

            string commandPattern =  @"\# gptCommand: ([a-zA-Z0-9-_]+)";
            string descriptionPattern =  @"\# gptDescription: ([a-zA-Z0-9-_ ,]+)";
            string branchPattern =  @"\# gptBranch: ([a-zA-Z0-9-_]+)";
            string branchDescriptionPattern =  @"\# gptBranchDescription: ([a-zA-Z0-9-_ ,]+)";
            string argsPattern =  @"\# gptArgument: ([a-zA-Z0-9-_ ,]+)";

            Regex commandRegex = new Regex(commandPattern);
            Regex descriptionRegex = new Regex(descriptionPattern);
            Regex branchRegex = new Regex(branchPattern);
            Regex branchDescriptionRegex = new Regex(branchDescriptionPattern);
            Regex argsRegex = new Regex(argsPattern);

            foreach (string line in lines) {
                if (commandRegex.IsMatch(line)) {
                    Match match = commandRegex.Match(line);

                    command = match.Groups[1].Value;
                }

                if (descriptionRegex.IsMatch(line)) {
                    Match match = descriptionRegex.Match(line);

                    description = match.Groups[1].Value;
                }

                if (branchRegex.IsMatch(line)) {
                    Match match = branchRegex.Match(line);

                    branch = match.Groups[1].Value;
                }

                if (branchDescriptionRegex.IsMatch(line)) {
                    Match match = branchDescriptionRegex.Match(line);

                    branchDescription = match.Groups[1].Value;
                }

                if (argsRegex.IsMatch(line)) {
                    Match match = argsRegex.Match(line);
                    args.Add(match.Groups[1].Value);
                }
            }

            if (command != String.Empty) {
                return new ShellScriptSettings(command, description, branch, branchDescription, args);
            }

            return null;
        }
    }
}