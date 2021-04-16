FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Customize runtime image
COPY custom-certificates/*.crt /usr/local/share/ca-certificates/
RUN update-ca-certificates --verbose
RUN ln -svf /usr/share/zoneinfo/Europe/Sofia /etc/localtime

# install ms corefonts
RUN sed -i 's/main$/main contrib/g' /etc/apt/sources.list && \
    apt-get update && \
    apt-get install -y ttf-mscorefonts-installer && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# required for wkhtmltopdf
RUN apt-get update && \
    apt-get install -y libgdiplus && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# install System.Drawing native dependencies
#        libgdiplus \
RUN apt-get update \
    && apt-get install -y --allow-unauthenticated \
        libc6-dev \
        libx11-dev \
     && rm -rf /var/lib/apt/lists/*

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src

# copy csproj so we can restore some stuff
COPY IOWebApplication/IOWebApplication.csproj IOWebApplication/
COPY IOWebApplication.Core/IOWebApplication.Core.csproj IOWebApplication.Core/
COPY IOWebApplication.Infrastructure/IOWebApplication.Infrastructure.csproj IOWebApplication.Infrastructure/

# add the nuget configs where needed so we can pull custom libs
COPY NuGet.config IOWebApplication/
COPY NuGet.config IOWebApplication.Core/
COPY NuGet.config IOWebApplication.Infrastructure/

# get packages
RUN dotnet restore IOWebApplication/IOWebApplication.csproj
COPY . .
WORKDIR /src/IOWebApplication
RUN dotnet build IOWebApplication.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish IOWebApplication.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
RUN chmod 755 /app/wwwroot/Rotativa/wkhtmltopdf /app/wwwroot/Rotativa/wkhtmltoimage

RUN echo '212.122.184.120 proxyvss.eispp-proxy.net' >> /etc/hosts

ENTRYPOINT ["dotnet", "IOWebApplication.dll"]
