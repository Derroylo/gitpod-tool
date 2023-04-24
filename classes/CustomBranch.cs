using System;
using System.Collections.Generic;

namespace Gitpod.Tool.Classes
{
    class CustomBranch
    {
        private string name = String.Empty;

        private string description = String.Empty;

        private List<CustomCommand> commands = new List<CustomCommand>();

        public string Name
        {
            get { return this.name; }
        }

        public string Description
        {
            get { return this.description; }
        }

        public List<CustomCommand> Commands
        {
            get { return this.commands; }
        }

        public CustomBranch(string name, string description = null) {
            if (name == null || name == String.Empty || name.Length < 1) {
                throw new Exception("Missing name for custom branch");
            }

            this.name = name;

            if (description != null && description.Length > 0) {
                this.description = description;
            }
        }
    }
}