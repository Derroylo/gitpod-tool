using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using Gitpod.Tool.Helper;
using Spectre.Console;
using Spectre.Console.Cli;
using YamlDotNet.Serialization.NamingConventions;

namespace Gitpod.Tool.Commands.Services
{
    class ListServicesCommand : Command<ListServicesCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            if (!File.Exists("docker-compose.yml")) {
                AnsiConsole.MarkupLine("[red]docker-compose.yml not found[/]");

                return 0;
            }

            var services = DockerComposeHelper.GetServices("docker-compose.yml");

            var servicesTable = new Table();

            servicesTable.AddColumn("Name");
            servicesTable.AddColumn("Status");
            servicesTable.AddColumn("Active per default");

            foreach(KeyValuePair<string, string> item in services) {
                bool isActive = GptConfigHelper.Config.Services.Active.Contains(item.Key);
                bool isRunning = DockerComposeHelper.IsServiceStarted(item.Value);

                servicesTable.AddRow(item.Key + (item.Key != item.Value ? " (" + item.Value + ")" : ""), isRunning ? "[green1]Running[/]" : "[red]Not started[/]", isActive ? "[green1]Active[/]" : "[red]Inactive[/]");
            }
            
            AnsiConsole.Write(servicesTable);

            return 0;
        }
    }   
}
