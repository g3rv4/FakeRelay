#!/bin/ash

if [ "$1" = "web" ]; then
    web
else
    cli "$@"
fi
