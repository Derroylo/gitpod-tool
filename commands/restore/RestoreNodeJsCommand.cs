using System;
using System.ComponentModel;
using System.IO;
using Gitpod.Tool.Helper;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Gitpod.Tool.Commands.Restore
{
    class RestoreNodeJsCommand : Command<RestoreNodeJsCommand.Settings>
    {
        private Settings settings;

        public class Settings : CommandSettings
        {
            [CommandOption("-d|--debug")]
            [Description("Outputs debug information")]
            [DefaultValue(false)]
            public bool Debug { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            this.settings = settings;

            RestoreHelper.RestoreNodeJsVersion(settings.Debug);
            
            return 0;
        }
    }
}