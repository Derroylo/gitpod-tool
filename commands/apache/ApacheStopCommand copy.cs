using Gitpod.Tool.Helper;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Gitpod.Tool.Commands.Apache
{
    class ApacheStopCommand : Command<ApacheStopCommand.Settings>
    {
        public class Settings : CommandSettings
        {

        }

        public override int Execute(CommandContext context, Settings settings)
        {
            string result = ExecCommand.Exec("apachectl stop");

            AnsiConsole.WriteLine(result);

            return 0;
        }
    }   
}
