using Gitpod.Tool.Helper;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Gitpod.Tool.Commands
{
    class SelfUpdateCommand : Command<SelfUpdateCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            // Force an update check
            var tmp = GptUpdateHelper.GetLatestVersion(true);

            if (!GptUpdateHelper.IsUpdateAvailable()) {
                AnsiConsole.MarkupLine("[red]You already have the latest version[/].");

                return 0;
            }

            AnsiConsole.WriteLine("Downloading the new release...");

            var res = GptUpdateHelper.UpdateToLatestRelease();

            if (!res.Result) {
                AnsiConsole.MarkupLine("[red]Failed to update the application.[/]");
            } else {
                AnsiConsole.MarkupLine("[green1]The application has been successfully updated to the latest version.[/]");
            }
            
            return 0;
        }
    }   
}
