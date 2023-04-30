using System;
using System.Collections.Generic;

namespace Gitpod.Tool.Classes.Configuration
{
    class Configuration
    {
        private PhpConfiguration php = new PhpConfiguration();

        public PhpConfiguration Php { get { return php; } set { php = value; } }
    }
}