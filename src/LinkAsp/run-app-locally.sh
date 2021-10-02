#!/usr/bin/env bash

function cleanup() {
    unset accountEndpoint
    unset accountKey
    exit
}

trap cleanup SIGINT
export accountEndpoint=$(cat .secrets/accountEndpoint.secret)
export accountKey=$(cat .secrets/accountKey.secret)

dotnet run
