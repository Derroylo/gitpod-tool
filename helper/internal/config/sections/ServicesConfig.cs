using System.Collections.Generic;

namespace Gitpod.Tool.Helper.Internal.Config.Sections
{
    class ServicesConfig: ConfigHelper
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