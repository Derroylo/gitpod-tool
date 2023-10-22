using System;
using System.Collections.Generic;
using System.Linq;
using Gitpod.Tool.Helper.Internal.Config.Sections;
using Spectre.Console;

namespace Gitpod.Tool.Helper.Php
{
    class PhpHelper
    {  
        public static void SetNewPhpVersion(string newVersion, bool isDebug)
        {
            AnsiConsole.Status()
                .AutoRefresh(true)
                .Start("Setting PHP Version to " + newVersion, ctx => 
                {
                    var availablePhpVersions = PhpVersionHelper.GetAvailablePhpVersions();

                    if (!availablePhpVersions.Contains(newVersion)) {
                        AnsiConsole.MarkupLine("Checking if the input is a valid php version....[red]Invalid[/]");

                        return;
                    }

                    AnsiConsole.MarkupLine("Checking if the input is a valid php version....[green1]Valid[/]");

                    string currentPhpVersion = PhpVersionHelper.GetCurrentPhpVersion();

                    // Check if we have selected another version as the currently active one
                    if (newVersion != currentPhpVersion) {
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
                    }

                    string testResult = PhpVersionHelper.GetCurrentPhpVersionOutput();

                    if (isDebug) {
                        AnsiConsole.WriteLine(testResult);
                    }

                    if (!testResult.Contains(newVersion)) {
                        AnsiConsole.MarkupLine("Validating that the new version has been set....[red]Failed[/]");

                        return;
                    }

                    AnsiConsole.MarkupLine("Validating that the new version has been set...[green1]Success[/]");

                    ctx.Status("Checking if the active php version has the same packages installed as the newest one...");

                    // Check if the newVersion is not the latest one and verify all packages are installed in the newVersion too
                    if (newVersion != availablePhpVersions[0]) {
                        // Read currently installed packages
                        var installedPackages = ExecCommand.Exec("apt list --installed");

                        var packagesList = installedPackages.Split("\n");
                        var packagesCleaned = new List<string>();

                        foreach (string package in packagesList) {
                            var tmp = package.Split("/");

                            packagesCleaned.Add(tmp[0].Trim());
                        }

                        var latestPackages = packagesCleaned.Where(p => p.Contains("php" + availablePhpVersions[0] + "-")).ToArray();

                        var newVersionPackages = packagesCleaned.Where(p => p.Contains("php" + newVersion + "-")).ToArray();

                        var missingPackages = latestPackages.Where(p => !newVersionPackages.Contains(p.Replace("php" + availablePhpVersions[0] + "-", "php" + newVersion + "-"))).ToArray();

                        if (missingPackages.Length > 0) {
                            var updateRes = ExecCommand.Exec("sudo apt-get update");
                            AnsiConsole.MarkupLine("Updating package manager list...[green1]Done[/]");

                            if (isDebug) {
                                AnsiConsole.WriteLine(updateRes);
                            }

                            string packages = string.Join(" ", missingPackages).Replace("php" + availablePhpVersions[0] + "-", "php" + newVersion + "-");

                            var installRes = ExecCommand.Exec("sudo apt-get install -y " + packages);
                            AnsiConsole.MarkupLine("Installing packages...[green1]Done[/]");
                            
                            if (isDebug) {
                                AnsiConsole.WriteLine(installRes);
                            }

                            // Restart webserver so newly installed packages are available
                            ExecCommand.Exec("apachectl stop");
                            ExecCommand.Exec("apachectl start");
                            AnsiConsole.MarkupLine("Restarting apache...[green1]Success[/]");
                        }
                    }

                    ctx.Status("Checking if additional packages are defined in the config file...");

                    if (PhpConfig.Packages.Count > 0) {
                        // Read currently installed packages
                        var installedPackages = ExecCommand.Exec("apt list --installed");

                        var packagesList = installedPackages.Split("\n");
                        var packagesCleaned = new List<string>();

                        foreach (string package in packagesList) {
                            var tmp = package.Split("/");

                            packagesCleaned.Add(tmp[0].Trim());
                        }

                        var packagesToCheck = PhpConfig.Packages;

                        // Check if packages from the config file are not already installed
                        var packagesToInstall = packagesToCheck.Where(p => !packagesCleaned.Contains(p.Replace("VERSION", newVersion).Replace("php-", "php" + newVersion + "-"))).ToArray();

                        if (packagesToInstall.Length > 0) {
                            var updateRes = ExecCommand.Exec("sudo apt-get update");
                            AnsiConsole.MarkupLine("Updating package manager list...[green1]Done[/]");

                            if (isDebug) {
                                AnsiConsole.WriteLine(updateRes);
                            }

                            string packages = string.Join(" ", packagesToInstall).Replace("VERSION", newVersion).Replace("php-", "php" + newVersion + "-");

                            var installRes = ExecCommand.Exec("sudo apt-get install -y " + packages);
                            AnsiConsole.MarkupLine("Installing packages...[green1]Done[/]");
                            
                            if (isDebug) {
                                AnsiConsole.WriteLine(installRes);
                            }

                            // Restart webserver so newly installed packages are available
                            ExecCommand.Exec("apachectl stop");
                            ExecCommand.Exec("apachectl start");
                            AnsiConsole.MarkupLine("Restarting apache...[green1]Success[/]");
                        }
                    } else {
                        AnsiConsole.MarkupLine("Checking if additional packages are defined in the config file...[cyan3]Not found[/]");
                    }

                    ctx.Status("Saving the new active version so it can be restored...");

                    try {
                        PhpConfig.PhpVersion  = newVersion;

                        AnsiConsole.MarkupLine("Saving the new active version so it can be restored...[green1]Done[/]");
                    } catch {
                        AnsiConsole.MarkupLine("Saving the new active version so it can be restored...[red]Failed[/]");
                    }
                });

            AnsiConsole.MarkupLine("PHP Version has been set to " + newVersion);
        }
    }
}
