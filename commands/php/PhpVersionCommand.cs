using Spectre.Console;
using Spectre.Console.Cli;
using System.Linq;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Gitpod.Tool.Helper;
using System.IO;

namespace Gitpod.Tool.Commands.Php
{
    class PhpVersionCommand : Command<PhpVersionCommand.Settings>
    {
        private Settings settings;

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
            this.settings = settings;

            if (this.settings.Version == null) {
                string result = PhpHelper.GetCurrentPhpVersionOutput();
                AnsiConsole.WriteLine(result);

                return 0;
            }

            PhpHelper.SetNewPhpVersion(this.settings.Version, this.settings.Debug);

            return 0;
        }        
    }
}