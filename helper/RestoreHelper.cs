using Gitpod.Tool.Helper.Internal.Config.Sections;
using Gitpod.Tool.Helper.NodeJs;
using Gitpod.Tool.Helper.Persist;
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
            AnsiConsole.Write("Checking if environment variables has been set via config....");

            if (PersistConfig.Variables.Count == 0) {
                AnsiConsole.MarkupLine("[cyan3]Not found[/]");

                return;
            }
            
            AnsiConsole.MarkupLine("[green1]Found[/]");

            PersistVariableHelper.RestoreEnvironmentVariables(debug);
        }

        public static void RestorePersistedFiles(bool debug = false)
        {
            // Check if there has been something set via config file
            AnsiConsole.Write("Checking if files has been persisted via config....");

            if (PersistConfig.Files.Count == 0) {
                AnsiConsole.MarkupLine("[cyan3]Not found[/]");

                return;
            }
            
            AnsiConsole.MarkupLine("[green1]Found[/]");

            PersistFileHelper.RestorePersistedFiles(debug);
        }

        public static void RestorePersistedFolders(bool debug = false)
        {
            // Check if there has been something set via config file
            AnsiConsole.Write("Checking if folders has been persisted via config....");

            if (PersistConfig.Folders.Count == 0) {
                AnsiConsole.MarkupLine("[cyan3]Not found[/]");

                return;
            }
            
            AnsiConsole.MarkupLine("[green1]Found[/]");

            PersistFolderHelper.RestorePersistedFolders(debug);
        }
    }
}