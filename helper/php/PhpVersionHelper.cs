using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Gitpod.Tool.Helper.Php
{
    class PhpVersionHelper
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

        public static string GetCurrentPhpVersion()
        {
            string output = GetCurrentPhpVersionOutput();

            Regex regex = new(@"(?:PHP) ([(0-9)].[(0-9)])");
            Match match = regex.Match(output);

            if (!match.Success) {
                throw new Exception("Failed to parse the php version command output to find the active version.");
            }

            return match.Groups[1].Value;
        }

        public static string GetCurrentPhpVersionOutput()
        {
            return ExecCommand.Exec("php -version");
        }
    }
}
