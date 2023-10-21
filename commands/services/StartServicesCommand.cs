using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Gitpod.Tool.Helper;
using Gitpod.Tool.Helper.Internal.Config;
using Spectre.Console;
using Spectre.Console.Cli;

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
            if (!File.Exists(DockerComposeHelper.GetFile())) {
                AnsiConsole.MarkupLine(string.Format("[red]{0} not found[/]", DockerComposeHelper.GetFile()));

                return 0;
            }

            var services = DockerComposeHelper.GetServices(DockerComposeHelper.GetFile());

            if (ServicesConfig.ActiveServices.Count == 0) {
                AnsiConsole.WriteLine("[red]No active services selected[/]");

                return 0;
            }

            var activeServices = new List<string>();

            foreach (KeyValuePair<string, Dictionary<string, string>> item in services) {
                activeServices.Add(item.Key);
            }

            var applicationDir = AppDomain.CurrentDomain.BaseDirectory;

            File.WriteAllText(applicationDir + ".services_start", "-f " + DockerComposeHelper.GetFile() + " up " + (settings.Detached ? "-d " : "") +  String.Join(' ', activeServices));

            return 0;
        }
    }   
}
