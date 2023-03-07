using System;
using System.IO;
using System.Text.RegularExpressions;
using Spectre.Console;

namespace Gitpod.Tool.Helper
{
    class PhpHelper
    {  
        public static void SetNewPhpVersion(string newVersion, bool isDebug)
        {
            AnsiConsole.Write("Checking if the input is a valid php version....");
            Regex regex = new Regex(@"[(0-9)].[(0-9)]");
            Match match = regex.Match(newVersion);

            if (!match.Success) {
                AnsiConsole.MarkupLine("[red]Invalid[/]");

                return;
            }

            string inputCheck = ExecCommand.Exec("update-alternatives --query php");

            if (!inputCheck.Contains("/usr/bin/php" + newVersion)) {
                AnsiConsole.MarkupLine("[red]Invalid[/]");
                
                if (isDebug) {
                    AnsiConsole.Write(inputCheck);
                }

                return;
            }

            AnsiConsole.MarkupLine("[green1]Valid[/]");

            AnsiConsole.Write("Setting PHP Version to " + newVersion + "....");
            ExecCommand.Exec("sudo update-alternatives --set php /usr/bin/php" + newVersion);
            ExecCommand.Exec("sudo update-alternatives --set php-config /usr/bin/php-config" + newVersion);
            AnsiConsole.MarkupLine("[green1]Done[/]");

            AnsiConsole.Write("Validating that the new version has been set....");

            string testResult = PhpHelper.GetCurrentPhpVersionOutput();

            if (isDebug) {
                AnsiConsole.Write(testResult);
            }

            if (!testResult.Contains(newVersion)) {
                AnsiConsole.MarkupLine("[red]Failed[/]");

                return;
            }

            AnsiConsole.MarkupLine("[green1]Success[/]");

            AnsiConsole.Write("Checking if file with additional packages exists....");

            string[] addPackages = PhpHelper.GetAdditionalPackagesFromConfig(newVersion, isDebug);

            if (addPackages != null && addPackages.Length > 0) {
                AnsiConsole.MarkupLine("[green1]Found[/]");

                if (isDebug) {
                    AnsiConsole.WriteLine("File content:");

                    foreach(string line in addPackages) {
                        AnsiConsole.WriteLine(line);
                    }
                }

                AnsiConsole.Write("Updating package manager list....");
                var updateRes = ExecCommand.Exec("sudo apt-get update");
                AnsiConsole.MarkupLine("[green1]Done[/]");

                if (isDebug) {
                    AnsiConsole.WriteLine(updateRes);
                }

                string packages = string.Join(" ", addPackages).Replace("VERSION", newVersion);

                AnsiConsole.Write("Installing packages....");
                var installRes = ExecCommand.Exec("sudo apt-get install -y " + packages);
                AnsiConsole.MarkupLine("[green1]Done[/]");                
                
                if (isDebug) {
                    AnsiConsole.WriteLine(installRes);
                }
            } else {
                AnsiConsole.MarkupLine("[cyan3]Not found[/]");
            }

            AnsiConsole.Write("Saving the new active version so it can be restored....");

            try {
                File.WriteAllText("./.devEnv/gitpod/php/active", newVersion);
            } catch {
                AnsiConsole.MarkupLine("[red]Failed[/]");

                return;
            }
            
            AnsiConsole.MarkupLine("[green1]Done[/]");
        }

        public static string[] GetAdditionalPackagesFromConfig(string phpVersion, bool isDebug)
        {
            string[] fileContent = null;

            if (isDebug) {
                AnsiConsole.Write("Checking if \".devEnv/gitpod/config/php/packages/" + phpVersion + "\" exists....");
            }

            if (File.Exists(".devEnv/gitpod/config/php/packages/" + phpVersion)) {
                fileContent = File.ReadAllLines(".devEnv/gitpod/config/php/packages/" + phpVersion);

                if (isDebug) {
                    AnsiConsole.Write("Exists. Skipping further checks....");
                }
            } else if (isDebug) {
                AnsiConsole.Write("Does not exists. Checking if \".devEnv/gitpod/config/php/packages/default\" exists....");
            }

            if (fileContent == null && File.Exists(".devEnv/gitpod/config/php/packages/default")) {
                fileContent = File.ReadAllLines(".devEnv/gitpod/config/php/packages/default");

                if (isDebug) {
                    AnsiConsole.Write("Exists....");
                }
            } else if (isDebug) {
                AnsiConsole.Write("Does not exists....");
            }

            return fileContent;
        }

        public static string GetCurrentPhpVersionOutput()
        {
            return ExecCommand.Exec("php -version");
        }

        public static string GetCurrentPhpVersion()
        {
            string output = PhpHelper.GetCurrentPhpVersionOutput();

            Regex regex = new Regex(@"(?:PHP) ([(0-9)].[(0-9)])");
            Match match = regex.Match(output);

            if (!match.Success) {
                throw new Exception("Failed to parse the php version command output to find the active version.");
            }

            return match.Groups[1].Value;
        }
    }
}