using System.ComponentModel;
using Gitpod.Tool.Helper.Php;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Gitpod.Tool.Commands.Php
{
    class PhpIniCommand : Command<PhpIniCommand.Settings>
    {
        private Settings settings;

        public class Settings : CommandSettings
        {
            [CommandArgument(0, "[settings]")]
            [Description("Define one or multiple settings")]
            public string[] PhpSettings { get; set; }

            [CommandOption("-w|--web")]
            [Description("Applies the given settings for web")]
            [DefaultValue(false)]
            public bool SetForWeb { get; set; }

            [CommandOption("-c|--cli")]
            [Description("Applies the given settings for CLI")]
            [DefaultValue(false)]
            public bool SetForCLI { get; set; }

            [CommandOption("-d|--debug")]
            [Description("Outputs debug information")]
            [DefaultValue(false)]
            public bool Debug { get; set; }
        }
        
        public override int Execute(CommandContext context, Settings settings)
        {
            this.settings = settings;

            if (settings.PhpSettings != null && settings.PhpSettings.Length > 0) {
                return ApplySettingsViaOption(settings.PhpSettings, settings.SetForWeb, settings.SetForCLI);
            }

            AnsiConsole.WriteLine(PhpIniHelper.GetPhpIniPath());

            if (!AnsiConsole.Confirm("Do you want to change a php setting?", false)) {
                return 0;
            }

            AskUserForPhpSettings();

            return 0;
        }

        private void AskUserForPhpSettings()
        {
            bool canceled = false;

            do {
                var settingName = AnsiConsole.Prompt(
                    new TextPrompt<string>("Name of the setting:")
                        .ValidationErrorMessage("[red]That´s not a valid name for a setting[/]")
                        .Validate(setting =>
                        {
                            return setting.Length switch
                            {
                                <= 3 => ValidationResult.Error("[red]The name must be at least 3 chars long.[/]"),
                                _ => ValidationResult.Success(),
                            };
                        }
                    )
                );

                var settingValue = AnsiConsole.Prompt(
                    new TextPrompt<string>("New value of the setting: ")
                        .ValidationErrorMessage("[red]That´s not a valid value[/]")
                        .Validate(setting =>
                        {
                            return setting.Length switch
                            {
                                <= 0 => ValidationResult.Error("[red]The value must be at least 1 chars long.[/]"),
                                _ => ValidationResult.Success(),
                            };
                        }
                    )
                );

                var scope = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("For which scope should the setting be applied to?")
                        .PageSize(3)
                        .AddChoices(new[] {
                            "Web", "CLI", "both",
                        }
                    )
                );

                PhpIniHelper.AddSettingToPhpIni(settingName, settingValue, scope == "Web", scope == "CLI", this.settings.Debug);

                if (!AnsiConsole.Confirm("Do you want to change more php settings?", false)) {
                    canceled = true;
                }
            } while (!canceled);
        }

        private int ApplySettingsViaOption(string[] newSettings, bool setForWeb = false, bool setForCLI = false)
        {
            foreach (string setting in newSettings) {
                AnsiConsole.WriteLine(setting);

                var splittedSetting = setting.Split("=");

                if (splittedSetting.Length != 2) {
                    AnsiConsole.MarkupLine("[red]\"" + setting + "\" is not a valid value.[/] It needs to be written in the format \"setting=newValue\". For example \"gpt php ini memory_limit=512M\"");

                    return 1;
                }

                PhpIniHelper.AddSettingToPhpIni(splittedSetting[0], splittedSetting[1], setForWeb, setForCLI, settings.Debug);
            }

            return 0;
        }
    }   
}
