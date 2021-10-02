#!/usr/bin/env bash

function cleanup() {
    unset accountEndpoint
    unset accountKey
    exit
}

trap cleanup SIGINT
pushd src/LinkAsp
export accountEndpoint=$(cat .secrets/accountEndpoint.secret)
export accountKey=$(cat .secrets/accountKey.secret)
popd
docker-compose up
