using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Gitpod.Tool.Classes.Configuration;
using Spectre.Console;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Gitpod.Tool.Helper
{
    class GptConfigHelper
    {  
        public static Configuration Config {get; set;}

        public static bool ReadConfigFile(bool showError = false, bool showException = false)
        {
            // Init the config object in case something fails below so we don´t end in showing an exception (will be refactored in a later version)
            GptConfigHelper.Config = new Configuration();

            var workspacePath = Environment.GetEnvironmentVariable("GITPOD_REPO_ROOT");

            if (workspacePath == null || workspacePath == string.Empty) {
                workspacePath = Directory.GetCurrentDirectory();
            }

            var configFile = workspacePath + "/.gpt.yml";

            if (!File.Exists(configFile)) {
                return false;
            }

            try {
                var deserializer = new DeserializerBuilder()
                                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                .Build();

                GptConfigHelper.Config = deserializer.Deserialize<Configuration>(File.ReadAllText(configFile));

                // in case we have an empty file
                GptConfigHelper.Config ??= new Configuration();

                return true;
            } catch (Exception e) {
                if (showException) {
                    AnsiConsole.WriteException(e);
                }

                if (showError) {
                    AnsiConsole.MarkupLine("[red]Failed to parse the configuration file '.gpt.yml'. Make sure the syntax is correct.[/]");
                }

                return false;
            }
        }

        public static bool WriteConfigFile()
        {
            var workspacePath = Environment.GetEnvironmentVariable("GITPOD_REPO_ROOT");

            if (workspacePath == null || workspacePath == string.Empty) {
                workspacePath = Directory.GetCurrentDirectory();
            }

            var configFile = workspacePath + "/.gpt.yml";

            try {
                var serializer = new SerializerBuilder()
                    .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitEmptyCollections | DefaultValuesHandling.OmitNull)
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                var stringResult = serializer.Serialize(GptConfigHelper.Config);

                File.WriteAllText(configFile, stringResult);

                return true;
            } catch (Exception e) {
                AnsiConsole.WriteException(e);

                return false;
            }
        }
    }
}