using Gitpod.Tool.Helper;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Gitpod.Tool.Commands.Apache
{
    class ApacheStatusCommand : Command<ApacheStatusCommand.Settings>
    {
        public class Settings : CommandSettings
        {

        }

        public override int Execute(CommandContext context, Settings settings)
        {
            string result = ExecCommand.Exec("apachectl status");

            AnsiConsole.WriteLine(result);

            return 0;
        }
    }   
}
