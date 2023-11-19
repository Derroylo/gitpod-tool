using Gitpod.Tool.Helper.Internal.Config.Sections;
using Spectre.Console;

namespace Gitpod.Tool.Helper.Php
{
    class PhpPackagesHelper
    {
        public static void InstallPackages(string[] newPackages, string phpVersion, bool debug = false)
        {
            // Update the pecl channel to the latest protocol
            ExecCommand.Exec("sudo pecl channel-update pecl.php.net");

            var updateRes = ExecCommand.Exec("sudo apt-get update");
            AnsiConsole.MarkupLine("Updating package manager list...[green1]Done[/]");

            if (debug) {
                AnsiConsole.WriteLine(updateRes);
            }

            string packages = string.Join(" ", newPackages).Replace("php-", "php" + phpVersion + "-");

            var installRes = ExecCommand.Exec("sudo apt-get install -y " + packages);
            AnsiConsole.MarkupLine("Installing packages...[green1]Done[/]");
            
            if (debug) {
                AnsiConsole.WriteLine(installRes);
            }

            SavePackagesInConfig(newPackages);
        }

        private static void SavePackagesInConfig(string[] packages)
        {
            foreach (string package in packages) {
                if (!PhpConfig.Packages.Contains(package)) {
                    PhpConfig.Packages.Add(package);
                }
            }    
        }
    }
}