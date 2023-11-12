using System.Collections.Generic;

namespace Gitpod.Tool.Classes.Configuration
{
    class ShellScriptsConfiguration
    {
        private List<string> additionalDirectories = new();

        public List<string> AdditionalDirectories { get { return additionalDirectories; } set { additionalDirectories = value; }}
    }
}