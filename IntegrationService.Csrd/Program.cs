﻿using IntegrationService.Csrd.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

namespace IntegrationService.Csrd
{
    class Program
    {

        static void Main(string[] args)
        {
            BuildHost(args)
                .Run();
        }


        public static IHost BuildHost(string[] args)
        {
            return new HostBuilder()

                 .ConfigureHostConfiguration(configHost =>
                 {
                     configHost.SetBasePath(Directory.GetCurrentDirectory());
                     configHost.AddEnvironmentVariables(prefix: "ASPNETCORE_");
                     configHost.AddCommandLine(args);

                 })
                 .ConfigureServices((hostContext, services) =>
                 {

                     services.AddAppDbContext(hostContext.Configuration);
                     services.ConfigureServices(hostContext.Configuration);
                     //SetHttpHandlers(hostContext.Configuration);
                     services.ConfigureHttpClients(hostContext.Configuration);
                 })
                 .ConfigureAppConfiguration((hostContext, configApp) =>
                 {
                     configApp.SetBasePath(Directory.GetCurrentDirectory());
                     configApp.AddEnvironmentVariables(prefix: "ASPNETCORE_");
                     configApp.AddJsonFile($"appsettings.json", true);
                     configApp.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", true);
                     configApp.AddCommandLine(args);
                 })
                .ConfigureLogging((hostContext, configLogging) =>
                {
                    configLogging.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                    configLogging.AddConsole();
                    configLogging.AddDebug();
                }).Build();
        }


        /// <summary>
        /// Stupid name, because it is NOT a WebHost, 
        /// but EF Tools are looking for this particular method!!!
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHost BuildWebHost(string[] args)
        {
            return BuildHost(args);
        }
    }
}