using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Spectre.Console;

namespace Gitpod.Tool.Helper
{
    class PhpHelper
    {  
        public static void SetNewPhpVersion(string newVersion, bool isDebug)
        {
            AnsiConsole.Status()
                .Start("Setting PHP Version to " + newVersion, ctx => 
                {
                    Regex regex = new Regex(@"[(0-9)].[(0-9)]");
                    Match match = regex.Match(newVersion);

                    if (!match.Success) {
                        AnsiConsole.MarkupLine("Checking if the input is a valid php version....[red]Invalid[/]");

                        return;
                    }

                    string inputCheck = ExecCommand.Exec("update-alternatives --query php");

                    if (!inputCheck.Contains("/usr/bin/php" + newVersion)) {
                        AnsiConsole.MarkupLine("Checking if the input is a valid php version....[red]Invalid[/]");
                        
                        if (isDebug) {
                            AnsiConsole.Write(inputCheck);
                        }

                        return;
                    }

                    AnsiConsole.MarkupLine("Checking if the input is a valid php version....[green1]Valid[/]");

                    string currentPhpVersion = PhpHelper.GetCurrentPhpVersion();

                    // Update the CLI Version
                    ExecCommand.Exec("sudo update-alternatives --set php /usr/bin/php" + newVersion);
                    AnsiConsole.MarkupLine("update-alternatives --set php /usr/bin/php" + newVersion + "...[green1]Success[/]");

                    ExecCommand.Exec("sudo update-alternatives --set php-config /usr/bin/php-config" + newVersion);
                    AnsiConsole.MarkupLine("update-alternatives --set php-config /usr/bin/php-config" + newVersion + "...[green1]Success[/]");

                    // Update the version apache uses
                    ExecCommand.Exec("sudo apt-get update");
                    AnsiConsole.MarkupLine("apt-get update...[green1]Success[/]");

                    ExecCommand.Exec("sudo apt-get install -y libapache2-mod-php" + newVersion);
                    AnsiConsole.MarkupLine("apt-get install -y libapache2-mod-php" + newVersion + "...[green1]Success[/]");

                    ExecCommand.Exec("sudo a2dismod php" + currentPhpVersion);
                    AnsiConsole.MarkupLine("a2dismod php" + currentPhpVersion + "...[green1]Success[/]");

                    ExecCommand.Exec("sudo a2enmod php" + newVersion);
                    AnsiConsole.MarkupLine("a2enmod php" + newVersion + "...[green1]Success[/]");
                    
                    // Checking apache status and restarting it
                    ctx.Status("Restarting apache...");
                    
                    if (ExecCommand.Exec("apachectl start").Contains(" already running")) {
                        ExecCommand.Exec("apachectl restart");
                    }

                    if (ExecCommand.Exec("service apache2 status").Contains("apache2 is not running")) {
                        ExecCommand.Exec("apachectl start");
                    }

                    AnsiConsole.MarkupLine("Restarting apache...[green1]Success[/]");


                    ctx.Status("Validating that the new version has been set...");

                    string testResult = PhpHelper.GetCurrentPhpVersionOutput();

                    if (isDebug) {
                        AnsiConsole.WriteLine(testResult);
                    }

                    if (!testResult.Contains(newVersion)) {
                        AnsiConsole.MarkupLine("Validating that the new version has been set....[red]Failed[/]");

                        return;
                    }

                    AnsiConsole.MarkupLine("Validating that the new version has been set...[green1]Success[/]");

                    ctx.Status("Checking if file with additional packages exists...");

                    string[] addPackages = PhpHelper.GetAdditionalPackagesFromConfig(newVersion, isDebug);

                    if (addPackages != null && addPackages.Length > 0) {
                        if (isDebug) {
                            AnsiConsole.WriteLine("File content:");

                            foreach(string line in addPackages) {
                                AnsiConsole.WriteLine(line);
                            }
                        }

                        var updateRes = ExecCommand.Exec("sudo apt-get update");
                        AnsiConsole.MarkupLine("Updating package manager list...[green1]Done[/]");

                        if (isDebug) {
                            AnsiConsole.WriteLine(updateRes);
                        }

                        string packages = string.Join(" ", addPackages).Replace("VERSION", newVersion);

                        var installRes = ExecCommand.Exec("sudo apt-get install -y " + packages);
                        AnsiConsole.MarkupLine("Installing packages...[green1]Done[/]");
                        
                        if (isDebug) {
                            AnsiConsole.WriteLine(installRes);
                        }
                    } else {
                        AnsiConsole.MarkupLine("Checking if file with additional packages exists...[cyan3]Not found[/]");
                    }

                    ctx.Status("Saving the new active version so it can be restored...");

                    try {
                        File.WriteAllText("./.devEnv/gitpod/php/active", newVersion);

                        AnsiConsole.MarkupLine("Saving the new active version so it can be restored...[green1]Done[/]");
                    } catch {
                        AnsiConsole.MarkupLine("Saving the new active version so it can be restored...[red]Failed[/]");
                    }
                });

            AnsiConsole.MarkupLine("PHP Version has been set to " + newVersion);
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

        public static string GetPhpIniPath()
        {
            return ExecCommand.Exec("php -i | grep 'Configuration File'");
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

        public static void UpdatePhpIniFiles(bool isDebug)
        {
            string currentPhpVersion = PhpHelper.GetCurrentPhpVersion();

            if (isDebug) {
                AnsiConsole.WriteLine("Active PHP Version " + currentPhpVersion);
            }

            AnsiConsole.Write("Checking if we have a folder with ini files for the active version....");

            if (!Directory.Exists("./.devEnv/gitpod/php/config/" + currentPhpVersion)) {
                AnsiConsole.MarkupLine("[cyan3]Directory Not found[/]");

                return;
            }

            string[] additionalIniFiles = Directory.GetFiles("./.devEnv/gitpod/php/config/" + currentPhpVersion, "*.ini", SearchOption.AllDirectories);

            if (additionalIniFiles.Length == 0) {
                AnsiConsole.MarkupLine("[cyan3]No files found[/]");

                return;
            }

            AnsiConsole.MarkupLine("[green1]Found " + additionalIniFiles.Length + " File(s)[/]");

            if (isDebug) {
                AnsiConsole.WriteLine("Found the following files: ");

                foreach (string file in additionalIniFiles) {
                    AnsiConsole.WriteLine(file);
                }
            }
            
            AnsiConsole.Write("Copy the file(s) to the target directory....");

            foreach (string file in additionalIniFiles) {
                string targetFile = "/etc/php/" + currentPhpVersion + "/" + file.Replace("./.devEnv/gitpod/php/config/" + currentPhpVersion  + "/", "");
                
                if (isDebug) {
                    AnsiConsole.WriteLine("Copying from \"" + file + "\" to \"" + targetFile + "\"");
                }
                
                ExecCommand.Exec("sudo cp " + file + " " + targetFile);
            }

            AnsiConsole.MarkupLine("[green1]Done[/]");
        }

        public static void AddSettingToPhpIni(string name, string value, bool isDebug)
        {
            if (name == null || name.Length <= 3 || value == null || value.Length < 1) {
                AnsiConsole.WriteLine("[red]Invalid name and/or value[/]");

                return;
            }

            string currentPhpVersion = PhpHelper.GetCurrentPhpVersion();

            if (isDebug) {
                AnsiConsole.WriteLine("Active PHP Version " + currentPhpVersion);
            }

            AnsiConsole.Write("Checking if we have a folder with ini files for the active version....");

            if (!Directory.Exists("./.devEnv/gitpod/php/config/" + currentPhpVersion)) {
                if (isDebug) {
                    AnsiConsole.Write("Not found, creating it....");
                }
                
                // Creating main directory
                Directory.CreateDirectory("./.devEnv/gitpod/php/config/" + currentPhpVersion);

                AnsiConsole.MarkupLine("[cyan3]Created[/]");
            } else {
                AnsiConsole.MarkupLine("[green1]Found[/]");
            }

            AnsiConsole.Write("Checking if the folder exists for cli and apache....");

            if (!Directory.Exists("./.devEnv/gitpod/php/config/" + currentPhpVersion + "/cli/conf.d")) {
                if (isDebug) {
                    AnsiConsole.Write("Not found, creating cli dir....");
                }
                
                // Creating subdirectory for Apache
                Directory.CreateDirectory("./.devEnv/gitpod/php/config/" + currentPhpVersion + "/cli/conf.d");

            }

            if (!Directory.Exists("./.devEnv/gitpod/php/config/" + currentPhpVersion + "/apache2/conf.d")) {
                if (isDebug) {
                    AnsiConsole.Write("Not found, creating apache dir....");
                }
                
                // Creating subdirectory for Apache
                Directory.CreateDirectory("./.devEnv/gitpod/php/config/" + currentPhpVersion + "/apache2/conf.d");

            }

            if (!Directory.Exists("./.devEnv/gitpod/php/config/" + currentPhpVersion + "/fpm/conf.d")) {
                if (isDebug) {
                    AnsiConsole.Write("Not found, creating fpm dir....");
                }
                
                // Creating subdirectory for fpm
                Directory.CreateDirectory("./.devEnv/gitpod/php/config/" + currentPhpVersion + "/fpm/conf.d");

            }

            AnsiConsole.MarkupLine("[green1]Done[/]");

            PhpHelper.AddUpdateIniSettingInCustomIni(name, value, "./.devEnv/gitpod/php/config/" + currentPhpVersion + "/cli/conf.d/custom.ini", isDebug);
            PhpHelper.AddUpdateIniSettingInCustomIni(name, value, "./.devEnv/gitpod/php/config/" + currentPhpVersion + "/apache2/conf.d/custom.ini", isDebug);
            PhpHelper.AddUpdateIniSettingInCustomIni(name, value, "./.devEnv/gitpod/php/config/" + currentPhpVersion + "/fpm/conf.d/custom.ini", isDebug);

            ExecCommand.Exec("apachectl restart");
        }

        private static void AddUpdateIniSettingInCustomIni(string name, string value, string fileWithPath, bool isDebug)
        {
            AnsiConsole.Write("Checking if \"" + fileWithPath + "\" exists....");

            if (!File.Exists(fileWithPath)) {
                if (isDebug) {
                    AnsiConsole.Write("Not found, creating it....");
                }
                
                File.Create(fileWithPath).Close();

                AnsiConsole.MarkupLine("[cyan3]Created[/]");
            } else {
                AnsiConsole.MarkupLine("[green1]Found[/]");
            }

            AnsiConsole.Write("Reading content of \"" + fileWithPath + "\"....");

            string[] fileContent = File.ReadAllLines(fileWithPath);

            AnsiConsole.MarkupLine("[green1]Done[/]");

            AnsiConsole.Write("Checking if the config already contains a value for the setting...");

            bool settingFound = false;
            Regex regex = new Regex("^" + name + @".?=.?(.*)");

            for (int i = 0; i < fileContent.Length; i++) {
                Match match = regex.Match(fileContent[i]);

                if (match.Success) {
                    if (isDebug) {
                        AnsiConsole.WriteLine("Found setting in current custom.ini: " + fileContent[i]);
                    }
                    
                    settingFound = true;
                    fileContent[i] = name + " = " + value;
                }
            }

            if (!settingFound) {
                AnsiConsole.MarkupLine("[cyan3]Not found, adding it to the end[/]");
                fileContent = fileContent.Append<String>(name + " = " + value).ToArray<string>();
            } else {
                AnsiConsole.MarkupLine("[green1]Updated[/]");
            }

            AnsiConsole.Write("Updating the custom.ini file...");

            File.WriteAllLines(fileWithPath, fileContent);

            AnsiConsole.MarkupLine("[green1]Done[/]");

            // Update the ini files that are being used by apache and cli
            PhpHelper.UpdatePhpIniFiles(isDebug);
        }
    }
}