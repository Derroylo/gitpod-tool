using Spectre.Console;
using Spectre.Console.Cli;
using System.Linq;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Gitpod.Tool.Helper;
using System.IO;
using Gitpod.Tool.Helper.Php;

namespace Gitpod.Tool.Commands.Php
{
    class PhpDebugCommand : Command<PhpDebugCommand.Settings>
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

            if (currentSettings.ContainsKey("web")) {
                string color = "[green]";

                if (currentSettings["web"].ToLower() == "Not installed/inactive".ToLower() || currentSettings["web"].ToLower() == "unknown".ToLower() || currentSettings["web"].ToLower() == "off".ToLower()) {
                    color = "[red]";
                }

                AnsiConsole.MarkupLine("[deepskyblue3]WEB[/] Current setting: " + color + currentSettings["web"] + "[/]");
            }

            if (currentSettings.ContainsKey("cli")) {
                string color = "[green]";

                if (currentSettings["cli"].ToLower() == "Not installed/inactive".ToLower() || currentSettings["cli"].ToLower() == "unknown".ToLower() || currentSettings["cli"].ToLower() == "off".ToLower()) {
                    color = "[red]";
                }

                AnsiConsole.MarkupLine("[deepskyblue3]CLI[/] Current setting: " + color + currentSettings["cli"] + "[/]");
            }

            if (!AnsiConsole.Confirm("Do you want to change the xdebug setting?", false)) {
                return 0;
            }

            if (currentSettings.ContainsKey("web") && currentSettings["web"].ToLower() == "Not installed/inactive".ToLower() || currentSettings["web"].ToLower() == "unknown".ToLower()) {
                AnsiConsole.MarkupLine("[deepskyblue3]WEB[/]: [red]The xdebug setting can´t be changed since it is either not installed or the status couldn´t be determined.[/]");
            } else if (AnsiConsole.Confirm("[deepskyblue3]WEB[/]: Do you want to change the xdebug setting?", false)) {
                var xdebugSettingWeb = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select the new xdebug setting")
                        .PageSize(5)
                        .AddChoices(availableXdebugSettings)
                );

                PhpHelper.AddSettingToPhpIni("xdebug.mode", xdebugSettingWeb.Split(" - ")[0], true, false);
            }

            if (currentSettings.ContainsKey("cli") && currentSettings["cli"].ToLower() == "Not installed/inactive".ToLower() || currentSettings["cli"].ToLower() == "unknown".ToLower()) {
                AnsiConsole.MarkupLine("[deepskyblue3]CLI[/]: [red]The xdebug setting can´t be changed since it is either not installed or the status couldn´t be determined.[/]");
            } else if (AnsiConsole.Confirm("[deepskyblue3]CLI[/]: Do you want to change the xdebug setting?", false)) {
                var xdebugSettingCli = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select the new xdebug setting")
                        .PageSize(5)
                        .AddChoices(availableXdebugSettings)
                );

                PhpHelper.AddSettingToPhpIni("xdebug.mode", xdebugSettingCli.Split(" - ")[0], false, true);
            }

            return 0;
        }        
    }
}