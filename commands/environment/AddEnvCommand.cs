using System.Collections.Generic;
using Spectre.Console;
using Spectre.Console.Cli;
using Gitpod.Tool.Helper.Internal.Config.Sections;

namespace Gitpod.Tool.Commands.Environment
{
    class AddEnvCommand : Command<AddEnvCommand.Settings>
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

            return 0;
        }
    }
}