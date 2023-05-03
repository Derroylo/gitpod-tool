using System;
using System.Collections.Generic;

namespace Gitpod.Tool.Classes.Configuration
{
    class ServiceConfiguration
    {
        private List<string> active = new List<string>();

        public List<string> Active { get { return this.active; } set { this.active = value; }}
    }
}