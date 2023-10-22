using Spectre.Console;
using Spectre.Console.Cli;
using System.Linq;
using System.ComponentModel;
using Gitpod.Tool.Helper.Php;

namespace Gitpod.Tool.Commands.Php
{
    class PhpVersionCommand : Command<PhpVersionCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandArgument(0, "[Version]")]
            [Description("Set this parameter to change the active PHP version. Leave this parameter empty to show the current version.")]
            public string Version { get; set; }

            [CommandOption("-d|--debug")]
            [Description("Outputs debug information")]
            [DefaultValue(false)]
            public bool Debug { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            if (settings.Version != null) {
                PhpHelper.SetNewPhpVersion(settings.Version, settings.Debug);

                return 0;
            }

            string result = PhpVersionHelper.GetCurrentPhpVersionOutput();
            AnsiConsole.WriteLine(result);

            if (!AnsiConsole.Confirm("Do you want to change the active php version?", false)) {
                return 0;
            }

            var availablePhpVersions = PhpVersionHelper.GetAvailablePhpVersions();

            var newVersion = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select the new active php version")
                        .PageSize(5)
                        .AddChoices(availablePhpVersions.ToArray<string>())
                );

            PhpHelper.SetNewPhpVersion(newVersion, settings.Debug);

            return 0;
        }        
    }
}