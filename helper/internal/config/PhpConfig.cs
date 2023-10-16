using System;
using System.IO;
using Gitpod.Tool.Classes.Configuration;

namespace Gitpod.Tool.Helper.Internal.Config
{
    class PhpConfig: AbstractConfig
    {
        public static string GetSelectedPhpVersion()
        {
            if (!IsConfigFileLoaded) {
                ReadConfigFile();
            }

            return appConfig.Php.Version;
        }
    }
}