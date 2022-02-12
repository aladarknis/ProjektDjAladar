#!/bin/bash

echo "Fixing config files ..."
sed -i 's/address: .*/address: lavalink/g' Lavalink/application.yml
sed -i 's/"lavalink_addr": ".*",/"lavalink_addr": "lavalink",/g' resources/config.json

echo "Running docker compose ..."
docker-compose up -d --build

echo "Reverting config files ..."
sed -i 's/address: lavalink/address: 127.0.0.1/g' Lavalink/application.yml
sed -i 's/"lavalink_addr": "lavalink",/"lavalink_addr": "127.0.0.1",/g' resources/config.json
