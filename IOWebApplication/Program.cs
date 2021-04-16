using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace IOWebApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                //.AddJsonFile("hosting.json", optional: true)
                .AddCommandLine(args)
                .Build();
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            return WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(config)
                .UseStartup<Startup>()
                .UseKestrel(options =>
                {
                    options.Limits.MaxRequestBodySize = null;
                    if (environment == "LocalDebug" || environment == "Development")
                    {
                        options.ListenAnyIP(8080, listenOptions =>
                        {
                            listenOptions.UseHttps("Certificates/eiss.local.pfx", "123456");
                        });
                    }
                })
                .Build();
        }
    }
}
