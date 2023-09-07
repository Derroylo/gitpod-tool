using System;
using System.Collections.Generic;

namespace Gitpod.Tool.Classes.Configuration
{
    class PhpConfiguration
    {
        private string version = String.Empty;

        private Dictionary<string, string> config = new Dictionary<string, string>();

        private Dictionary<string, string> configWeb = new Dictionary<string, string>();

        private Dictionary<string, string> configCLI = new Dictionary<string, string>();

        private List<string> packages = new List<string>();

        public string Version { get { return this.version; } set { this.version = value; }}

        public Dictionary<string, string> Config { get { return this.config; } set { this.config = value; }}

        public Dictionary<string, string> ConfigWeb { get { return this.configWeb; } set { this.configWeb = value; }}

        public Dictionary<string, string> ConfigCLI { get { return this.configCLI; } set { this.configCLI = value; }}

        public List<string> Packages { get { return this.packages; } set { this.packages = value; }}
    }
}