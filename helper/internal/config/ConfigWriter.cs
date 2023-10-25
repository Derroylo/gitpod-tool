using System.IO;
using Gitpod.Tool.Classes.Configuration;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Gitpod.Tool.Helper.Internal.Config
{
    class ConfigWriter
    {
        public static void WriteConfigFile(string configFile, Configuration configuration)
        {
            var serializer = new SerializerBuilder()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitEmptyCollections | DefaultValuesHandling.OmitNull)
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var stringResult = serializer.Serialize(configuration);

            // TODO: Try to keep the format of the original file like including comments etc.
            File.WriteAllText(configFile, stringResult);
        }
    }
}