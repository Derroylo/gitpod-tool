using Gitpod.Tool.Helper.Persist;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Gitpod.Tool.Commands.Persist
{
    class AddEntryCommand : Command<AddEntryCommand.Settings>
    {
        public class Settings : CommandSettings
        {
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            AnsiConsole.MarkupLine("[orange3]Save and restore different types of data on workspace start or init.[/]");
            AnsiConsole.MarkupLine("[orange3]The save location can be either within the file .gpt.yml or as Gitpod env variable.[/]");
            AnsiConsole.WriteLine("");
            AnsiConsole.MarkupLine("[orange3]Types:[/]");
            AnsiConsole.MarkupLine("[orange3]- An environment variable[/]");
            AnsiConsole.MarkupLine("[orange3]- A single file[/]");
            AnsiConsole.MarkupLine("[orange3]- Content of a folder[/]");
            AnsiConsole.WriteLine("");

            var entryTypes = new string[] {"Variable", "File", "Folder"};

            var entryType = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the type you want to add")
                    .PageSize(10)
                    .AddChoices(entryTypes)
            );

            if (entryType == "Variable") {
                PersistVariableHelper.AddVariable();
            } else if (entryType == "File") {
                PersistFileHelper.AddFile();
            } else if (entryType == "Folder") {
                PersistFolderHelper.AddFolder();
            }

            return 0;
        }
    }
}