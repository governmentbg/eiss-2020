FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
RUN ln -svf /usr/share/zoneinfo/Europe/Sofia /etc/localtime

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src

# copy csproj so we can restore some stuff
COPY IOWebApplicationApi/IOWebApplicationApi.csproj IOWebApplicationApi/
COPY IOWebApplication.Core/IOWebApplication.Core.csproj IOWebApplication.Core/
COPY IOWebApplication.Infrastructure/IOWebApplication.Infrastructure.csproj IOWebApplication.Infrastructure/

# add the nuget configs where needed so we can pull custo libs
COPY NuGet.config IOWebApplicationApi/
COPY NuGet.config IOWebApplication.Core/
COPY NuGet.config IOWebApplication.Infrastructure/

# add the nuget configs where needed so we can pull custom libs
RUN dotnet restore IOWebApplicationApi/IOWebApplicationApi.csproj
COPY . .
WORKDIR /src/IOWebApplicationApi
RUN dotnet build IOWebApplicationApi.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish IOWebApplicationApi.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "IOWebApplicationApi.dll"]
