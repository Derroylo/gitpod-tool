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