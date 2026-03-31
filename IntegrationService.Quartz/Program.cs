using Microsoft.Extensions.Hosting;
using System;

namespace IntegrationService.Quartz
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("IntegrationService.Quartz started");
            using IHost host = Host.CreateDefaultBuilder(args)


           .ConfigureServices((hostContext, services) =>
           {
               services.AddAppDbContext(hostContext.Configuration);
               services.ConfigureServices(hostContext.Configuration);
               services.ConfigureHttpClients(hostContext.Configuration);

               services.AddQuartConfiguration(hostContext.Configuration);
           })
           .Build();
            
            host.Run();
        }
    }
}
