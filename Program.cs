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
using System.Reflection;
using YamlDotNet.Serialization.NamingConventions;
using Gitpod.Tool.Classes.Configuration;
using System.IO;
using YamlDotNet.Serialization;
using Gitpod.Tool.Commands.Config;
using Gitpod.Tool.Commands.Services;
using Gitpod.Tool.Commands.ModeJS;
using Gitpod.Tool.Commands.Restore;
using Gitpod.Tool.Commands.NodeJS;

namespace Gitpod.Tool
{
    class Program
    {
        static void Main(string[] args)
        {
            var app     = new CommandApp();
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            // Check for updates
            var latestVersion = GptUpdateHelper.GetLatestVersion().Result;
            var isUpdateAvailable = GptUpdateHelper.IsUpdateAvailable();

            AnsiConsole.Write(new FigletText("GPT"));
            AnsiConsole.Markup("[deepskyblue3]Gitpod Tool[/] - Version [green]" + version + "[/]");

            if (isUpdateAvailable) {
                AnsiConsole.MarkupLine(" - [orange3]Latest Version is " + latestVersion + ". Use 'gpt update' to update.[/]");
            } else {
                AnsiConsole.MarkupLine("");
            }

            // Load the configuration file if it exits
            GptConfigHelper.ReadConfigFile();

            // Load additional commands that are defined within shell scripts
            var addidionalCommands = CustomCommandsLoader.Load();

            app.Configure(config =>
            {
                config.SetApplicationName("gpt");
                config.SetApplicationVersion(version);
               
                config.AddCommand<SelfUpdateCommand>("update")
                    .WithDescription("Update this tool to the latest version");

                config.AddCommand<AskCommand>("ask")
                    .WithDescription("Ask the gitpod ai");

                config.AddBranch("config", config =>
                {
                    config.SetDescription("Creates or verify the configuration file");
                    
                    config.AddCommand<VerifyConfigCommand>("verify")
                        .WithAlias("v")
                        .WithDescription(@"Tries to read the config file and shows it`s content");                    

                    if (addidionalCommands.ContainsKey("config")) {
                        foreach (CustomCommand cmd in addidionalCommands["config"].Commands) {
                            config.AddCommand<ShellFileCommand>(cmd.Command)
                                .WithData(cmd)
                                .WithDescription(cmd.Description);
                        }
                    }
                });

                config.AddBranch("php", php =>
                {
                    php.SetDescription("Different commands to change active php version, ini settings etc.");
                    
                    php.AddCommand<PhpVersionCommand>("version")
                        .WithAlias("v")
                        .WithDescription("Shows or sets the currently used PHP Version");
                    php.AddCommand<PhpIniCommand>("ini")
                        .WithAlias("i")
                        .WithDescription("Change the value of a PHP setting.");
                    php.AddCommand<PhpRestoreCommand>("restore")
                        .WithAlias("r")
                        .WithDescription("Restores a previously set PHP version and their settings");
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
                    nodejs.SetDescription("Different commands to change active nodejs version, etc.");
                    
                    nodejs.AddCommand<NodeJSVersionCommand>("version")
                        .WithAlias("v")
                        .WithDescription("Shows or sets the currently used NodeJS Version");
                    nodejs.AddCommand<NodeJSRestoreCommand>("restore")
                        .WithAlias("r")
                        .WithDescription("Restores a previously set NodeJS version");

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
                    services.SetDescription("List, status of services and define which should be started");

                    services.AddCommand<ListServicesCommand>("list")
                        .WithDescription("List available the services");
                    services.AddCommand<StartServicesCommand>("start")
                        .WithDescription("Start the services that are marked as active");
                    services.AddCommand<StartServicesCommand>("stop")
                        .WithDescription("Stops running services");
                    services.AddCommand<SelectServicesCommand>("select")
                        .WithDescription("Select which services should be active");

                    if (addidionalCommands.ContainsKey("services")) {
                        foreach (CustomCommand cmd in addidionalCommands["services"].Commands) {
                            services.AddCommand<ShellFileCommand>(cmd.Command)
                                .WithData(cmd)
                                .WithDescription(cmd.Description);
                        }
                    }
                });

                config.AddBranch("restore", restore =>
                {
                    restore.SetDescription("Restore settings separate for nodejs or php, or for all at once ");
                    
                    restore.AddCommand<RestoreAllCommand>("all")
                        .WithAlias("a")
                        .WithDescription("Restore all settings");
                    restore.AddCommand<RestorePhpCommand>("php")
                        .WithAlias("p")
                        .WithDescription("Restore settings for php");
                    restore.AddCommand<RestoreNodeJsCommand>("nodejs")
                        .WithAlias("n")
                        .WithDescription("Restore settings for NodeJS");

                    if (addidionalCommands.ContainsKey("restore")) {
                        foreach (CustomCommand cmd in addidionalCommands["restore"].Commands) {
                            restore.AddCommand<ShellFileCommand>(cmd.Command)
                                .WithData(cmd)
                                .WithDescription(cmd.Description);
                        }
                    }
                });

                var reservedBranches = new List<String>() { "default", "config", "php", "nodejs", "apache", "mysql", "services", "restore" };

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

                // Add all commands without branches
                if (addidionalCommands.ContainsKey("default")) {
                    foreach (CustomCommand cmd in addidionalCommands["default"].Commands) {
                        config.AddCommand<ShellFileCommand>(cmd.Command)
                            .WithData(cmd)
                            .WithDescription(cmd.Description);
                    }
                }
            });

            app.Run(args);
        }
    }
}
