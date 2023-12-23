using Gitpod.Tool.Helper.Persist;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Gitpod.Tool.Commands.Persist
{
    class UpdateEntryCommand : Command<UpdateEntryCommand.Settings>
    {
        public class Settings : CommandSettings
        {
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            var entryTypes = new string[] {"Variable", "File", "Folder"};

            var entryType = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the type you want to update")
                    .PageSize(10)
                    .AddChoices(entryTypes)
            );

            if (entryType == "Variable") {
                PersistVariableHelper.UpdateVariable();
            } else if (entryType == "File") {
                PersistFileHelper.UpdateFile();
            } else if (entryType == "Folder") {
                PersistFolderHelper.UpdateFolder();
            }

            return 0;
        }
    }
}