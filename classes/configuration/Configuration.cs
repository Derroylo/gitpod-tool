namespace Gitpod.Tool.Classes.Configuration
{
    class Configuration
    {
        private ConfigConfiguration config = new();

        public ConfigConfiguration Config { get { return config; } set { config = value; } }

        private PhpConfiguration php = new();

        public PhpConfiguration Php { get { return php; } set { php = value; } }

        private NodeJsConfiguration nodejs = new();

        public NodeJsConfiguration Nodejs { get { return nodejs; } set { nodejs = value; } }

        private ServiceConfiguration services = new();

        public ServiceConfiguration Services { get { return services; } set { services = value; } }

        private ShellScriptsConfiguration shellScripts = new();

        public ShellScriptsConfiguration ShellScripts { get { return shellScripts; } set { shellScripts = value; } }

        private EnvConfiguration env = new();

        public EnvConfiguration Env { get { return env; } set { env = value; } }
    }
}