using System.Collections.Generic;

namespace Gitpod.Tool.Classes.Configuration
{
    class PhpConfiguration
    {
        private string version = "8.2";

        private Dictionary<string, string> config = new();

        private Dictionary<string, string> configWeb = new();

        private Dictionary<string, string> configCLI = new();

        private List<string> packages = new();

        public string Version { get { return version; } set { version = value; }}

        public Dictionary<string, string> Config { get { return config; } set { config = value; }}

        public Dictionary<string, string> ConfigWeb { get { return configWeb; } set { configWeb = value; }}

        public Dictionary<string, string> ConfigCLI { get { return configCLI; } set { configCLI = value; }}

        public List<string> Packages { get { return packages; } set { packages = value; }}
    }
}