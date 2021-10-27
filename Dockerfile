FROM mcr.microsoft.com/dotnet/sdk:5.0
WORKDIR /root

RUN apt-get update && \
apt-get install -y wget

# Install open-jdk
RUN wget https://download.java.net/java/GA/jdk17/0d483333a00540d886896bac774ff48b/35/GPL/openjdk-17_linux-x64_bin.tar.gz && \
tar xvf openjdk-17*_bin.tar.gz -C /usr/bin && \
rm openjdk-17*_bin.tar.gz 
   
# Edit PATH   
ENV PATH="/usr/bin/jdk-17/bin:${PATH}"

# Copy source
RUN mkdir src Lavalink
ADD src src
ADD Lavalink Lavalink
COPY start.sh .

# Build project
RUN dotnet publish -c Release -r linux-x64

# Start bot
CMD ["./start.sh"]
