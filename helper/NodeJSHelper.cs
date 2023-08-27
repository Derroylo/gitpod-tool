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
    class NodeJSHelper
    {  
        public static string GetCurrentNodeJSVersionOutput()
        {
            return ExecCommand.Exec("node -v");
        }

        public static string GetCurrentNodeJSVersion()
        {
            string output = NodeJSHelper.GetCurrentNodeJSVersionOutput();

            Regex regex = new Regex(@"v([0-9]+).([0-9]+).([0-9]+)");
            Match match = regex.Match(output);

            if (!match.Success) {
                throw new Exception("Failed to parse the node version command output to find the active version.");
            }

            return output;
        }

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
                    ExecCommand.Exec("source \"$NVM_DIR\"/nvm-lazy.sh && nvm install " + newVersion);

                    // Write the selected version to a file, so we can change the active nodejs version via the gpt.sh script
                    var applicationDir = AppDomain.CurrentDomain.BaseDirectory;
                    File.WriteAllText(applicationDir + ".nodejs", newVersion);

                    AnsiConsole.WriteLine("NodeJS has been set to " + newVersion);
                    AnsiConsole.WriteLine("You may need to reload the terminal or open a new one to change to the new active version!");
                });
        }
    }
}