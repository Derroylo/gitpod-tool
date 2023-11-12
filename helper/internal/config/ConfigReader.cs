using System.IO;
using Gitpod.Tool.Classes.Configuration;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Gitpod.Tool.Helper.Internal.Config
{
    class ConfigReader
    {
        public static Configuration ReadConfigFile(string configFile)
        {
            var deserializer = new DeserializerBuilder()
                            .WithNamingConvention(CamelCaseNamingConvention.Instance)
                            .Build();

            return deserializer.Deserialize<Configuration>(File.ReadAllText(configFile));
        }
    }
}