using System;
using System.Collections.Generic;

namespace Gitpod.Tool.Classes
{
    class CustomBranch
    {
        private readonly string name = string.Empty;

        private readonly string description = string.Empty;

        private readonly List<CustomCommand> commands = new();

        public string Name
        {
            get { return name; }
        }

        public string Description
        {
            get { return description; }
        }

        public List<CustomCommand> Commands
        {
            get { return commands; }
        }

        public CustomBranch(string name, string description = null) {
            if (name == null || name == string.Empty || name.Length < 1) {
                throw new Exception("Missing name for custom branch");
            }

            this.name = name;

            if (description != null && description.Length > 0) {
                this.description = description;
            }
        }
    }
}