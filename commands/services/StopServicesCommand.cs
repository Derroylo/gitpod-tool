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
    class StopServicesCommand : Command<StopServicesCommand.Settings>
    {
        public class Settings : CommandSettings
        {

        }

        public override int Execute(CommandContext context, Settings settings)
        {
            var dockerComposeFile = GptConfigHelper.Config.DockerComposeFile ?? "docker-compose.yml";

            if (!File.Exists(dockerComposeFile)) {
                AnsiConsole.MarkupLine(String.Format("[red]{0} not found[/]", dockerComposeFile));

                return 0;
            }

            var applicationDir = AppDomain.CurrentDomain.BaseDirectory;

            File.WriteAllText(applicationDir + ".services_stop", " ");

            return 0;
        }
    }   
}
