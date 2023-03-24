using System.ComponentModel;
using System.IO;
using System.Linq;
using Gitpod.Tool.Helper;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Gitpod.Tool.Commands.Php
{
    class PhpIniCommand : Command<PhpIniCommand.Settings>
    {
        private Settings settings;

        public class Settings : CommandSettings
        {
            [CommandArgument(0, "[Mode]")]
            [Description("'update' for updating the php.ini files for apache/cli, 'set' for changing a value or empty to show the path")]
            [DefaultValue("")]
            public string Mode { get; set; }

            [CommandArgument(1, "[IniSettingName]")]
            [Description("When using 'set', this is the name of the setting you want to change")]
            [DefaultValue("")]
            public string IniSettingName { get; set; }

            [CommandArgument(2, "[IniSettingValue]")]
            [Description("When using 'set', this is the new value")]
            [DefaultValue("")]
            public string IniSettingValue { get; set; }

            [CommandOption("-d|--debug")]
            [Description("Outputs debug information")]
            [DefaultValue(false)]
            public bool Debug { get; set; }
        }
        
        public override ValidationResult Validate(CommandContext context, Settings settings)
        {
            string[] allowedModes = {"update", "set"};

            if (settings.Mode != "" && !allowedModes.Contains(settings.Mode))
            {
                return ValidationResult.Error($"Mode not allowed - {settings.Mode}");
            }

            return base.Validate(context, settings);
        }


        public override int Execute(CommandContext context, Settings settings)
        {
            this.settings = settings;

            switch (settings.Mode)
            {
                case "update":
                    PhpHelper.UpdatePhpIniFiles(settings.Debug);

                    return 0;

                case "set":
                    PhpHelper.AddSettingToPhpIni(settings.IniSettingName, settings.IniSettingValue, settings.Debug);

                    return 0;
                default:
                    AnsiConsole.WriteLine(PhpHelper.GetPhpIniPath());
                    break;
            }

            return 0;
        }
    }   
}
