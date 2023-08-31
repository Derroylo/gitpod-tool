using System;
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
    class StartServicesCommand : Command<StartServicesCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("-d|--detached")]
            [Description("Outputs debug information")]
            [DefaultValue(false)]
            public bool Detached { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            if (!File.Exists("docker-compose.yml")) {
                AnsiConsole.MarkupLine("[red]docker-compose.yml not found[/]");

                return 0;
            }

            var services = DockerComposeHelper.GetServices("docker-compose.yml");

            if (GptConfigHelper.Config.Services == null || GptConfigHelper.Config.Services.Active.Count == 0) {
                AnsiConsole.WriteLine("[red]No active services selected[/]");

                return 0;
            }

            var activeServices = new List<string>();

            foreach (KeyValuePair<string, Dictionary<string, string>> item in services) {
                activeServices.Add(item.Key);
            }

            var applicationDir = AppDomain.CurrentDomain.BaseDirectory;

            File.WriteAllText(applicationDir + ".services_start", (settings.Detached ? "-d " : "") +  String.Join(' ', activeServices));

            return 0;
        }
    }   
}
