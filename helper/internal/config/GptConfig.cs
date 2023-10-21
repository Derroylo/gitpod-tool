namespace Gitpod.Tool.Helper.Internal.Config
{
    class GptConfig: AbstractConfig
    {
        public static bool AllowPreReleases
        {
            get {
                if (!IsConfigFileLoaded) {
                    ReadConfigFile();
                }

                return appConfig.Config.AllowPreReleases;
            }
        }
    }
}