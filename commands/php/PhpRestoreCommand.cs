using System.ComponentModel;
using Gitpod.Tool.Helper;
using Spectre.Console.Cli;

namespace Gitpod.Tool.Commands.Php
{
    class PhpRestoreCommand : Command<PhpRestoreCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("-d|--debug")]
            [Description("Outputs debug information")]
            [DefaultValue(false)]
            public bool Debug { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            RestoreHelper.RestorePhpVersion(settings.Debug);
            RestoreHelper.RestorePhpIni(settings.Debug);

            return 0;
        }
    }
}