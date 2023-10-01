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
    class PhpHelper
    {  
        public static List<string> GetAvailablePhpVersions()
        {
            var availablePhpVersions = new List<string>();

            string pattern = @"Alternative: \/usr\/bin\/php([0-9.]+)";
            string input = ExecCommand.Exec("update-alternatives --query php");

            RegexOptions options = RegexOptions.Multiline;
        
            foreach (Match m in Regex.Matches(input, pattern, options))
            {
                if (!m.Groups.ContainsKey("1") || m.Groups[1].ToString().Length < 3) {
                    continue;
                }

                availablePhpVersions.Insert(0, m.Groups[1].ToString());
            }

            return availablePhpVersions;
        }

        public static void SetNewPhpVersion(string newVersion, bool isDebug)
        {
            AnsiConsole.Status()
                .Start("Setting PHP Version to " + newVersion, ctx => 
                {
                    var availablePhpVersions = PhpHelper.GetAvailablePhpVersions();

                    if (!availablePhpVersions.Contains(newVersion)) {
                        AnsiConsole.MarkupLine("Checking if the input is a valid php version....[red]Invalid[/]");

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
                    
                    // Restarting Apache
                    ExecCommand.Exec("apachectl stop");
                    ExecCommand.Exec("apachectl start");
                    
                    AnsiConsole.MarkupLine("Restarting apache...[green1]Success[/]");

                    //ctx.Status("Validating that the new version has been set...");

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

                    if (GptConfigHelper.Config?.Php?.Packages?.Count > 0) {
                        var updateRes = ExecCommand.Exec("sudo apt-get update");
                        AnsiConsole.MarkupLine("Updating package manager list...[green1]Done[/]");

                        if (isDebug) {
                            AnsiConsole.WriteLine(updateRes);
                        }

                        string packages = string.Join(" ", GptConfigHelper.Config.Php.Packages).Replace("VERSION", newVersion).Replace("php-", "php" + newVersion + "-");

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
                        if (GptConfigHelper.Config == null) {
                            GptConfigHelper.Config = new Classes.Configuration.Configuration();
                        }

                        if (GptConfigHelper.Config.Php == null) {
                            GptConfigHelper.Config.Php = new Classes.Configuration.PhpConfiguration();
                        }

                        GptConfigHelper.Config.Php.Version = newVersion;
                        GptConfigHelper.WriteConfigFile();

                        AnsiConsole.MarkupLine("Saving the new active version so it can be restored...[green1]Done[/]");
                    } catch {
                        AnsiConsole.MarkupLine("Saving the new active version so it can be restored...[red]Failed[/]");
                    }
                });

            AnsiConsole.MarkupLine("PHP Version has been set to " + newVersion);
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

            AnsiConsole.Write("Generating custom.ini....");

            var customIniFiles = PhpHelper.GenerateCustomIniFilesFromConfig();

            if (customIniFiles.Count == 0) {
                AnsiConsole.MarkupLine("[cyan3]No custom settings found[/]");

                return;
            }

            AnsiConsole.MarkupLine("[green1]Done[/]");
           
            if (customIniFiles.ContainsKey("cli")) {
                AnsiConsole.Write("Copy the custom_cli.ini to the target directory....");

                string targetFile = "/etc/php/" + currentPhpVersion + "/cli/conf.d/custom.ini";
                
                if (isDebug) {
                    AnsiConsole.WriteLine("Copying from \"" + customIniFiles["cli"] + "\" to \"" + targetFile + "\"");
                }
                
                ExecCommand.Exec("sudo cp " + customIniFiles["cli"] + " " + targetFile);
            }

            if (customIniFiles.ContainsKey("web")) {
                string targetFile = "/etc/php/" + currentPhpVersion + "/apache2/conf.d/custom.ini";
                
                if (isDebug) {
                    AnsiConsole.WriteLine("Copying from \"" + customIniFiles["web"] + "\" to \"" + targetFile + "\"");
                }
                
                ExecCommand.Exec("sudo cp " + customIniFiles["web"] + " " + targetFile);

                targetFile = "/etc/php/" + currentPhpVersion + "/apache2/conf.d/custom.ini";
                
                if (isDebug) {
                    AnsiConsole.WriteLine("Copying from \"" + customIniFiles["web"] + "\" to \"" + targetFile + "\"");
                }
                
                ExecCommand.Exec("sudo cp " + customIniFiles["web"] + " " + targetFile);
            }

            AnsiConsole.MarkupLine("[green1]Done[/]");
        }

        public static void AddSettingToPhpIni(string name, string value, bool setForWeb = false, bool setForCLI = false, bool isDebug = false)
        {
            if (name == null || name.Length <= 3 || value == null || value.Length < 1) {
                AnsiConsole.MarkupLine("[red]Invalid name and/or value[/]");

                return;
            }

            if (!setForWeb && !setForCLI) {
                if (GptConfigHelper.Config.Php.Config.ContainsKey(name)) {
                    GptConfigHelper.Config.Php.Config[name] = value;
                } else {
                    GptConfigHelper.Config.Php.Config.Add(name, value);
                }
            }

            if (setForWeb) {
                if (GptConfigHelper.Config.Php.ConfigWeb.ContainsKey(name)) {
                    GptConfigHelper.Config.Php.ConfigWeb[name] = value;
                } else {
                    GptConfigHelper.Config.Php.ConfigWeb.Add(name, value);
                }
            }

            if (setForCLI) {
                if (GptConfigHelper.Config.Php.ConfigCLI.ContainsKey(name)) {
                    GptConfigHelper.Config.Php.ConfigCLI[name] = value;
                } else {
                    GptConfigHelper.Config.Php.ConfigCLI.Add(name, value);
                }
            }

            GptConfigHelper.WriteConfigFile();

            // Update the ini files that are being used by apache and cli
            PhpHelper.UpdatePhpIniFiles(isDebug);

            if (setForWeb) {
                ExecCommand.Exec("apachectl restart");
            }
        }

        private static Dictionary<string, string> GenerateCustomIniFilesFromConfig()
        {
            var applicationDir = AppDomain.CurrentDomain.BaseDirectory;
            var iniSettingsFolder = applicationDir + "/php";
            var customFilesWithPath = new Dictionary<string, string>();

            if (!Directory.Exists(iniSettingsFolder)) {
                Directory.CreateDirectory(iniSettingsFolder);
            }

            if (GptConfigHelper.Config.Php.Config.Count == 0 && GptConfigHelper.Config.Php.ConfigCLI.Count == 0 && GptConfigHelper.Config.Php.ConfigWeb.Count == 0) {
                return customFilesWithPath;
            }

            // Combine config and configWeb to a custom.ini for web
            var configWeb = new Dictionary<string, string>(GptConfigHelper.Config.Php.ConfigWeb);
            var customWeb = new Dictionary<string, string>(GptConfigHelper.Config.Php.Config);

            foreach (KeyValuePair<string, string> item in customWeb) {
                if (configWeb.ContainsKey(item.Key)) {
                    customWeb[item.Key] = configWeb[item.Key];
                    configWeb.Remove(item.Key);
                }
            }

            if (configWeb.Count > 0) {
                customWeb = customWeb.Union(configWeb)
                                .ToLookup(x => x.Key, x => x.Value)
                                .ToDictionary(x => x.Key, g => g.First());
            }

            string customIniContent = String.Empty;

            foreach (KeyValuePair<string, string> item in customWeb) {
                customIniContent += item.Key + " = " + item.Value + "\n";
            }

            File.WriteAllText(iniSettingsFolder + "/custom_web.ini", customIniContent);
            customFilesWithPath.Add("web", iniSettingsFolder + "/custom_web.ini");

            // Combine config and configCLI to a custom.ini for CLI
            var configCLI = new Dictionary<string, string>(GptConfigHelper.Config.Php.ConfigCLI);
            var customCLI = new Dictionary<string, string>(GptConfigHelper.Config.Php.Config);

            foreach (KeyValuePair<string, string> item in customCLI) {
                if (configCLI.ContainsKey(item.Key)) {
                    customCLI[item.Key] = configCLI[item.Key];
                    configCLI.Remove(item.Key);
                }
            }

            if (configCLI.Count > 0) {
                customCLI = customCLI.Union(configCLI)
                                .ToLookup(x => x.Key, x => x.Value)
                                .ToDictionary(x => x.Key, g => g.First());
            }

            customIniContent = String.Empty;

            foreach (KeyValuePair<string, string> item in customCLI) {
                customIniContent += item.Key + " = " + item.Value + "\n";
            }

            File.WriteAllText(iniSettingsFolder + "/custom_cli.ini", customIniContent);
            customFilesWithPath.Add("cli", iniSettingsFolder + "/custom_cli.ini");

            return customFilesWithPath;
        }
    }
}