using System.Collections.Generic;

namespace Gitpod.Tool.Helper.Internal.Config
{
    class ServicesConfig: AbstractConfig
    {
        public static string DockerComposeFile
        {
            get {
                if (!IsConfigFileLoaded) {
                    ReadConfigFile();
                }

                return appConfig.Services.File;
            }
        }

        public static List<string> ActiveServices
        {
            get {
                if (!IsConfigFileLoaded) {
                    ReadConfigFile();
                }

                return appConfig.Services.Active;
            }

            set {
                appConfig.Services.Active = value;
            }
        }
    }
}