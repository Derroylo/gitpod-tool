using System;
using System.Collections.Generic;

namespace Gitpod.Tool.Classes.Configuration
{
    class Configuration
    {
        private PhpConfiguration php = new PhpConfiguration();

        public PhpConfiguration Php { get { return php; } set { php = value; } }

        private ServiceConfiguration services = new ServiceConfiguration();

        public ServiceConfiguration Services { get { return services; } set { services = value; } }

        private ShellScriptsConfiguration shellScripts = new ShellScriptsConfiguration();

        public ShellScriptsConfiguration ShellScripts { get { return shellScripts; } set { shellScripts = value; } }
    }
}