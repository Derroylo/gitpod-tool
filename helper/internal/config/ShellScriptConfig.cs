using System.Collections.Generic;

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