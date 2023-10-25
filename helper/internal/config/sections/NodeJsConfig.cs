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
                appConfig.Nodejs.Version = value;
            }
        }
    }
}