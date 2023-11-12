using Gitpod.Tool.Helper;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Gitpod.Tool.Commands.Apache
{
    class ApacheStartCommand : Command<ApacheStartCommand.Settings>
    {
        public class Settings : CommandSettings
        {

        }

        public override int Execute(CommandContext context, Settings settings)
        {
            AnsiConsole.WriteLine(ExecCommand.Exec("apachectl start"));

            return 0;
        }
    }   
}
