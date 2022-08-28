# How to run
- An environmental variable `ALADAR_BOT` has to be set to the bot token string
  - Windows -- `$env:ALADAR_BOT="TOKEN_STRING"`
  - Linux -- `export ALADAR_BOT=TOKEN_STRING`
- Lavalink has to be running on `localhost`, check Lavalink setup below
- After compilation run the `ProjektDjAladar` or `ProjektDjAladar.exe` to start bot

## Lavalink setup
- Java 13 or greater required
- Run with `java -jar Lavalink.jar`

# Docker setup
- Alternatively the bot can run in a docker containers
  - Booth the bot and Lavalink have separate containers
- Make sure you have installed docker engine ([guide here](https://docs.docker.com/engine/install/)) and docker
compose ([guide here](https://docs.docker.com/compose/install/))
- To build and start the docker containers run: `$ ./build.sh`
- Create `.env` file following the template `.env.example` and set an environmental variable `ALADAR_BOT` to the bot token string 
