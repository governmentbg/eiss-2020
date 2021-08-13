using IntegrationService.Csrd.Services;
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

        //public static HttpClientHandler HttpClientHandler_CSRD;
        //public static HttpClientHandler HttpClientHandler_ISPN;
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

        //private static void SetHttpHandlers(IConfiguration config)
        //{
        //    var csrdCertificatePath = config.GetValue<string>("CSRD:CertificatePath");
        //    var csrdCcertificatePassword = config.GetValue<string>("CSRD:CertificatePassword");
        //    HttpClientHandler_CSRD = new HttpClientHandler();
        //    if (!string.IsNullOrEmpty(csrdCertificatePath))
        //    {
        //        var _cert = new X509Certificate2(csrdCertificatePath, csrdCcertificatePassword);
        //        HttpClientHandler_CSRD.ClientCertificates.Add(_cert);
        //        HttpClientHandler_CSRD.ClientCertificateOptions = ClientCertificateOption.Manual;
        //        HttpClientHandler_CSRD.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
        //    }

        //    var ispnCertificatePath = config.GetValue<string>("ISPN:CertificatePath");
        //    var ispnCcertificatePassword = config.GetValue<string>("ISPN:CertificatePassword");
        //    HttpClientHandler_ISPN = new HttpClientHandler();
        //    if (!string.IsNullOrEmpty(ispnCertificatePath))
        //    {
        //        var _cert = new X509Certificate2(ispnCertificatePath, ispnCcertificatePassword);
        //        HttpClientHandler_ISPN.ClientCertificates.Add(_cert);
        //        HttpClientHandler_ISPN.ClientCertificateOptions = ClientCertificateOption.Manual;
        //        HttpClientHandler_ISPN.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
        //    }
        //}

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