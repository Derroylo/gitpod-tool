#!/usr/bin/env bash

# This file is the entrypoint for the tool and is required for the self-update

# run the application and pass all arguments to it
dotnet gitpod-tool.dll "$@"

# Check if the update folder exists
if [ -d "update" ]; then
    # Move all files from the update folder to the current one and remove it afterwards
    mv update/* .

    rm -rf update
fi