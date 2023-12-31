using System.IO;
using Gitpod.Tool.Helper;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Gitpod.Tool.Commands.Sync
{
    class SyncWorkspaceCommand : Command<SyncWorkspaceCommand.Settings>
    {
        public class Settings : CommandSettings
        {
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            AnsiConsole.MarkupLine("Sync a folder from this machine to another workspace.");
            AnsiConsole.MarkupLine("");
           
            if (!ExecCommand.Exec("which rclone").Contains("rclone")) {
                if (!AnsiConsole.Confirm("rclone needs to be installed for this function. Do you want to install it now?")) {
                    return 1;
                }

                ExecCommand.Exec("sudo -v ; curl https://rclone.org/install.sh | sudo bash");

                if (!ExecCommand.Exec("which rclone").Contains("rclone")) {
                    AnsiConsole.MarkupLine("[red]Installation of rclone failed. You need to install it manually[/]");

                    return 1;
                }
            }

            if (!ExecCommand.Exec("which inotifywait").Contains("inotifywait")) {
                if (!AnsiConsole.Confirm("inotifywait needs to be installed for this function. Do you want to install it now?")) {
                    return 1;
                }

                ExecCommand.Exec("sudo -v ; curl https://rclone.org/install.sh | sudo bash");

                if (!ExecCommand.Exec("which inotifywait").Contains("inotifywait")) {
                    AnsiConsole.MarkupLine("[red]Installation of inotifywait failed. You need to install it manually[/]");

                    return 1;
                }
            }

            bool inputInvalid = false;

            var workspaceHost = string.Empty;
            var workspaceUser = string.Empty;
            var workspaceAccessToken = string.Empty;
            var localFolder = string.Empty;
            var remoteFolder = string.Empty;

            do {
                AnsiConsole.WriteLine("Enter the url of the workspace you want to connect to.");
                AnsiConsole.MarkupLine("[orange3]Info: The format should look like this: https://WORKSPACE-ID.ws-REGION.gitpod.io/[/]");
                
                workspaceHost = AnsiConsole.Ask<string>("Host:");

                if (workspaceHost == string.Empty) {
                    AnsiConsole.MarkupLine("[red]Enter a valid gitpod workspace url.[/]");
                    inputInvalid = true;
                } else {
                    inputInvalid = false;
                }
            } while (inputInvalid);

            do {
                AnsiConsole.WriteLine("Enter the name of the user.");
                AnsiConsole.MarkupLine("[orange3]Info: This is the name of the workspace[/]");
                
                workspaceUser = AnsiConsole.Ask<string>("User:");

                if (workspaceUser == string.Empty) {
                    AnsiConsole.MarkupLine("[red]Enter a valid user.[/]");
                    inputInvalid = true;
                } else {
                    inputInvalid = false;
                }
            } while (inputInvalid);

            do {
                AnsiConsole.WriteLine("Enter the access token.");
                
                workspaceAccessToken = AnsiConsole.Ask<string>("Access Token:");

                if (workspaceAccessToken == string.Empty) {
                    AnsiConsole.MarkupLine("[red]Enter a valid access token.[/]");
                    inputInvalid = true;
                } else {
                    inputInvalid = false;
                }
            } while (inputInvalid);

            do {
                AnsiConsole.WriteLine("Enter the local folder");
                AnsiConsole.MarkupLine("[orange3]Info: This is the folder you want to sync over to the remove machine.[/]");

                localFolder = AnsiConsole.Ask<string>("Local folder:");

                if (localFolder == string.Empty) {
                    AnsiConsole.MarkupLine("[red]Enter a valid local folder.[/]");
                    inputInvalid = true;
                } else {
                    inputInvalid = false;
                }
            } while (inputInvalid);

            do {
                AnsiConsole.WriteLine("Enter the remote folder");
                AnsiConsole.MarkupLine("[orange3]Info: The folder on the remove machine to which you want to sync the files to.[/]");

                remoteFolder = AnsiConsole.Ask<string>("Remote folder:");

                if (remoteFolder == string.Empty) {
                    AnsiConsole.MarkupLine("[red]Enter a valid remote folder.[/]");
                    inputInvalid = true;
                } else {
                    inputInvalid = false;
                }
            } while (inputInvalid);
            
            var accessTokenObscured = ExecCommand.Exec("echo \"" + workspaceAccessToken + "\" | rclone obscure -");

            var command = "rclone sync";

            // Set local folder
            command += " " + localFolder;

            // Set remote folder
            command += " :sftp:" + remoteFolder;

            // Set Host
            command += " --sftp-host " + workspaceHost;

            // Set User
            command += " --sftp-user " + workspaceUser;

            // Set the password
            command += " --sftp-pass " + accessTokenObscured;

            // Set shell type
            command += " --sftp-shell-type unix";

            // Set hash commands
            command += " --sftp-md5sum-command md5sum --sftp-sha1sum-command sha1sum";

            File.WriteAllText(".sync", command);

            return 0;
        }
    }
}