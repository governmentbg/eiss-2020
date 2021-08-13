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
COPY IntegrationService.Csrd/IntegrationService.Csrd.csproj IntegrationService.Csrd/
COPY IOWebApplication.Core/IOWebApplication.Core.csproj IOWebApplication.Core/
COPY IOWebApplication.Infrastructure/IOWebApplication.Infrastructure.csproj IOWebApplication.Infrastructure/
COPY IOWebApplicationService.Infrastructure/IOWebApplicationService.Infrastructure.csproj IOWebApplicationService.Infrastructure/

# add the nuget configs where needed so we can pull custom libs
COPY NuGet.config IntegrationService.Csrd/
COPY NuGet.config IOWebApplication.Core/
COPY NuGet.config IOWebApplication.Infrastructure/
COPY NuGet.config IOWebApplicationService.Infrastructure/

# get packages
RUN dotnet restore IntegrationService.Csrd/IntegrationService.Csrd.csproj
COPY . .
WORKDIR /src/IntegrationService.Csrd
RUN dotnet build IntegrationService.Csrd.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish IntegrationService.Csrd.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .

ENTRYPOINT ["dotnet", "IntegrationService.Csrd.dll"]
