using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Gitpod.Tool.Helper.Docker;
using Gitpod.Tool.Helper.Internal.Config.Sections;
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

            bool isUsingCustomDockerfile = ServicesConfig.DockerComposeFile != "docker-compose.yml";
            AnsiConsole.WriteLine(ServicesConfig.DockerComposeFile);
            AnsiConsole.WriteLine(isUsingCustomDockerfile.ToString());
            AnsiConsole.WriteLine(applicationDir);

            if (applicationDir == "/workspace/.gpt/" && isUsingCustomDockerfile && File.Exists("/home/gitpod/.gpt/gpt.sh")) {
                var gptScriptContent = File.ReadAllText("/home/gitpod/.gpt/gpt.sh");

                if (gptScriptContent.Contains("docker-compose up $activeServices")) {
                    AnsiConsole.MarkupLine("[red]Disabled usage of custom named docker-compose files[/]");
                    AnsiConsole.MarkupLine("[orange3]It seems that in the workspace an older version of gpt is installed which contains an error that prevents the usage of custom named docker-compose files.[/]");
                    AnsiConsole.MarkupLine("[orange3]As temporary fix the file 'gpt.sh' has been copied to '/home/gitpod.gpt/' so that you can execute this command again to get it working.[/]");
                    AnsiConsole.MarkupLine("[orange3]Since that folder is not being persisted, you will face this error again after workspace restart. Update the workspace image so it uses the latest version of GPT.[/]");
                    AnsiConsole.MarkupLine("[orange3]https://www.gitpod.io/docs/configure/workspaces/workspace-image#manually-rebuild-a-workspace-image[/]");

                    try {
                        File.Copy("/workspace/.gpt/gpt.sh", "/home/gitpod/.gpt/gpt.sh");
                    } catch {
                        AnsiConsole.MarkupLine("[red]Copying the file has failed.[/] Try it manually with 'cp /workspace/.gpt/gpt.sh /home/gitpod/.gpt/gpt.sh");
                    }
                    
                    return 0;
                }
            }

            File.WriteAllText(applicationDir + ".services_start", "-f " + DockerComposeHelper.GetFile() + " up " + (settings.Detached ? "-d " : "") +  string.Join(' ', activeServices));

            return 0;
        }
    }   
}
