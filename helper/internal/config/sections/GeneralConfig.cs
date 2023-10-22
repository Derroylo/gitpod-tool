namespace Gitpod.Tool.Helper.Internal.Config.Sections
{
    class GeneralConfig: ConfigHelper
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