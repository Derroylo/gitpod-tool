using System.Collections.Generic;

namespace Gitpod.Tool.Helper.Internal.Config.Sections
{
    class PhpConfig: ConfigHelper
    {
        public static string PhpVersion
        {
            get {
                if (!IsConfigFileLoaded) {
                    ReadConfigFile();
                }

                return appConfig.Php.Version;
            }

            set {
                appConfig.Php.Version = value;
            }
        }

        public static Dictionary<string, string> Config
        {
            get {
                if (!IsConfigFileLoaded) {
                    ReadConfigFile();
                }

                return appConfig.Php.Config;
            }
        }

        public static Dictionary<string, string> ConfigWeb
        {
            get {
                if (!IsConfigFileLoaded) {
                    ReadConfigFile();
                }

                return appConfig.Php.ConfigWeb;
            }
        }

        public static Dictionary<string, string> ConfigCli
        {
            get {
                if (!IsConfigFileLoaded) {
                    ReadConfigFile();
                }

                return appConfig.Php.ConfigWeb;
            }
        }

        public static List<string> Packages
        {
            get {
                if (!IsConfigFileLoaded) {
                    ReadConfigFile();
                }

                return appConfig.Php.Packages;
            }
        }
    }
}