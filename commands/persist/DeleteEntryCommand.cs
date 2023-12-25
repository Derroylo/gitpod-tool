using Gitpod.Tool.Helper.Persist;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Gitpod.Tool.Commands.Persist
{
    class DeleteEntryCommand : Command<DeleteEntryCommand.Settings>
    {
        public class Settings : CommandSettings
        {
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            var entryTypes = new string[] {"Variable", "File", "Folder"};

            var entryType = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the type you want to delete")
                    .PageSize(10)
                    .AddChoices(entryTypes)
            );

            if (entryType == "Variable") {
                PersistVariableHelper.DeleteVariable();
            } else if (entryType == "File") {
                PersistFileHelper.DeleteFile();
            } else if (entryType == "Folder") {
                PersistFolderHelper.DeleteFolder();
            }

            return 0;
        }
    }
}