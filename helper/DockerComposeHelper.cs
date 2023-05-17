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
        public static Dictionary<string, Dictionary<string, string>> GetServices(string filename)
        {
            var services = new Dictionary<string, Dictionary<string, string>>();

            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                    .Build();
            dynamic dockerCompose = deserializer.Deserialize<dynamic>(File.ReadAllText("docker-compose.yml"));

            foreach (KeyValuePair<object, object> item in dockerCompose["services"]) {
                var serviceInfos = new Dictionary<string, string>();

                string alias = item.Key.ToString();               
                string serviceName = item.Key.ToString();

                if (((Dictionary<object, object>) dockerCompose["services"][alias]).ContainsKey("container_name")) {
                    alias = dockerCompose["services"][alias]["container_name"].ToString();
                }

                serviceInfos.Add("alias", alias);

                if (((Dictionary<object, object>) dockerCompose["services"][serviceName]).ContainsKey("labels")) {
                    if (dockerCompose["services"][serviceName]["labels"].ContainsKey("com.gitpod.gpt.category")) {
                        serviceInfos.Add("category", dockerCompose["services"][serviceName]["labels"]["com.gitpod.gpt.category"].ToString());
                    }
                }

                services.Add(item.Key.ToString(), serviceInfos);
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
