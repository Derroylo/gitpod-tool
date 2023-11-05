using Gitpod.Tool.Helper.Internal.Config.Sections;
using Gitpod.Tool.Helper.NodeJs;
using Gitpod.Tool.Helper.Php;
using Spectre.Console;

namespace Gitpod.Tool.Helper
{
    class RestoreHelper
    {
        public static void RestorePhpVersion(bool debug = false)
        {
            AnsiConsole.Write("Checking if php version has been set via config....");

            if (PhpConfig.PhpVersion == string.Empty) {
                AnsiConsole.MarkupLine("[cyan3]Not found[/]");

                return;
            }

            AnsiConsole.MarkupLine("[green1]Found[/]");

            PhpHelper.SetNewPhpVersion(PhpConfig.PhpVersion, debug);
        }

        public static void RestorePhpIni(bool debug = false)
        {
            AnsiConsole.Write("Checking if php settings has been set via config....");

            if (PhpConfig.Config.Count == 0 && PhpConfig.ConfigCli.Count == 0 && PhpConfig.ConfigWeb.Count == 0) {
                AnsiConsole.MarkupLine("[cyan3]Not found[/]");

                return;
            }

            AnsiConsole.MarkupLine("[green1]Found[/]");

            PhpIniHelper.UpdatePhpIniFiles(debug);
        }

        public static void RestoreNodeJsVersion(bool debug = false)
        {
            AnsiConsole.Write("Checking if NodeJS version has been set via config....");

            if (NodeJsConfig.NodeJsVersion == string.Empty) {
                AnsiConsole.MarkupLine("[cyan3]Not found[/]");

                return;
            }

            AnsiConsole.MarkupLine("[green1]Found[/]");

            NodeJsVersionHelper.SetNewNodeJSVersion(NodeJsConfig.NodeJsVersion, debug);
        }

        public static void RestoreEnvVariables(bool debug = false)
        {
            // Check if there has been something set via config file
            AnsiConsole.Write("Checking if Env variables has been set via config....");

            // Not implemented yet, will come with the next major release
            AnsiConsole.MarkupLine("[cyan3]Not found[/]");
        }
    }
}