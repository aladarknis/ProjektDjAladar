FROM mcr.microsoft.com/dotnet/sdk:6.0
WORKDIR /root

# ENV ALADAR_BOT=""

# Copy source
RUN mkdir src resources .git
ADD src src
ADD resources resources
ADD .git .git
COPY ProjektDjAladar.sln ./

# Build project
RUN dotnet publish -c Release -r linux-x64

# Start bot
WORKDIR src/ProjektDjAladar/bin/Release/net6.0/linux-x64/publish
CMD ["./ProjektDjAladar"]

