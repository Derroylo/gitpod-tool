#!/usr/bin/env bash

# This file is the entrypoint for the tool and is required for the self-update

# Get the location of this script
SCRIPT=$(readlink -f "$0")
SCRIPTPATH=$(dirname "$SCRIPT")

# Directory where gpt is located
GPTDIR=$SCRIPTPATH

# After an update, gpt will be installed in /workspace/.gpt so the update will not be lost after a restart
if [[ $SCRIPTPATH == "/home/gitpod/.gpt" ]]  && [ -d "/workspace/.gpt" ] && [ -f "/workspace/.gpt/gitpod-tool.dll" ]; then
	GPTDIR="/workspace/.gpt"
fi

# run the application and pass all arguments to it
dotnet "$GPTDIR/gitpod-tool.dll" "$@"

# Check if the update folder exists
if [ -d "update" ]; then
    cd $GPTDIR

    # Move all files from the update folder to the current one and remove it afterwards
    mv update/* .

    # Remove the update folder
    rm -rf update

    # Set execution rights for the shell script
    chmod +x gpt.sh
fi

# Check if we want to start services
if [ -f "$GPTDIR/.services_start" ]; then
    activeServices=$(<"$GPTDIR/.services_start")

    rm "$GPTDIR/.services_start"

    docker-compose up $activeServices
fi

# Check if we want to stop services
if [ -f "$GPTDIR/.services_stop" ]; then
    rm "$GPTDIR/.services_stop"

    docker-compose stop
fi

# Check if we want to change the nodejs version
if [ -f "$GPTDIR/.nodejs" ]; then
    version=$(<"$GPTDIR/.nodejs")

    rm "$GPTDIR/.nodejs"

    . ~/.nvm/nvm.sh

    nvm use $version
    
    nvm alias default $version
fi