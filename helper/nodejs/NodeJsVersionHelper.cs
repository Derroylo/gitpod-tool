using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Gitpod.Tool.Helper.Internal.Config.Sections;
using Spectre.Console;

namespace Gitpod.Tool.Helper.NodeJs
{
    partial class NodeJsVersionHelper
    {  
        public static string GetCurrentNodeJSVersionOutput()
        {
            return ExecCommand.Exec("node -v");
        }

        public static string GetCurrentNodeJSVersion()
        {
            string output = GetCurrentNodeJSVersionOutput();

            Match match = NodeJsVersionMatchRegex().Match(output);

            if (!match.Success) {
                throw new Exception("Failed to parse the node version command output to find the active version.");
            }

            return output;
        }

        [GeneratedRegex(@"v([0-9]+).([0-9]+).([0-9]+)")]
        private static partial Regex NodeJsVersionMatchRegex();

        public static List<string> GetAvailableNodeJSVersions()
        {
            var availableNodeJSVersions = new List<string>();

            string pattern = @"v(([0-9]+).([0-9]+).([0-9]+))";
            string input = ExecCommand.Exec("source \"$NVM_DIR\"/nvm-lazy.sh && nvm ls-remote");

            RegexOptions options = RegexOptions.Multiline;
        
            foreach (Match m in Regex.Matches(input, pattern, options))
            {
                if (!m.Groups.ContainsKey("1") || m.Groups[1].ToString().Length < 3) {
                    continue;
                }

                availableNodeJSVersions.Insert(0, m.Groups[1].ToString());
            }

            return availableNodeJSVersions;
        }

        public static void SetNewNodeJSVersion(string newVersion, bool isDebug)
        {
            AnsiConsole.Status()
                .Start("Changing to NodeJS " + newVersion + ", this can take a few mins.", ctx => 
                {
                    var currentVersion = GetCurrentNodeJSVersion();

                    if (currentVersion == newVersion)                 {
                        AnsiConsole.WriteLine("NodeJS " + newVersion + " is already the current active one.");

                        return;
                    }

                    ctx.Status("Loading installed packages...");

                    // Get the installed packages for the current nodejs version
                    var installedPackages = NodeJsPackageHelper.GetCurrentInstalledNodeJSPackages();

                    ctx.Status("Switching to new nodejs version...");

                    ExecCommand.Exec("source \"$NVM_DIR\"/nvm-lazy.sh && nvm install " + newVersion, 300);

                    // Write the selected version to a file, so we can change the active nodejs version via the gpt.sh script
                    var applicationDir = AppDomain.CurrentDomain.BaseDirectory;
                    File.WriteAllText(applicationDir + ".nodejs", newVersion);

                    ctx.Status("Loading installed packages in the new nodejs version...");

                    // Fetch the packages installed in the new version
                    // We have to set the version again, otherwise it will just show the old nodejs output
                    var listPackagesOutput = ExecCommand.Exec(". ~/.nvm/nvm.sh && nvm use " + newVersion + " && nvm alias default " + newVersion + " && npm list -g --depth=0", 300);
                    var installedPackagesNew = NodeJsPackageHelper.GetCurrentInstalledNodeJSPackages(listPackagesOutput);

                    var missingPackages = installedPackages.Where(p => !installedPackagesNew.Contains(p)).ToList();

                    if (missingPackages.Count > 0) {
                        ctx.Status("Installing packages that are missing in the new version...");

                        ExecCommand.Exec(". ~/.nvm/nvm.sh && nvm use " + newVersion + " && nvm alias default " + newVersion + " && npm install -g " + string.Join(" ", missingPackages), 300);
                    }

                    ctx.Status("Saving the new active version so it can be restored...");

                    try {
                        NodeJsConfig.NodeJsVersion = newVersion;

                        AnsiConsole.MarkupLine("Saving the new active version so it can be restored...[green1]Done[/]");
                    } catch {
                        AnsiConsole.MarkupLine("Saving the new active version so it can be restored...[red]Failed[/]");
                    }

                    AnsiConsole.WriteLine("NodeJS has been set to " + newVersion);
                    AnsiConsole.WriteLine("You may need to reload the terminal(execute 'source ~/.bashrc') or open a new terminal to change to the new active version!");
                });
        }
    }
}