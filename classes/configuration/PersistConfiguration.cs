using System.Collections.Generic;

namespace Gitpod.Tool.Classes.Configuration
{
    class PersistConfiguration
    {
        private Dictionary<string, string> vars = new();

        public Dictionary<string, string> Vars { get { return vars; } set { vars = value; }}

        private Dictionary<string, Dictionary<string, string>> files = new();

        public Dictionary<string, Dictionary<string, string>> Files { get { return files; } set { files = value; }}

        private Dictionary<string, Dictionary<string, string>> folders = new();

        public Dictionary<string, Dictionary<string, string>> Folders { get { return folders; } set { folders = value; }}
    }
}