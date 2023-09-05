using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json.Converters;
using Spectre.Console;

namespace Gitpod.Tool.Helper
{
    class RestoreHelper
    {
        public static void RestorePhpVersion(bool debug = false)
        {
            AnsiConsole.Write("Checking if php version has been set via config....");

            if (GptConfigHelper.Config.Php.Version == String.Empty) {
                AnsiConsole.MarkupLine("[cyan3]Not found[/]");

                return;
            }

            AnsiConsole.MarkupLine("[green1]Found[/]");

            PhpHelper.SetNewPhpVersion(GptConfigHelper.Config.Php.Version, debug);
        }

        public static void RestorePhpIni(bool debug = false)
        {
            AnsiConsole.Write("Checking if php settings has been set via config....");

            if (GptConfigHelper.Config.Php.Config.Count == 0 && GptConfigHelper.Config.Php.ConfigCLI.Count == 0 && GptConfigHelper.Config.Php.ConfigWeb.Count == 0) {
                AnsiConsole.MarkupLine("[cyan3]Not found[/]");

                return;
            }

            AnsiConsole.MarkupLine("[green1]Found[/]");

            PhpHelper.UpdatePhpIniFiles(debug);
        }

        public static void RestoreNodeJsVersion(bool debug = false)
        {
            AnsiConsole.Write("Checking if NodeJS version has been set via config....");

            if (GptConfigHelper.Config.Nodejs.Version == String.Empty) {
                AnsiConsole.MarkupLine("[cyan3]Not found[/]");

                return;
            }

            AnsiConsole.MarkupLine("[green1]Found[/]");

            NodeJSHelper.SetNewNodeJSVersion(GptConfigHelper.Config.Nodejs.Version, debug);
        }
    }
}