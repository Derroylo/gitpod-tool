using System;
using System.Collections.Generic;

namespace Gitpod.Tool.Classes.Configuration
{
    class ConfigConfiguration
    {
        private bool allowPreReleases = false;

        public bool AllowPreReleases { get { return this.allowPreReleases; } set { this.allowPreReleases = value; }}
    }
}