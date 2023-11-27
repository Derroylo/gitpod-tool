using System.Collections.Generic;
using Spectre.Console;
using Spectre.Console.Cli;
using Gitpod.Tool.Helper.Internal.Config.Sections;
using System.Linq;
using System.Text.RegularExpressions;
using System;

namespace Gitpod.Tool.Commands.Environment
{
    partial class AddEnvCommand : Command<AddEnvCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            var envTypes = new string[] {"Variable", "File"};

            var envType = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the type you want to add")
                    .PageSize(10)
                    .AddChoices(envTypes)
            );

            if (envType == "Variable") {
                AddVariable();
            }

            return 0;
        }

        [GeneratedRegex(@"^([a-z0-9-_]+)$")]
        private static partial Regex EnvNameMatchRegex();

        private void AddVariable()
        {
            AnsiConsole.MarkupLine("[red]Don´t save any sensible informations via this command![/]");
            AnsiConsole.MarkupLine("[red]For sensible data, use the gp env command[/]");

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

                if (EnvironmentConfig.Variables.Keys.Contains(envVarName, StringComparer.OrdinalIgnoreCase)) {
                    if (AnsiConsole.Confirm("A variable with that name already exists. Do you want to replace it?", false)) {
                        var key = EnvironmentConfig.Variables.Keys.FirstOrDefault(x => x.Equals(envVarName, StringComparison.OrdinalIgnoreCase));

                        EnvironmentConfig.Variables[key] = envVarContent;
                        EnvironmentConfig.ConfigUpdated = true;

                        AnsiConsole.MarkupLine("[green]The variable has been update.[/]");
                    }
                } else {
                    EnvironmentConfig.Variables.Add(envVarName, envVarContent);
                    EnvironmentConfig.ConfigUpdated = true;

                    AnsiConsole.MarkupLine("[green]The variable has been added.[/]");
                }
                
                if (!AnsiConsole.Confirm("Do you want to add more variables?", false)) {
                    finished = true;
                }
            } while (!finished);
        }
    }
}