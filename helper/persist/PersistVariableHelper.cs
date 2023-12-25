using System;
using System.Linq;
using Gitpod.Tool.Helper.Internal.Config.Sections;
using Spectre.Console;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;

namespace Gitpod.Tool.Helper.Persist
{
    partial class PersistVariableHelper
    {
        [GeneratedRegex(@"^([a-z0-9-_]+)$")]
        private static partial Regex EnvNameMatchRegex();

        public static void AddVariable(string updateEntryName = null)
        {
            AnsiConsole.MarkupLine("[red]Don´t save any sensible informations via this command![/]");
            AnsiConsole.MarkupLine("[red]For sensible data, use the `gp env` command[/]");

            bool finished = false;

            do {
                var envVarName = string.Empty;
                bool varNameInvalid = false;

                if (updateEntryName == null) {
                    do {
                        varNameInvalid = false;

                        envVarName = AnsiConsole.Ask<string>("What is the name of the variable?");

                        if (!EnvNameMatchRegex().Match(envVarName).Success) {
                            AnsiConsole.MarkupLine("[red]The variable name should only consist of the following characters: a-z 0-9 - _[/]");
                            varNameInvalid = true;
                        }
                    } while (varNameInvalid);
                } else {
                    envVarName = updateEntryName;
                }
                
                var envVarContent = AnsiConsole.Ask<string>("What is the content of the variable?");

                if (PersistConfig.Variables.Keys.Contains(envVarName, StringComparer.OrdinalIgnoreCase)) {
                    if (updateEntryName == null && AnsiConsole.Confirm("A variable with that name already exists. Do you want to replace it?", false)) {
                        var key = PersistConfig.Variables.Keys.FirstOrDefault(x => x.Equals(envVarName, StringComparison.OrdinalIgnoreCase));

                        PersistConfig.Variables[key] = envVarContent;
                        PersistConfig.ConfigUpdated = true;

                        AnsiConsole.MarkupLine("[green]The variable has been updated.[/]");
                    } else if(updateEntryName != null) {
                        var key = PersistConfig.Variables.Keys.FirstOrDefault(x => x.Equals(envVarName, StringComparison.OrdinalIgnoreCase));

                        PersistConfig.Variables[key] = envVarContent;
                        PersistConfig.ConfigUpdated = true;

                        AnsiConsole.MarkupLine("[green]The variable has been updated.[/]");
                    }
                } else {
                    PersistConfig.Variables.Add(envVarName, envVarContent);
                    PersistConfig.ConfigUpdated = true;

                    AnsiConsole.MarkupLine("[green]The variable has been added.[/]");
                }
                
                if (updateEntryName == null && !AnsiConsole.Confirm("Do you want to add more variables?", false)) {
                    finished = true;
                } else if(updateEntryName != null) {
                    finished = true;
                }
            } while (!finished);

            AnsiConsole.MarkupLine("[green]The changes have been saved.[/]");
        }

        public static void RestoreEnvironmentVariables(bool debug = false)
        {
            if (PersistConfig.Variables.Count == 0) {
                return;
            }

            var applicationDir = AppDomain.CurrentDomain.BaseDirectory;

            List<string> envVariables = new() {
                {"#!/usr/bin/env bash"}
            };
            
            foreach (KeyValuePair<string, string> entry in PersistConfig.Variables) {
                envVariables.Add("export " + entry.Key + "=\"" + entry.Value + "\"");
            }

            File.WriteAllText(applicationDir + ".env_restore", string.Join("\n", envVariables.ToArray<string>()));
        }

        public static void DeleteVariable()
        {
            if (PersistConfig.Variables.Count == 0) {
                AnsiConsole.WriteLine("No persisted environment variables have been set via gpt.yml");

                return;
            }

            var variables = PersistConfig.Variables.Keys.ToArray<string>();

            var variable = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the variable you want to delete")
                    .PageSize(10)
                    .AddChoices(variables)
            );

            PersistConfig.Variables.Remove(variable);
            PersistConfig.ConfigUpdated = true;

            AnsiConsole.MarkupLine("[green]The changes have been saved.[/]");
        }

        public static void UpdateVariable()
        {
            if (PersistConfig.Variables.Count == 0) {
                AnsiConsole.WriteLine("No persisted environment variables have been set via gpt.yml");

                return;
            }

            var variables = PersistConfig.Variables.Keys.ToArray<string>();

            var variable = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the variable you want to update")
                    .PageSize(10)
                    .AddChoices(variables)
            );

            PersistVariableHelper.AddVariable(variable);
        }
    }
}