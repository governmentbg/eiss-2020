FROM microsoft/dotnet:2.2-sdk

RUN curl -sL https://deb.nodesource.com/setup_12.x | bash - && \
    apt-get update && \
    apt-get install -yy openjdk-8-jre-headless patch nodejs && \
    apt-get clean && \
    dotnet tool install --global dotnet-sonarscanner && \
    export PATH="$PATH:/root/.dotnet/tools"
