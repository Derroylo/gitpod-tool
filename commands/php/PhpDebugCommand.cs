using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using Gitpod.Tool.Helper.Php;

namespace Gitpod.Tool.Commands.Php
{
    class PhpDebugCommand : Command<PhpDebugCommand.Settings>
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
            var currentSettings = DebugHelper.GetCurrentSettings();

            var availableXdebugSettings = new [] { 
                "off - xdebug is fully disabled", 
                "develop - dev helpers including the overloaded var_dump",
                "coverage - mainly used for code coverage reports with phpunit", 
                "debug - step by step debugging via IDE", 
                "gcstats - enables garbage collection statistics", 
                "profile - enables the profiler and creates files for performance bottleneck searches", 
                "trace - every function call including arguments, var assignments and return values will be recorded" 
            };

            AnsiConsole.MarkupLine("Further explanation of the xdebug settings and how to setup your IDE correctly for it");
            AnsiConsole.MarkupLine("can be found under https://derroylo.github.io/guide/ [red]Update this link once the docs are live[/]");
            AnsiConsole.WriteLine("");

            if (currentSettings.TryGetValue("web", out string currentWebSetting)) {
                string color = "[green]";

                if (currentWebSetting.ToLower() == "Not installed/inactive".ToLower() || currentWebSetting.ToLower() == "unknown".ToLower() || currentWebSetting.ToLower() == "off".ToLower()) {
                    color = "[red]";
                }

                AnsiConsole.MarkupLine("[deepskyblue3]WEB[/] Current setting: " + color + currentWebSetting + "[/]");
            }

            if (currentSettings.TryGetValue("cli", out string currentCliSetting)) {
                string color = "[green]";

                if (currentCliSetting.ToLower() == "Not installed/inactive".ToLower() || currentCliSetting.ToLower() == "unknown".ToLower() || currentCliSetting.ToLower() == "off".ToLower()) {
                    color = "[red]";
                }

                AnsiConsole.MarkupLine("[deepskyblue3]CLI[/] Current setting: " + color + currentSettings["cli"] + "[/]");
            }

            if (!AnsiConsole.Confirm("Do you want to change the xdebug setting?", false)) {
                return 0;
            }

            if (currentWebSetting != string.Empty && currentWebSetting.ToLower() == "Not installed/inactive".ToLower() || currentWebSetting.ToLower() == "unknown".ToLower()) {
                AnsiConsole.MarkupLine("[deepskyblue3]WEB[/]: [red]The xdebug setting can´t be changed since it is either not installed or the status couldn´t be determined.[/]");
            } else if (AnsiConsole.Confirm("[deepskyblue3]WEB[/]: Do you want to change the xdebug setting?", false)) {
                var xdebugSettingWeb = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select the new xdebug setting")
                        .PageSize(5)
                        .AddChoices(availableXdebugSettings)
                );

                PhpIniHelper.AddSettingToPhpIni("xdebug.mode", xdebugSettingWeb.Split(" - ")[0], true, false);
            }

            if (currentCliSetting != string.Empty && currentCliSetting.ToLower() == "Not installed/inactive".ToLower() || currentCliSetting.ToLower() == "unknown".ToLower()) {
                AnsiConsole.MarkupLine("[deepskyblue3]CLI[/]: [red]The xdebug setting can´t be changed since it is either not installed or the status couldn´t be determined.[/]");
            } else if (AnsiConsole.Confirm("[deepskyblue3]CLI[/]: Do you want to change the xdebug setting?", false)) {
                var xdebugSettingCli = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select the new xdebug setting")
                        .PageSize(5)
                        .AddChoices(availableXdebugSettings)
                );

                PhpIniHelper.AddSettingToPhpIni("xdebug.mode", xdebugSettingCli.Split(" - ")[0], false, true);
            }

            return 0;
        }        
    }
}