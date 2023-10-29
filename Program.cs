using System;
using Spectre.Console;
using Spectre.Console.Cli;
using Gitpod.Tool.Commands.Php;
using Gitpod.Tool.Commands.Apache;
using Gitpod.Tool.Commands;
using Gitpod.Tool.Classes;
using System.Collections.Generic;
using System.Linq;
using Gitpod.Tool.Commands.Shell;
using Gitpod.Tool.Commands.Config;
using Gitpod.Tool.Commands.Services;
using Gitpod.Tool.Commands.ModeJS;
using Gitpod.Tool.Commands.Restore;
using Gitpod.Tool.Commands.NodeJS;
using Gitpod.Tool.Helper.Internal;
using Gitpod.Tool.Helper.Internal.Config;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Gitpod.Tool
{
    class Program
    {
        static void Main(string[] args)
        {
            var app     = new CommandApp();
            var version = UpdateHelper.CurrentVersion;

            // Output the program name, version and info if the config file could not be read
            OutputProgramHeader(version, args.Contains("--debug"));

            // Load additional commands that are defined within shell scripts
            var additionalCommands = new Dictionary<string, CustomBranch>();

            try {
                additionalCommands = CustomCommandsLoader.Load();
            } catch (Exception e) {
                AnsiConsole.MarkupLine("[red]Unable to load the custom commands[/] - [orange3]Append '--debug' to show more details[/]");

                // With he debug arg, output the exception and exit
                if (args.Contains("--debug")) {
                    AnsiConsole.WriteException(e);

                    return;
                }
            }

            // Always copy the file gpt.sh to /home/workspace/.gpt/ to prevent problems with older versions or while using PreReleases that add new functions to it
            try {
                var applicationDir = AppDomain.CurrentDomain.BaseDirectory;

                if (applicationDir == "/workspace/.gpt/" && File.Exists("/home/gitpod/.gpt/gpt.sh") && GetMD5HashForFile("/workspace/.gpt/gpt.sh") != GetMD5HashForFile("/home/gitpod/.gpt/gpt.sh")) {
                    File.Copy("/workspace/.gpt/gpt.sh", "/home/gitpod/.gpt/gpt.sh", true);

#pragma warning disable CA1416
                    File.SetUnixFileMode("/home/gitpod/.gpt/gpt.sh", UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute | UnixFileMode.GroupRead | UnixFileMode.OtherRead);
#pragma warning restore CA1416

                    AnsiConsole.MarkupLine("[orange3]gpt.sh has been updated to the latest version. If you have encountered an error, try running the command again.[/]");
                }
            } catch (Exception e) {
                AnsiConsole.MarkupLine("[red]Unable to copy the file /workspace/.gpt/gpt.sh to /home/gitpod/.gpt/gpt.sh[/] - [orange3]Append '--debug' to show more details[/]");

                // With he debug arg, output the exception and exit
                if (args.Contains("--debug")) {
                    AnsiConsole.WriteException(e);

                    return;
                }
            }

            app.Configure(config =>
            {
                config.SetApplicationName("gpt");
                config.SetApplicationVersion(version);
                
                // Add Branches and their commands
                config.AddBranch("apache", branch => AddApacheCommandBranch(branch, additionalCommands));
                config.AddCommand<AskCommand>("ask").WithDescription("Ask the gitpod ai");
                config.AddBranch("config", branch => AddConfigCommandBranch(branch, additionalCommands));
                config.AddBranch("mysql", branch => AddMysqlCommandBranch(branch, additionalCommands));
                config.AddBranch("nodejs", branch => AddNodeJsCommandBranch(branch, additionalCommands));
                config.AddBranch("php", branch => AddPhpCommandBranch(branch, additionalCommands));
                config.AddBranch("restore", branch => AddRestoreCommandBranch(branch, additionalCommands));
                config.AddBranch("services", branch => AddServicesCommandBranch(branch, additionalCommands));
                config.AddCommand<SelfUpdateCommand>("update").WithDescription("Update this tool to the latest version");

                List<string> reservedBranches = new() { "default", "config", "php", "nodejs", "apache", "mysql", "services", "restore" };

                // Add branches that haven´t been added yet via custom commands
                foreach (KeyValuePair<string, CustomBranch> entry in additionalCommands.Where(x => !reservedBranches.Contains(x.Key))) {
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
                //CustomBranch branch = null;
                if (additionalCommands.TryGetValue("default", out CustomBranch branch)) {
                    foreach (CustomCommand cmd in branch.Commands) {
                        config.AddCommand<ShellFileCommand>(cmd.Command)
                            .WithData(cmd)
                            .WithDescription(cmd.Description);
                    }
                }
            });

            app.Run(args);

            if (ConfigHelper.ConfigFileExists && ConfigHelper.IsConfigFileValid && ConfigHelper.ConfigUpdated) {
                try {
                    // Save config file
                    ConfigHelper.SaveConfigFile();
                } catch (Exception e) {
                    AnsiConsole.WriteLine("[red]Saving the config file failed[/] - [orange3]Append '--debug' to show more details[/]");

                    if (args.Contains("--debug")) {
                        AnsiConsole.WriteException(e);
                    }
                }
            }
        }

        private static string GetMD5HashForFile(string filename)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filename);

            return Encoding.Default.GetString(md5.ComputeHash(stream));
        }

        private static void OutputProgramHeader(string programVersion, bool showException = false)
        {
            AnsiConsole.Write(new FigletText("GPT"));
            AnsiConsole.Markup("[deepskyblue3]Gitpod Tool[/] - Version [green]" + programVersion + "[/]");

            // Try to load the config file
            ConfigHelper.ReadConfigFile();

            try {
                // Check for updates
                var latestVersion = UpdateHelper.GetLatestVersion().Result;
                var isUpdateAvailable = UpdateHelper.IsUpdateAvailable();

                if (isUpdateAvailable) {
                    AnsiConsole.MarkupLine(" - [orange3]Latest Version is " + latestVersion + ". Use 'gpt update' to update.[/]");
                } else {
                    AnsiConsole.MarkupLine("");
                }
            } catch (Exception e) {
                AnsiConsole.MarkupLine(" - [red]Check for update failed[/]");

                if (showException) {
                    AnsiConsole.WriteException(e);
                }
            }

            if (!ConfigHelper.ConfigFileExists) {
                AnsiConsole.MarkupLine("[orange3]No config file found - falling back to default settings[/]");
            } else if(!ConfigHelper.IsConfigFileValid) {
                AnsiConsole.MarkupLine("[red]Config file is invalid - falling back to default settings[/] - [orange3]Append '--debug' to show more details[/]");

                if (showException) {
                    try {
                        //ConfigHelper.ReadConfigFile(true);
                    } catch (Exception e) {
                        AnsiConsole.WriteException(e);
                    }
                }
            }
        }

        private static void AddConfigCommandBranch(IConfigurator<CommandSettings> branch, Dictionary<string, CustomBranch> additionalCommands)
        {
            branch.SetDescription("Creates or verify the configuration file");
                    
            branch.AddCommand<VerifyConfigCommand>("verify")
                .WithAlias("v")
                .WithDescription(@"Tries to read the config file and shows it`s content");                    

            if (additionalCommands.TryGetValue("config", out CustomBranch customBranch)) {
                foreach (CustomCommand cmd in customBranch.Commands) {
                    branch.AddCommand<ShellFileCommand>(cmd.Command)
                        .WithData(cmd)
                        .WithDescription(cmd.Description);
                }
            }
        }

        private static void AddPhpCommandBranch(IConfigurator<CommandSettings> branch, Dictionary<string, CustomBranch> additionalCommands)
        {
            branch.SetDescription("Different commands to change active php version, ini settings etc.");
                    
            branch.AddCommand<PhpVersionCommand>("version")
                .WithAlias("v")
                .WithDescription("Shows or sets the currently used PHP Version");
            branch.AddCommand<PhpIniCommand>("ini")
                .WithAlias("i")
                .WithDescription("Change the value of a PHP setting.");
            branch.AddCommand<PhpRestoreCommand>("restore")
                .WithAlias("r")
                .WithDescription("Restores a previously set PHP version and their settings");
            branch.AddCommand<PhpDebugCommand>("xdebug")
                .WithAlias("d")
                .WithDescription("Shows or sets the current xdebug mode");
            branch.AddCommand<PhpPackageCommand>("packages")
                .WithAlias("p")
                .WithDescription("Shows installed php packages or install new ones");

            if (additionalCommands.TryGetValue("php", out CustomBranch customBranch)) {
                foreach (CustomCommand cmd in customBranch.Commands) {
                    branch.AddCommand<ShellFileCommand>(cmd.Command)
                        .WithData(cmd)
                        .WithDescription(cmd.Description);
                }
            }
        }

        private static void AddNodeJsCommandBranch(IConfigurator<CommandSettings> branch, Dictionary<string, CustomBranch> additionalCommands)
        {
            branch.SetDescription("Different commands to change active nodejs version, etc.");
                    
            branch.AddCommand<NodeJSVersionCommand>("version")
                .WithAlias("v")
                .WithDescription("Shows or sets the currently used NodeJS Version");
            branch.AddCommand<NodeJSRestoreCommand>("restore")
                .WithAlias("r")
                .WithDescription("Restores a previously set NodeJS version");

            if (additionalCommands.TryGetValue("nodejs", out CustomBranch customBranch)) {
                foreach (CustomCommand cmd in customBranch.Commands) {
                    branch.AddCommand<ShellFileCommand>(cmd.Command)
                        .WithData(cmd)
                        .WithDescription(cmd.Description);
                }
            }
        }

        private static void AddApacheCommandBranch(IConfigurator<CommandSettings> branch, Dictionary<string, CustomBranch> additionalCommands)
        {
            branch.SetDescription("Some simple commands to start/stop/restart the apache webserver");

            branch.AddCommand<ApacheStatusCommand>("status")
                .WithDescription("Shows the current status of apache");
            branch.AddCommand<ApacheStartCommand>("start")
                .WithDescription("Starts apache");
            branch.AddCommand<ApacheStopCommand>("stop")
                .WithDescription("Stops apache");
            branch.AddCommand<ApacheRestartCommand>("restart")
                .WithDescription("Restarts apache");

            if (additionalCommands.TryGetValue("apache", out CustomBranch customBranch)) {
                foreach (CustomCommand cmd in customBranch.Commands) {
                    branch.AddCommand<ShellFileCommand>(cmd.Command)
                        .WithData(cmd)
                        .WithDescription(cmd.Description);
                }
            }
        }

        private static void AddMysqlCommandBranch(IConfigurator<CommandSettings> branch, Dictionary<string, CustomBranch> additionalCommands)
        {
            branch.SetDescription("Import or Export Databases or create snapshots [red]Not implemented yet[/]");

            branch.AddCommand<NotYetImplementedCommand>("export")
                .WithDescription("Exports the content of the database to a file [red]Not implemented yet[/]");
            branch.AddCommand<NotYetImplementedCommand>("import")
                .WithDescription("Imports database content from a file [red]Not implemented yet[/]");
            branch.AddCommand<NotYetImplementedCommand>("snapshot")
                .WithDescription("Create/Restore a snapshot of the database. Useful to make a backup before you test something and want to restore the old state fast if anything goes wrong [red]Not implemented yet[/]");

            if (additionalCommands.TryGetValue("mysql", out CustomBranch customBranch)) {
                foreach (CustomCommand cmd in customBranch.Commands) {
                    branch.AddCommand<ShellFileCommand>(cmd.Command)
                        .WithData(cmd)
                        .WithDescription(cmd.Description);
                }
            }
        }

        private static void AddServicesCommandBranch(IConfigurator<CommandSettings> branch, Dictionary<string, CustomBranch> additionalCommands)
        {
            branch.SetDescription("List, status of services and define which should be started");

            branch.AddCommand<ListServicesCommand>("list")
                .WithDescription("List available the services");
            branch.AddCommand<StartServicesCommand>("start")
                .WithDescription("Start the services that are marked as active");
            branch.AddCommand<StartServicesCommand>("stop")
                .WithDescription("Stops running services");
            branch.AddCommand<SelectServicesCommand>("select")
                .WithDescription("Select which services should be active");

            if (additionalCommands.TryGetValue("services", out CustomBranch customBranch)) {
                foreach (CustomCommand cmd in customBranch.Commands) {
                    branch.AddCommand<ShellFileCommand>(cmd.Command)
                        .WithData(cmd)
                        .WithDescription(cmd.Description);
                }
            }
        }

        private static void AddRestoreCommandBranch(IConfigurator<CommandSettings> branch, Dictionary<string, CustomBranch> additionalCommands)
        {
            branch.SetDescription("Restore settings separate for nodejs or php, or for all at once ");
                    
            branch.AddCommand<RestoreAllCommand>("all")
                .WithAlias("a")
                .WithDescription("Restore all settings");
            branch.AddCommand<RestorePhpCommand>("php")
                .WithAlias("p")
                .WithDescription("Restore settings for php");
            branch.AddCommand<RestoreNodeJsCommand>("nodejs")
                .WithAlias("n")
                .WithDescription("Restore settings for NodeJS");

            if (additionalCommands.TryGetValue("restore", out CustomBranch customBranch)) {
                foreach (CustomCommand cmd in customBranch.Commands) {
                    branch.AddCommand<ShellFileCommand>(cmd.Command)
                        .WithData(cmd)
                        .WithDescription(cmd.Description);
                }
            }
        }
    }
}
