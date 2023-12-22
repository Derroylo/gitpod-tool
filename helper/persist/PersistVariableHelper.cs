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

        public static void AddVariable()
        {
            AnsiConsole.MarkupLine("[red]Don´t save any sensible informations via this command![/]");
            AnsiConsole.MarkupLine("[red]For sensible data, use the `gp env` command[/]");

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

                if (PersistConfig.Variables.Keys.Contains(envVarName, StringComparer.OrdinalIgnoreCase)) {
                    if (AnsiConsole.Confirm("A variable with that name already exists. Do you want to replace it?", false)) {
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
                
                if (!AnsiConsole.Confirm("Do you want to add more variables?", false)) {
                    finished = true;
                }
            } while (!finished);
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
    }
}