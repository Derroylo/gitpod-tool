namespace Gitpod.Tool.Helper.Internal.Config
{
    class NodeJsConfig: AbstractConfig
    {
        public static string NodeJsVersion
        {
            get {
                if (!IsConfigFileLoaded) {
                    ReadConfigFile();
                }

                return appConfig.Nodejs.Version;
            }

            set {
                appConfig.Nodejs.Version = value;
            }
        }
    }
}