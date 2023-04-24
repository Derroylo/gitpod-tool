using System;
using System.Collections.Generic;

namespace Gitpod.Tool.Classes
{
    class ShellScriptSettings
    {
        private string branch = String.Empty;

        private string branchDescription = String.Empty;

        private string command = String.Empty;

        private string description = String.Empty;

        private List<string> arguments = new List<string>();

        public string Branch
        {
            get { return this.branch; }
        }

        public string BranchDescription
        {
            get { return this.branchDescription; }
        }

        public string Command
        {
            get { return this.command; }
        }

        public string Description
        {
            get { return this.description; }
        }

        public List<string> Arguments
        {
            get { return this.arguments; }
        }

        public ShellScriptSettings(string command, string description = null, string branch = null, string branchDescription = null, List<string> arguments = null) {
            if (command == null || command == String.Empty || command.Length < 1) {
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