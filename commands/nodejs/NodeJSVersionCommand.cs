using Spectre.Console;
using Spectre.Console.Cli;
using System.Linq;
using System.ComponentModel;
using Gitpod.Tool.Helper.NodeJs;

namespace Gitpod.Tool.Commands.ModeJS
{
    class NodeJSVersionCommand : Command<NodeJSVersionCommand.Settings>
    {
        private Settings settings;

        public class Settings : CommandSettings
        {
            [CommandArgument(0, "[Version]")]
            [Description("Set this parameter to change the active nodejs version. Leave this parameter empty to show the current version.")]
            public string Version { get; set; }

            [CommandOption("-d|--debug")]
            [Description("Outputs debug information")]
            [DefaultValue(false)]
            public bool Debug { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            this.settings = settings;

            if (this.settings.Version != null) {
                NodeJsVersionHelper.SetNewNodeJSVersion(this.settings.Version, this.settings.Debug);

                return 0;
            }

            string result = NodeJsVersionHelper.GetCurrentNodeJSVersion();
            AnsiConsole.WriteLine(result);

            if (!AnsiConsole.Confirm("Do you want to change the active nodejs version?", false)) {
                return 0;
            }

            var availableNodeJSVersions = NodeJsVersionHelper.GetAvailableNodeJSVersions();

            var newVersion = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select the new active NodeJS version")
                        .PageSize(10)
                        .AddChoices(availableNodeJSVersions.ToArray<string>())
                );

            NodeJsVersionHelper.SetNewNodeJSVersion(newVersion, this.settings.Debug);

            return 0;
        }        
    }
}