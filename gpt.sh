#!/usr/bin/env bash

# This file is the entrypoint for the tool and is required for the self-update

# Get the location of this script
SCRIPT=$(readlink -f "$0")
SCRIPTPATH=$(dirname "$SCRIPT")

# run the application and pass all arguments to it
dotnet "$SCRIPTPATH/gitpod-tool.dll" "$@"

# Check if the update folder exists
if [ -d "update" ]; then
    cd $SCRIPTPATH

    # Move all files from the update folder to the current one and remove it afterwards
    mv update/* .

    # Remove the update folder
    rm -rf update

    # Set execution rights for the shell script
    chmod +x gpt.sh
fi

# Check if we want to start services
if [ -f "$SCRIPTPATH/.services_start" ]; then
    activeServices=$(<"$SCRIPTPATH/.services_start")

    rm "$SCRIPTPATH/.services_start"

    docker-compose up $activeServices
fi

# Check if we want to stop services
if [ -f "$SCRIPTPATH/.services_stop" ]; then
    rm "$SCRIPTPATH/.services_stop"

    docker-compose stop
fi