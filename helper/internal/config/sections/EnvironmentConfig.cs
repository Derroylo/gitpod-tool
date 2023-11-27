using System.Collections.Generic;

namespace Gitpod.Tool.Helper.Internal.Config.Sections
{
    class EnvironmentConfig: ConfigHelper
    {
        public static Dictionary<string, string> Variables
        {
            get {
                return appConfig.Env.Vars;
            }

            set {
                ConfigUpdated = true;

                appConfig.Env.Vars = value;
            }
        }

        public static Dictionary<string, Dictionary<string, string>> Files
        {
            get {
                return appConfig.Env.Files;
            }

            set {
                ConfigUpdated = true;

                appConfig.Env.Files = value;
            }
        }
    }
}