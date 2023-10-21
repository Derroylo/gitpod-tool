using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using Spectre.Console;

namespace Gitpod.Tool.Helper.Php
{
    class DebugHelper
    {
        public static Dictionary<string, string> GetCurrentSettings()
        {
            Dictionary<string, string> currentSettings = new() {{"web", ""}, {"cli", ""}};

            AnsiConsole.Status()
                .Start("Reading debug config", ctx => 
                {
                    ctx.Status("Checking install status for CLI");

                    var result = ExecCommand.Exec("php -r 'xdebug_info();'");
                    
                    if (result.Contains("Call to undefined function xdebug_info()")) {
                        currentSettings["cli"] = "Not installed/inactive";
                    }

                    if (currentSettings["cli"] == "") {
                        string pattern = @"xdebug\.mode => ([a-z ]+) => ([a-z ]+)";

                        RegexOptions options = RegexOptions.Multiline;
        
                        var matches = Regex.Matches(result, pattern, options);

                        if (matches.Count > 0) {
                            currentSettings["cli"] = matches[0].Groups[1].Value;

                            if (currentSettings["cli"] == "no value") {
                                currentSettings["cli"] = "off";
                            }
                        }
                    }

                    if (currentSettings["cli"] == "") {
                        currentSettings["cli"] = "Unknown";
                    }

                    ctx.Status("Checking install status for WEB");

                    try {
                        var webClient = new WebClient();
                        //var response = File.ReadAllText("phpinfo.txt");
                        var response = webClient.DownloadString("http://localhost:8080/phpinfo");

                        var patternWeb = """<tr><td class="e">xdebug\.mode<\/td><td class="v">(?:<i>)?([a-z ]+)(?:<\/i>)?<\/td><td class="v">(?:<i>)?([a-z ]+)(?:<\/i>)?<\/td><\/tr>""";

                        RegexOptions optionsWeb = RegexOptions.Multiline;
        
                        var matchesWeb = Regex.Matches(response.ToString(), patternWeb, optionsWeb);

                        if (matchesWeb.Count > 0) {
                            currentSettings["web"] = matchesWeb[0].Groups[1].Value;

                            if (currentSettings["web"] == "no value") {
                                currentSettings["web"] = "off";
                            }
                        }                       
                    } catch (Exception e) {
                        currentSettings["web"] = "Unknown";
                    }
                    
                    if (currentSettings["web"] == "") {
                        currentSettings["web"] = "Unknown";
                    }
                }
            );

            return currentSettings;
        }
    }
}