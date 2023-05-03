using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using Spectre.Console;
using YamlDotNet.Serialization.NamingConventions;

namespace Gitpod.Tool.Helper
{
    class DockerComposeHelper
    {
        public static Dictionary<string, string> GetServices(string filename)
        {
            Dictionary<string, string> services = new Dictionary<string, string>();

            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                    .Build();
            dynamic dockerCompose = deserializer.Deserialize<dynamic>(File.ReadAllText("docker-compose.yml"));

            AnsiConsole.WriteLine(dockerCompose["services"]["mysql"]["container_name"].ToString());
            foreach (KeyValuePair<object, object> item in dockerCompose["services"]) {
                string alias = item.Key.ToString();               
                
                if (((Dictionary<object, object>) dockerCompose["services"][alias]).ContainsKey("container_name")) {
                    alias = dockerCompose["services"][alias]["container_name"].ToString();
                }

                services.Add(item.Key.ToString(), alias);
            }

            return services;
        }

        public static bool IsServiceStarted(string name)
        {
            var result = ExecCommand.Exec("docker ps -q -f status=running -f name=^/" + name);

            if (result.Trim().Length == 0) {
                return false;
            }

            return true;
        }
    }
}
