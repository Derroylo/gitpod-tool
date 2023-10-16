using System;
using System.IO;
using System.Collections.Generic;
using Gitpod.Tool.Classes;
using Spectre.Console;
using System.ComponentModel.Design.Serialization;
using System.Text.RegularExpressions;
using Gitpod.Tool.Helper.Internal.Config;

namespace Gitpod.Tool.Helper
{
    class CustomCommandsLoader
    {
        public static Dictionary<string, CustomBranch> Load()
        {           
            var scripts = SearchForShellScripts(".devEnv/gitpod/scripts");

            if (ShellScriptConfig.AdditionalDirectories.Count > 0) {
                foreach (string folder in ShellScriptConfig.AdditionalDirectories) {
                    scripts = SearchForShellScripts(folder, scripts);
                }
            }

            return scripts;
        }

        private static Dictionary<string, CustomBranch> SearchForShellScripts(string folder, Dictionary<string, CustomBranch> commands = null)
        {
            if (commands == null) {
                var defaultBranch = new CustomBranch("default");
                commands = new()
                {
                    // Add the default branch (commands that have no specified branch)
                    { defaultBranch.Name, defaultBranch }
                };
            }

            if (!Directory.Exists(folder)) {
                return commands;
            }

            string[] files = Directory.GetFiles(folder, "*.sh");
            foreach (string file in files) {
                var shellScriptSettings = ProcessShellScript(file);
                
                if (shellScriptSettings == null) {
                    continue;
                }

                var newCustomCommand = new CustomCommand(shellScriptSettings.Command, file, shellScriptSettings.Description, shellScriptSettings.Arguments);

                if (shellScriptSettings.Branch != string.Empty) {
                    if (commands.ContainsKey(shellScriptSettings.Branch)) {
                        commands[shellScriptSettings.Branch].Commands.Add(newCustomCommand);
                    } else {
                        var newBranch = new CustomBranch(shellScriptSettings.Branch, shellScriptSettings.BranchDescription);
                        newBranch.Commands.Add(newCustomCommand);

                        commands.Add(newBranch.Name, newBranch);
                    }
                } else {
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

            string command = string.Empty;
            string description = string.Empty;
            string branch = string.Empty;
            string branchDescription = string.Empty;
            List<string> args = new();

            string commandPattern =  @"\# gptCommand: ([a-zA-Z0-9-_]+)";
            string descriptionPattern =  @"\# gptDescription: ([a-zA-Z0-9-_ ,]+)";
            string branchPattern =  @"\# gptBranch: ([a-zA-Z0-9-_]+)";
            string branchDescriptionPattern =  @"\# gptBranchDescription: ([a-zA-Z0-9-_ ,]+)";
            string argsPattern =  @"\# gptArgument: ([a-zA-Z0-9-_ ,]+)";

            Regex commandRegex = new(commandPattern);
            Regex descriptionRegex = new(descriptionPattern);
            Regex branchRegex = new(branchPattern);
            Regex branchDescriptionRegex = new(branchDescriptionPattern);
            Regex argsRegex = new(argsPattern);

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

            if (command != string.Empty) {
                return new ShellScriptSettings(command, description, branch, branchDescription, args);
            }

            return null;
        }
    }
}