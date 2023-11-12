using System;
using System.Collections.Generic;

namespace Gitpod.Tool.Classes
{
    class ShellScriptSettings
    {
        private readonly string branch = string.Empty;

        private readonly string branchDescription = string.Empty;

        private readonly string command = string.Empty;

        private readonly string description = string.Empty;

        private readonly List<string> arguments = new();

        public string Branch
        {
            get { return branch; }
        }

        public string BranchDescription
        {
            get { return branchDescription; }
        }

        public string Command
        {
            get { return command; }
        }

        public string Description
        {
            get { return description; }
        }

        public List<string> Arguments
        {
            get { return arguments; }
        }

        public ShellScriptSettings(string command, string description = null, string branch = null, string branchDescription = null, List<string> arguments = null) {
            if (command == null || command == string.Empty || command.Length < 1) {
                throw new Exception("Missing command");
            }

            this.command = command;

            if (description != null && description.Length > 0) {
                this.description = description;
            }

            if (branch != null && branch.Length > 0) {
                this.branch = branch;
            }

            if (branchDescription != null && branchDescription.Length > 0) {
                this.branchDescription = branchDescription;
            }

            if (arguments != null && arguments.Count > 0) {
                this.arguments = arguments;
            }
        }
    }
}