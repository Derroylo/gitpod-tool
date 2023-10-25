using System.Collections.Generic;

namespace Gitpod.Tool.Helper.Internal.Config.Sections
{
    class ShellScriptConfig: ConfigHelper
    {
        public static List<string> AdditionalDirectories
        {
            get {
                return appConfig.ShellScripts.AdditionalDirectories;
            }
        }
    }
}