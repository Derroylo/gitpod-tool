using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gitpod.Tool.Helper.Internal.Config.Sections;

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

            List<string> envVariables = new();
            envVariables.Add("#!/usr/bin/env bash");
            
            foreach (KeyValuePair<string, string> entry in EnvironmentConfig.Variables) {
                envVariables.Add("export " + entry.Key + "=\"" + entry.Value + "\"");
            }

            File.WriteAllText(applicationDir + ".env_restore", string.Join("\n", envVariables.ToArray<string>()));
        }
    }
}
