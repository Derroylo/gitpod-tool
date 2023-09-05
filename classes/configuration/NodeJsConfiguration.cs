using System;
using System.Collections.Generic;

namespace Gitpod.Tool.Classes.Configuration
{
    class NodeJsConfiguration
    {
        private string version = String.Empty;

        public string Version { get { return this.version; } set { this.version = value; }}
    }
}