using System.Collections.Generic;

namespace Gitpod.Tool.Helper.Internal.Config.Sections
{
    class ServicesConfig: ConfigHelper
    {
        public static string DockerComposeFile
        {
            get {
                return appConfig.Services.File;
            }
        }

        public static List<string> ActiveServices
        {
            get {
                return appConfig.Services.Active;
            }

            set {
                ConfigUpdated = true;

                appConfig.Services.Active = value;
            }
        }
    }
}