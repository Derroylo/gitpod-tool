using System;
using Spectre.Console;
using Spectre.Console.Cli;
using Gitpod.Tool.Commands.Php;
using Gitpod.Tool.Commands.Apache;
using Gitpod.Tool.Commands.Mysql;
using Gitpod.Tool.Commands;
using Gitpod.Tool.Classes;
using System.Collections.Generic;
using Gitpod.Tool.Helper;
using System.Linq;
using Gitpod.Tool.Commands.Shell;

namespace Gitpod.Tool
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new CommandApp();

            var addidionalCommands = CustomCommandsLoader.Load();

            app.Configure(config =>
            {
                config.SetApplicationName("gpt");
                config.SetApplicationVersion("0.1.0");

                // First add all commands without branches
                if (addidionalCommands.ContainsKey("default")) {
                    foreach (CustomCommand cmd in addidionalCommands["default"].Commands) {
                        config.AddCommand<ShellFileCommand>(cmd.Command)
                            .WithData(cmd)
                            .WithDescription(cmd.Description);
                    }
                }
                
                config.AddBranch("php", php =>
                {
                    php.SetDescription("Different commands to change active php version, ini settings etc.");
                    
                    php.AddCommand<PhpVersionCommand>("version")
                        .WithAlias("v")
                        .WithDescription("Shows or sets the currently used PHP Version");
                    php.AddCommand<PhpIniCommand>("ini")
                        .WithAlias("i")
                        .WithDescription("Different functions for PHP ini files like updating or changing values");
                    php.AddCommand<PhpRestoreCommand>("restore")
                        .WithAlias("r")
                        .WithDescription("Restores a previously set PHP version and their ini files");
                    php.AddCommand<NotYetImplementedCommand>("debug")
                        .WithAlias("d")
                        .WithDescription("Enables/Disables xdebug [red]Not implemented yet[/]");

                    if (addidionalCommands.ContainsKey("php")) {
                        foreach (CustomCommand cmd in addidionalCommands["php"].Commands) {
                            php.AddCommand<ShellFileCommand>(cmd.Command)
                                .WithData(cmd)
                                .WithDescription(cmd.Description);
                        }
                    }
                });

                config.AddBranch("nodejs", nodejs =>
                {
                    nodejs.SetDescription("Different commands to change active nodejs version, etc. [red]Not implemented yet[/]");
                    
                    nodejs.AddCommand<NotYetImplementedCommand>("version")
                        .WithAlias("v")
                        .WithDescription("Shows or sets the currently used NodeJS Version [red]Not implemented yet[/]");
                    nodejs.AddCommand<PhpRestoreCommand>("restore")
                        .WithAlias("r")
                        .WithDescription("Restores a previously set NodeJS version [red]Not implemented yet[/]");

                    if (addidionalCommands.ContainsKey("nodejs")) {
                        foreach (CustomCommand cmd in addidionalCommands["nodejs"].Commands) {
                            nodejs.AddCommand<ShellFileCommand>(cmd.Command)
                                .WithData(cmd)
                                .WithDescription(cmd.Description);
                        }
                    }
                });

                config.AddBranch("apache", apache =>
                {
                    apache.SetDescription("Some simple commands to start/stop/restart the apache webserver");

                    apache.AddCommand<ApacheStatusCommand>("status")
                        .WithDescription("Shows the current status of apache");
                    apache.AddCommand<ApacheStartCommand>("start")
                        .WithDescription("Starts apache");
                    apache.AddCommand<ApacheStopCommand>("stop")
                        .WithDescription("Stops apache");
                    apache.AddCommand<ApacheRestartCommand>("restart")
                        .WithDescription("Restarts apache");

                    if (addidionalCommands.ContainsKey("apache")) {
                        foreach (CustomCommand cmd in addidionalCommands["apache"].Commands) {
                            apache.AddCommand<ShellFileCommand>(cmd.Command)
                                .WithData(cmd)
                                .WithDescription(cmd.Description);
                        }
                    }
                });

                config.AddBranch("mysql", mysql =>
                {
                    mysql.SetDescription("Import or Export Databases or create snapshots [red]Not implemented yet[/]");

                    mysql.AddCommand<NotYetImplementedCommand>("export")
                        .WithDescription("Exports the content of the database to a file [red]Not implemented yet[/]");
                    mysql.AddCommand<NotYetImplementedCommand>("import")
                        .WithDescription("Imports database content from a file [red]Not implemented yet[/]");
                    mysql.AddCommand<NotYetImplementedCommand>("snapshot")
                        .WithDescription("Create/Restore a snapshot of the database. Useful to make a backup before you test something and want to restore the old state fast if anything goes wrong [red]Not implemented yet[/]");

                    if (addidionalCommands.ContainsKey("mysql")) {
                        foreach (CustomCommand cmd in addidionalCommands["mysql"].Commands) {
                            mysql.AddCommand<ShellFileCommand>(cmd.Command)
                                .WithData(cmd)
                                .WithDescription(cmd.Description);
                        }
                    }
                });

                config.AddBranch("services", services =>
                {
                    services.SetDescription("Define which services should be started [red]Not implemented yet[/]");

                    services.AddCommand<NotYetImplementedCommand>("start")
                        .WithDescription("Start the services that are marked as active [red]Not implemented yet[/]");
                    services.AddCommand<NotYetImplementedCommand>("select")
                        .WithDescription("Select which services should be active [red]Not implemented yet[/]");

                    if (addidionalCommands.ContainsKey("services")) {
                        foreach (CustomCommand cmd in addidionalCommands["services"].Commands) {
                            services.AddCommand<ShellFileCommand>(cmd.Command)
                                .WithData(cmd)
                                .WithDescription(cmd.Description);
                        }
                    }
                });

                var reservedBranches = new List<String>() { "default", "php", "nodejs", "apache", "mysql", "services" };

                // Add branches that haven´t been added yet via custom commands
                foreach (KeyValuePair<string, CustomBranch> entry in addidionalCommands.Where(x => !reservedBranches.Contains(x.Key))) {
                    config.AddBranch(entry.Value.Name, branch => 
                    {
                        branch.SetDescription(entry.Value.Description);

                        foreach (CustomCommand cmd in entry.Value.Commands) {
                            branch.AddCommand<ShellFileCommand>(cmd.Command)
                                .WithData(cmd)
                                .WithDescription(cmd.Description);
                        }
                    });
                }
            });

            app.Run(args);
        }
    }
}
