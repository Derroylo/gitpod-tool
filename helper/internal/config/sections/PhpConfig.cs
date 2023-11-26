using System.Collections.Generic;

namespace Gitpod.Tool.Helper.Internal.Config.Sections
{
    class PhpConfig: ConfigHelper
    {
        public static string PhpVersion
        {
            get {
                return appConfig.Php.Version;
            }

            set {
                if (appConfig.Php.Version != value) {
                    ConfigUpdated = true;
                }

                appConfig.Php.Version = value;
            }
        }

        public static Dictionary<string, string> Config
        {
            get {
                return appConfig.Php.Config;
            }
        }

        public static Dictionary<string, string> ConfigWeb
        {
            get {
                return appConfig.Php.ConfigWeb;
            }
        }

        public static Dictionary<string, string> ConfigCli
        {
            get {
                return appConfig.Php.ConfigCLI;
            }
        }

        public static List<string> Packages
        {
            get {
                return appConfig.Php.Packages;
            }
        }
    }
}