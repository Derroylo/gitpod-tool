using System;
using System.Collections.Generic;

namespace Gitpod.Tool.Classes.Configuration
{
    class ShellScriptsConfiguration
    {
        private List<string> additionalDirectories = new List<string>();

        public List<string> AdditionalDirectories { get { return this.additionalDirectories; } set { this.additionalDirectories = value; }}
    }
}