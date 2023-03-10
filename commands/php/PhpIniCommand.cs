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
            [DefaultValue("")]
            public string Mode { get; set; }

            [CommandArgument(1, "[IniSettingName]")]
            [DefaultValue("")]
            public string IniSettingName { get; set; }

            [CommandArgument(2, "[IniSettingValue]")]
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
