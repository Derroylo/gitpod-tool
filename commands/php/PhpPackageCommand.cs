using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Gitpod.Tool.Helper;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Gitpod.Tool.Commands.Php
{
    class PhpPackageCommand : Command<PhpPackageCommand.Settings>
    {
        private Settings settings;

        public class Settings : CommandSettings
        {
            [CommandOption("-d|--debug")]
            [Description("Outputs debug information")]
            [DefaultValue(false)]
            public bool Debug { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            this.settings = settings;

            // Read currently installed packages
            var installedPackages = ExecCommand.Exec("apt list --installed");

            var packagesList = installedPackages.Split("\n");

            // Filter the list so that we only show php packages
            var phpPackages = packagesList.Where(c => c.StartsWith("php") && c.Contains("-")).ToArray();

            List<string> phpPackagesCleaned = new List<string>();

            foreach (string package in phpPackages) {
                var tmp = package.Split("/");

                phpPackagesCleaned.Add(tmp[0].Trim());
            }

            // Show a list of the installed packages
            AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Currently installed php packages")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Move up and down to show more packages)[/]")
                    .AddChoices(phpPackagesCleaned.ToArray())
            );

            if (!AnsiConsole.Confirm("Do you want to install other packages?", false)) {
                return 0;
            }

            var currentPhpVersion = PhpHelper.GetCurrentPhpVersion();

            var recommendedPackages = new List<string> {
                "php-bcmath",
                "php-curl",
                "php-dom",
                "php-fileinfo",
                "php-gd",
                "php-iconv",
                "php-imagick",
                "php-intl",
                "php-json",
                "php-libxml",
                "php-mbstring",
                "php-openssl",
                "php-pcre",
                "php-pdo",
                "php-pdo_mysql",
                "php-phar",
                "php-redis",
                "php-simplexml",
                "php-soap",
                "php-sodium",
                "php-tokenizer",
                "php-xml",
                "php-zip",
                "php-zlib"
            };

            // Filter the list of recommended packages with the ones that are already installed
            var filteredPackages = recommendedPackages.Where(p => !phpPackagesCleaned.Contains(p) && !phpPackagesCleaned.Contains(p.Replace("php-", "php" + currentPhpVersion + "-"))).ToArray();

            if (filteredPackages.Length > 0 && AnsiConsole.Confirm("Found some commonly used packages that are not installed yet. Do you want to select one of them?", false)) {
                var selections = AnsiConsole.Prompt(
                    new MultiSelectionPrompt<string>()
                        .Title("Recommended php packages")
                        .PageSize(10)
                        .MoreChoicesText("[grey](Move up and down to show more packages)[/]")
                        .AddChoices(filteredPackages)
                );

                if (selections.Count > 0) {
                    AnsiConsole.WriteLine("Installing the selected packages");

                    this.InstallPackages(selections.ToArray(), currentPhpVersion);
                }
            }

            if (!AnsiConsole.Confirm("Do you want to install packages that were not listed before?", false)) {
                return 0;
            }
            
            var newPackages = AnsiConsole.Prompt(
                new TextPrompt<string>("Name of the package (separate multiple packages with a space): ")
            );

            if (newPackages.Length == 0) {
                AnsiConsole.WriteLine("No packages entered");

                return 0;
            }

            this.InstallPackages(newPackages.Split(" "), currentPhpVersion);

            return 0;
        }

        private void InstallPackages(string[] newPackages, string phpVersion)
        {
            var updateRes = ExecCommand.Exec("sudo apt-get update");
            AnsiConsole.MarkupLine("Updating package manager list...[green1]Done[/]");

            if (this.settings.Debug) {
                AnsiConsole.WriteLine(updateRes);
            }

            string packages = string.Join(" ", newPackages).Replace("php-", "php" + phpVersion + "-");

            var installRes = ExecCommand.Exec("sudo apt-get install -y " + packages);
            AnsiConsole.MarkupLine("Installing packages...[green1]Done[/]");
            
            if (this.settings.Debug) {
                AnsiConsole.WriteLine(installRes);
            }

            this.SavePackagesInConfig(newPackages);
        }

        private void SavePackagesInConfig(string[] packages)
        {
            if (GptConfigHelper.Config == null) {
                GptConfigHelper.Config = new Classes.Configuration.Configuration();
            }

            if (GptConfigHelper.Config.Php == null) {
                GptConfigHelper.Config.Php = new Classes.Configuration.PhpConfiguration();
            }

            foreach (string package in packages) {
                if (!GptConfigHelper.Config.Php.Packages.Contains(package)) {
                    GptConfigHelper.Config.Php.Packages.Add(package);
                }
            }

            GptConfigHelper.WriteConfigFile();            
        }
    }
}