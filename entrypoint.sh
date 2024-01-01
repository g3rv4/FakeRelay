#!/bin/ash

if [ "$1" = "web" ]; then
    dotnet FakeRelay.Web.dll
else
    dotnet FakeRelay.Cli.dll "$@"
fi
