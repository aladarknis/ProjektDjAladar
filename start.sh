#!/bin/sh

echo "Starting Lavalink ..."
# Move to directory so that lavalink can read the config
cd Lavalink; nohup java -jar Lavalink.jar &

# Wait for Lavalink to start
sleep 10

echo "Starting discord bot ..."
cd ../src/bin/Release/net5.0/linux-x64/publish; ./ProjektDjAladar
