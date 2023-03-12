using System.ComponentModel;
using System.IO;
using System.Linq;
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
            string result = ExecCommand.Exec("apachectl start");

            AnsiConsole.WriteLine(result);

            return 0;
        }
    }   
}
