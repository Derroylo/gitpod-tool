using System;
using Spectre.Console;
using Spectre.Console.Cli;
using Gitpod.Tool.Commands.Php;
using Gitpod.Tool.Commands.Apache;

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

                config.AddBranch("apache", apache =>
                {
                    apache.AddCommand<ApacheStatusCommand>("status");
                    apache.AddCommand<ApacheStartCommand>("start");
                    apache.AddCommand<ApacheStopCommand>("stop");
                    apache.AddCommand<ApacheRestartCommand>("restart");
                });
            });

            app.Run(args);
        }
    }
}
