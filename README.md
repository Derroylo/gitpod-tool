# CLI Tool for Gitpod

## Purpose
This CLI Tool aims to make it easier to use Gitpod for web development. It is still in it´s very early stages of development but this are the planned or already implemented features:

- [x] Change and persist PHP Version on the fly
- [x] Change and persist PHP Settings on the fly
- [x] Start, Restart and Stop the Apache Webserver
- [x] Define which services should be started from docker-compose
- [x] Make the Tool extendable via shell scripts
- [x] Save the tool configurations in a yml file
- [x] Ability to update the tool via command
- [ ] Enable/Disable xdebug on the fly
- [ ] Change the nodejs version
- [ ] Import/Export the database or create/restore Snapshots
- [ ] Define additional folders that should be included for extending this tool via shell scripts
- [ ] Add the ability to save a file and/or folder as env variable
- [ ] Split php settings for cli and web
- [ ] Select php version via dropdown, similar to services
- [ ] Add php modules via command
- [ ] Implement a method to reuse the index for phpstorm within prebuilds (phpstorm can use a shared index)

## Documentation
The documentation can be found under [GPT Documentation](https://derroylo.github.io). If you want to try it, open [Shopware workspace sample](https://github.com/Derroylo/shopware-workspace-sample) in gitpod. In the terminal type `gpt -h` to get a list of the available commands.

## Installation

Add the microsoft repository as the official ubuntu package manager doesn´t contain the dotnet runtime in version 7
```
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt update
```

Install the dotnet runtime
```
sudo apt-get install -y dotnet-runtime-7.0
```

Download the latest release of this tool
```
curl -s https://api.github.com/repos/Derroylo/gitpod-tool/releases/latest | grep "browser_download_url.*zip" | cut -d : -f 2,3 | tr -d \" | wget -qi -
```

Unzip the downloaded tool, create a folder in your home folder and add an alias to your bashrc.
__If you are not using bashrc, you might need to change the following lines accordingly.__
```
mkdir /home/gitpod/.gpt
unzip gitpod-tool.zip -d /home/gitpod/.gpt/
rm gitpod-tool.zip
echo "alias gpt='dotnet $HOME/.gpt/gitpod-tool.dll'" > .bashrc.d/gitpod-tool
```

If everything worked, then you should be able to use the `gpt` command in the terminal.

## Development
This tool is written in C# and runs on dotnet v7.

[![Open in Gitpod](https://gitpod.io/button/open-in-gitpod.svg)](https://gitpod.io/#https://github.com/derroylo/gitpod-tool)

Within the terminal you can run the app with `dotnet run`. Normally dotnet would use the given flags, like `dotnet run -h`, and interpret them. If you want to pass one or more flags to the app, write `--` before them like in this example `dotnet run -- -h -f`. Now `-h` and `-f` would be handled by the app itself.

To compile the app execute the following command `dotnet build -r linux-x64 --configuration Release --no-restore --self-contained false /p:PublishSingleFile=true /p:PublishTrimmed=true`. You can now copy the created files from `bin/Release/net7.0/linux-x64/`.
There is also a `compile.sh` script in the root folder that compiles everything and puts the result into `gitpod-tool.zip` in the root folder of the project.

## Issues, Feature requests etc.
Create an issue if you encounter problems with this tool or have suggestions on what to add next.

You can also find me on the [Gitpod Discord Server](https://discord.com/invite/gitpod) 
