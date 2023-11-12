using System;
using System.Collections.Generic;

namespace Gitpod.Tool.Classes
{
    class CustomCommand
    {
        private readonly string command = string.Empty;

        private readonly string file = string.Empty;

        private readonly string description = string.Empty;

        private readonly List<string> arguments = new();

        public string Command
        {
            get { return command; }
        }

        public string File
        {
            get { return file; }
        }

        public string Description
        {
            get { return description; }
        }

        public List<string> Arguments
        {
            get { return arguments; }
        }

        public CustomCommand(string command, string file, string description = null, List<string> arguments = null) {
            if (command == null || command == string.Empty || command.Length < 1) {
                throw new Exception("Missing command for custom command");
            }

            if (file == null || file == string.Empty || file.Length < 4) {
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