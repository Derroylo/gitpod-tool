using Spectre.Console;
using Spectre.Console.Cli;

namespace Gitpod.Tool.Commands.Mysql
{
    class MysqlExportCommand : Command<MysqlExportCommand.Settings>
    {
        public class Settings : CommandSettings
        {

        }

        public override int Execute(CommandContext context, Settings settings)
        {
            AnsiConsole.WriteLine("Not implemented yet");

            return 0;
        }
    }   
}
