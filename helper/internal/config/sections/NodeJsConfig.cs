using Spectre.Console;

namespace Gitpod.Tool.Helper.Internal.Config.Sections
{
    class NodeJsConfig: ConfigHelper
    {
        public static string NodeJsVersion
        {
            get {
                return appConfig.Nodejs.Version;
            }

            set {
                if (appConfig.Nodejs.Version != value) {
                    ConfigUpdated = true;
                }

                appConfig.Nodejs.Version = value;
            }
        }
    }
}