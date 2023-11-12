using System;
using System.IO;
using Gitpod.Tool.Helper.Docker;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Gitpod.Tool.Commands.Services
{
    class StopServicesCommand : Command<StopServicesCommand.Settings>
    {
        public class Settings : CommandSettings
        {

        }

        public override int Execute(CommandContext context, Settings settings)
        {
            if (!File.Exists(DockerComposeHelper.GetFile())) {
                AnsiConsole.MarkupLine(string.Format("[red]{0} not found[/]", DockerComposeHelper.GetFile()));

                return 0;
            }

            var applicationDir = AppDomain.CurrentDomain.BaseDirectory;

            File.WriteAllText(applicationDir + ".services_stop", "-f " + DockerComposeHelper.GetFile() + " stop");

            return 0;
        }
    }   
}
