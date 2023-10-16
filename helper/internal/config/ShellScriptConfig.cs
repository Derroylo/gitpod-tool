using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Gitpod.Tool.Classes.Configuration;

namespace Gitpod.Tool.Helper.Internal.Config
{
    class ShellScriptConfig: AbstractConfig
    {
        public static List<string> AdditionalDirectories
        {
            get {
                if (!IsConfigFileLoaded) {
                    ReadConfigFile();
                }

                return appConfig.ShellScripts.AdditionalDirectories;
            }
        }
    }
}