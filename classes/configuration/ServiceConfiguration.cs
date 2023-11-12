using System.Collections.Generic;

namespace Gitpod.Tool.Classes.Configuration
{
    class ServiceConfiguration
    {
        private List<string> active = new();

        public List<string> Active { get { return active; } set { active = value; }}
        
        private string file = "docker-compose.yml";

        public string File { get { return file; } set { file = value; } }
    }
}