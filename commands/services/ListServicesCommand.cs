using System.Collections.Generic;
using System.IO;
using Spectre.Console;
using Spectre.Console.Cli;
using Gitpod.Tool.Helper.Docker;
using Gitpod.Tool.Helper.Internal.Config.Sections;

namespace Gitpod.Tool.Commands.Services
{
    class ListServicesCommand : Command<ListServicesCommand.Settings>
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

            var services = DockerComposeHelper.GetServices(DockerComposeHelper.GetFile());

            var servicesTable = new Table();

            servicesTable.AddColumn("Name");
            servicesTable.AddColumn("Status");
            servicesTable.AddColumn("Active per default");

            foreach(KeyValuePair<string, Dictionary<string, string>> item in services) {
                var serviceAlias = item.Value["alias"];

                bool isActive = ServicesConfig.ActiveServices.Contains(item.Key);
                bool isRunning = DockerComposeHelper.IsServiceStarted(serviceAlias);

                servicesTable.AddRow(item.Key + (item.Key != serviceAlias ? " (" + serviceAlias + ")" : ""), isRunning ? "[green1]Running[/]" : "[red]Not started[/]", isActive ? "[green1]Active[/]" : "[red]Inactive[/]");
            }
            
            AnsiConsole.Write(servicesTable);

            return 0;
        }
    }   
}
