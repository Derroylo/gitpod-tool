using System;
using System.Collections.Generic;

namespace Gitpod.Tool.Classes
{
    class CustomCommand
    {
        private string command = String.Empty;

        private string file = String.Empty;

        private string description = String.Empty;

        private List<string> arguments = new List<string>();

        public string Command
        {
            get { return this.command; }
        }

        public string File
        {
            get { return this.file; }
        }

        public string Description
        {
            get { return this.description; }
        }

        public List<string> Arguments
        {
            get { return this.arguments; }
        }

        public CustomCommand(string command, string file, string description = null, List<string> arguments = null) {
            if (command == null || command == String.Empty || command.Length < 1) {
                throw new Exception("Missing command for custom command");
            }

            if (file == null || file == String.Empty || file.Length < 4) {
                throw new Exception("Missing file for custom command");
            }

            this.command = command;
            this.file = file;

            if (description != null && description.Length > 0) {
                this.description = description;
            }

            if (arguments != null && arguments.Count > 0) {
                this.arguments = arguments;
            }
        }
    }
}