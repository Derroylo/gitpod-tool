using Gitpod.Tool.Helper.Internal.Config;
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

            PhpHelper.UpdatePhpIniFiles(debug);
        }

        public static void RestoreNodeJsVersion(bool debug = false)
        {
            AnsiConsole.Write("Checking if NodeJS version has been set via config....");

            if (NodeJsConfig.NodeJsVersion == string.Empty) {
                AnsiConsole.MarkupLine("[cyan3]Not found[/]");

                return;
            }

            AnsiConsole.MarkupLine("[green1]Found[/]");

            NodeJSHelper.SetNewNodeJSVersion(NodeJsConfig.NodeJsVersion, debug);
        }
    }
}