# CLI Tool for Gitpod

## Purpose
This CLI Tool aims to make it easier to use Gitpod for web development. It is still in it´s very early stages of development but this are the planned or already implemented features:

- [x] Change and persist PHP Version on the fly
- [x] Change and persist PHP Settings on the fly
- [ ] Enable/Disable xdebug on the fly
- [x] Start, Restart and Stop the Apache Webserver
- [ ] Change the nodejs version
- [ ] Import/Export the database or create/restore Snapshots
- [ ] Define which services should be started from docker-compose
- [x] Make the Tool extendable via shell scripts
- [ ] Save the tool configurations in a yml file

## Documentation
There is currently no documentation available but should be available in the coming weeks. If you want to try it, open [Shopware workspace sample](https://github.com/Derroylo/shopware-workspace-sample) in gitpod. In the terminal type `gpt -h` to get a list of the available commands.

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

## Issues, Feature requests etc.
Create an issue if you encounter problems with this tool or have suggestions on what to add next.

You can also find me on the [Gitpod Discord Server](https://discord.com/invite/gitpod) 