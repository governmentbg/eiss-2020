FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 8080
EXPOSE 443
RUN ln -svf /usr/share/zoneinfo/Europe/Sofia /etc/localtime

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY IOWebApplication.Cdn/IOWebApplication.Cdn.csproj IOWebApplication.Cdn/

# add the nuget configs where needed so we can pull custom libs
COPY NuGet.config IOWebApplication.Cdn/
RUN dotnet restore IOWebApplication.Cdn/IOWebApplication.Cdn.csproj
COPY . .
WORKDIR /src/IOWebApplication.Cdn
RUN dotnet build IOWebApplication.Cdn.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish IOWebApplication.Cdn.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "IOWebApplication.Cdn.dll"]
