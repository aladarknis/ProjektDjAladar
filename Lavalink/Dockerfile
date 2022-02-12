FROM debian:latest

RUN apt-get update && apt-get install -y wget

# Install open-jdk
RUN wget https://download.java.net/java/GA/jdk17/0d483333a00540d886896bac774ff48b/35/GPL/openjdk-17_linux-x64_bin.tar.gz && \
tar xvf openjdk-17*_bin.tar.gz -C /usr/bin && \
rm openjdk-17*_bin.tar.gz

# Edit PATH
ENV PATH="/usr/bin/jdk-17/bin:${PATH}"

RUN mkdir Lavalink
WORKDIR ./Lavalink
COPY Lavalink.jar application.yml ./

CMD ["java", "-jar", "Lavalink.jar"]

