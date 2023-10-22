using System.Collections.Generic;

namespace Gitpod.Tool.Helper.Internal.Config.Sections
{
    class ShellScriptConfig: ConfigHelper
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