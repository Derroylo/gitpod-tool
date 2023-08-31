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

        private String dockerComposeFile = null;

        public String DockerComposeFile { get { return this.dockerComposeFile; } set { this.dockerComposeFile = value; } }
    }
}