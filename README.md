# How to run
- An environmental variable `ALADAR_BOT` has to be set to the bot token string
- Lavalink has to be running on `localhost`, check Lavalink setup below

# Lavalink setup
In order to run Lavalink, you must have Java 13 or greater installed.
Certain Java versions may not be functional with Lavalink, so it is best to check the requirements before downloading.

Make sure the location of the newest JRE's bin folder is added to your system variable's path.
You can verify that you have the right version by entering java -version in your command prompt or terminal.

Open your command prompt or terminal and navigate to the directory containing Lavalink.
Once there, type `java -jar Lavalink.jar`. You should start seeing log output from Lavalink.

# Docker setup
- Make sure you have installed docker engine ([guide here](https://docs.docker.com/engine/install/)) and docker
compose ([guide here](https://docs.docker.com/compose/install/))
- To build and start the docker run:
```
$ docker-compose up --build -d
```
- An environmental variable `ALADAR_BOT` have to be set to the bot token string 

