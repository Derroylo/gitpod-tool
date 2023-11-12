using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gitpod.Tool.Helper.Internal.Config;
using Gitpod.Tool.Helper.Internal.Config.Sections;
using Spectre.Console;

namespace Gitpod.Tool.Helper.Php
{
    class PhpIniHelper
    {
        public static string GetPhpIniPath()
        {
            return ExecCommand.Exec("php -i | grep 'Configuration File'");
        }

        public static void UpdatePhpIniFiles(bool isDebug)
        {
            string currentPhpVersion = PhpVersionHelper.GetCurrentPhpVersion();

            if (isDebug) {
                AnsiConsole.WriteLine("Active PHP Version " + currentPhpVersion);
            }

            AnsiConsole.Write("Generating custom.ini....");

            var customIniFiles = GenerateCustomIniFilesFromConfig();

            if (customIniFiles.Count == 0) {
                AnsiConsole.MarkupLine("[cyan3]No custom settings found[/]");

                return;
            }

            AnsiConsole.MarkupLine("[green1]Done[/]");
           
            if (customIniFiles.TryGetValue("cli", out string customIniFileCli)) {
                AnsiConsole.Write("Copy the custom_cli.ini to the target directory....");

                string targetFile = "/etc/php/" + currentPhpVersion + "/cli/conf.d/custom.ini";
                
                if (isDebug) {
                    AnsiConsole.WriteLine("Copying from \"" + customIniFileCli + "\" to \"" + targetFile + "\"");
                }
                
                ExecCommand.Exec("sudo cp " + customIniFileCli + " " + targetFile);
            }

            if (customIniFiles.TryGetValue("web", out string customIniFileWeb)) {
                string targetFile = "/etc/php/" + currentPhpVersion + "/apache2/conf.d/custom.ini";
                
                if (isDebug) {
                    AnsiConsole.WriteLine("Copying from \"" + customIniFileWeb + "\" to \"" + targetFile + "\"");
                }
                
                ExecCommand.Exec("sudo cp " + customIniFileWeb + " " + targetFile);

                targetFile = "/etc/php/" + currentPhpVersion + "/apache2/conf.d/custom.ini";
                
                if (isDebug) {
                    AnsiConsole.WriteLine("Copying from \"" + customIniFileWeb + "\" to \"" + targetFile + "\"");
                }
                
                ExecCommand.Exec("sudo cp " + customIniFileWeb + " " + targetFile);

                // Restart apache when changes has been made to the web php.ini files (doing this here as this one is used be restore php command)
                ExecCommand.Exec("apachectl stop");
                ExecCommand.Exec("apachectl start");
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
                if (PhpConfig.Config.ContainsKey(name)) {
                    PhpConfig.Config[name] = value;
                } else {
                    PhpConfig.Config.Add(name, value);
                }
            }

            if (setForWeb) {
                if (PhpConfig.ConfigWeb.ContainsKey(name)) {
                    PhpConfig.ConfigWeb[name] = value;
                } else {
                    PhpConfig.ConfigWeb.Add(name, value);
                }
            }

            if (setForCLI) {
                if (PhpConfig.ConfigCli.ContainsKey(name)) {
                    PhpConfig.ConfigCli[name] = value;
                } else {
                    PhpConfig.ConfigCli.Add(name, value);
                }
            }

            // Manually set that the config has been updated
            ConfigHelper.ConfigUpdated = true;

            // Update the ini files that are being used by apache and cli
            UpdatePhpIniFiles(isDebug);
        }

        private static Dictionary<string, string> GenerateCustomIniFilesFromConfig()
        {
            var applicationDir = AppDomain.CurrentDomain.BaseDirectory;
            var iniSettingsFolder = applicationDir + "/php";
            var customFilesWithPath = new Dictionary<string, string>();

            if (!Directory.Exists(iniSettingsFolder)) {
                Directory.CreateDirectory(iniSettingsFolder);
            }

            if (PhpConfig.Config.Count == 0 && PhpConfig.ConfigWeb.Count == 0 && PhpConfig.ConfigCli.Count == 0) {
                return customFilesWithPath;
            }

            // Combine config and configWeb to a custom.ini for web
            var configWeb = new Dictionary<string, string>(PhpConfig.ConfigWeb);
            var customWeb = new Dictionary<string, string>(PhpConfig.Config);

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

            string customIniContent = string.Empty;

            foreach (KeyValuePair<string, string> item in customWeb) {
                customIniContent += item.Key + " = " + item.Value + "\n";
            }

            File.WriteAllText(iniSettingsFolder + "/custom_web.ini", customIniContent);
            customFilesWithPath.Add("web", iniSettingsFolder + "/custom_web.ini");

            // Combine config and configCLI to a custom.ini for CLI
            var configCLI = new Dictionary<string, string>(PhpConfig.ConfigCli);
            var customCLI = new Dictionary<string, string>(PhpConfig.Config);

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

            customIniContent = string.Empty;

            foreach (KeyValuePair<string, string> item in customCLI) {
                customIniContent += item.Key + " = " + item.Value + "\n";
            }

            File.WriteAllText(iniSettingsFolder + "/custom_cli.ini", customIniContent);
            customFilesWithPath.Add("cli", iniSettingsFolder + "/custom_cli.ini");

            return customFilesWithPath;
        }
    }
}
