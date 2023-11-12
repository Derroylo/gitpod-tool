using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Gitpod.Tool.Helper.Internal.Config.Sections;
using Spectre.Console;

namespace Gitpod.Tool.Helper.NodeJs
{
    partial class NodeJsPackageHelper
    {  
        [GeneratedRegex(@"([a-z0-9\-]+)@([0-9.]+)")]
        private static partial Regex NodeJsPackageMatchRegex();

        public static List<string> GetCurrentInstalledNodeJSPackages(string packageListOutput = null)
        {
            packageListOutput ??= ExecCommand.Exec("npm list -g --depth=0");

            List<string> packages = packageListOutput.Split("\n").ToList();

            List<string> filteredPackages = new();

            foreach (string package in packages) {
                Match match = NodeJsPackageMatchRegex().Match(package);

                if (match.Success && match.Groups[1].Value != "npm" && match.Groups[1].Value != "corepack") {
                    filteredPackages.Add(match.Groups[1].Value);
                }
            }

            return filteredPackages;
        }

        public static void InstallPackages(string[] newPackages, bool debug = false)
        {
            string packages = string.Join(" ", newPackages);

            var installRes = ExecCommand.Exec("npm install -g " + packages);
            AnsiConsole.MarkupLine("Installing packages...[green1]Done[/]");
            
            if (debug) {
                AnsiConsole.WriteLine(installRes);
            }
        }
    }
}