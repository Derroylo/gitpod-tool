#!/usr/bin/env bash

dotnet build -r linux-x64 --configuration Release --no-restore --self-contained false /p:PublishSingleFile=true /p:PublishTrimmed=true

cd ./bin/Release/net7.0/linux-x64/ && zip -r ../../../../gitpod-tool.zip ./* && cd -