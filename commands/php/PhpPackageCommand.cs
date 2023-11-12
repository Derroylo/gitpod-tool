using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Gitpod.Tool.Helper;
using Gitpod.Tool.Helper.Php;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Gitpod.Tool.Commands.Php
{
    class PhpPackageCommand : Command<PhpPackageCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("-d|--debug")]
            [Description("Outputs debug information")]
            [DefaultValue(false)]
            public bool Debug { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            // Read currently installed packages
            var installedPackages = ExecCommand.Exec("apt list --installed");

            var packagesList = installedPackages.Split("\n");

            // Filter the list so that we only show php packages
            var phpPackages = packagesList.Where(c => c.StartsWith("php") && c.Contains('-')).ToArray();

            List<string> phpPackagesCleaned = new();

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

            var currentPhpVersion = PhpVersionHelper.GetCurrentPhpVersion();

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

                    PhpPackagesHelper.InstallPackages(selections.ToArray(), currentPhpVersion, settings.Debug);
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

            PhpPackagesHelper.InstallPackages(newPackages.Split(" "), currentPhpVersion, settings.Debug);

            return 0;
        }
    }
}
