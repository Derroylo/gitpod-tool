using System;
using Spectre.Console;
using Spectre.Console.Cli;
using Gitpod.Tool.Commands.Php;

namespace Gitpod.Tool
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new CommandApp();

            app.Configure(config =>
            {
                config.AddBranch("php", php =>
                {
                    php.AddCommand<PhpVersionCommand>("version");
                    php.AddCommand<PhpIniCommand>("ini");
                    php.AddCommand<PhpRestoreCommand>("restore");
                });
            });

            app.Run(args);
        }
    }
}
