using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gitpod.Tool.Helper.Internal.Config.Sections;
using Spectre.Console;

namespace Gitpod.Tool.Helper.Env
{
    class EnvRestoreHelper
    {
        public static void RestoreEnvironmentVariables(bool debug = false)
        {
            if (EnvironmentConfig.Variables.Count == 0) {
                return;
            }

            var applicationDir = AppDomain.CurrentDomain.BaseDirectory;

            List<string> envVariables = new() {
                {"#!/usr/bin/env bash"}
            };
            
            foreach (KeyValuePair<string, string> entry in EnvironmentConfig.Variables) {
                envVariables.Add("export " + entry.Key + "=\"" + entry.Value + "\"");
            }

            File.WriteAllText(applicationDir + ".env_restore", string.Join("\n", envVariables.ToArray<string>()));
        }

        public static void RestoreEnvironmentFiles(bool debug = false)
        {
            if (EnvironmentConfig.Files.Count == 0) {
                return;
            }

            foreach (KeyValuePair<string, Dictionary<string, string>> entry in EnvironmentConfig.Files) {
                entry.Value.TryGetValue("file", out string outputFileName);

                if (outputFileName == null || outputFileName == string.Empty) {
                    AnsiConsole.MarkupLine("[red]Missing value \"file\" for restoring env " + entry.Key + "[/]");
                    
                    continue;
                }

                entry.Value.TryGetValue("content", out string encodedFileContent);
                entry.Value.TryGetValue("gpVariable", out string gpVariable);

                if (encodedFileContent == null && gpVariable != null) {
                    encodedFileContent = Environment.GetEnvironmentVariable(gpVariable);
                }

                if (encodedFileContent != null && encodedFileContent != string.Empty) {
                    string decodedFileString = string.Empty;

                    try {
                        decodedFileString = Encoding.UTF8.GetString(Convert.FromBase64String(encodedFileContent));
                    } catch (Exception e) {
                        AnsiConsole.WriteException(e);

                        continue;
                    }

                    File.WriteAllText(outputFileName, decodedFileString);
                }
            }
        }
    }
}
