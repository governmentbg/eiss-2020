using Integration.Epep;
using IntegrationService.Csrd.Contracts;
using IO.SignTools.Contracts;
using IO.SignTools.Services;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Services;
using IOWebApplication.Core.Services.Tasks;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models;
using IOWebApplication.Infrastructure.Services;
using IOWebApplicationService.Infrastructure.Contracts;
using IOWebApplicationService.Infrastructure.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;

namespace IntegrationService.Csrd.Services
{
    public static class IOServiceAppServiceCollectionExtension
    {
        /// <summary>
        /// Регистрира услугите на приложението в IoC контейнера
        /// </summary>
        /// <param name="services">Регистрирани услуги</param>
        /// <param name="Configuration">Настройки на приложението</param>
        public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // services.AddLogging();
            services.AddHostedService<HostedServiceStarter>();


            services.AddScoped<IConsoleTaskExecuteMessageService, ConsoleTaskExecuteMessageService>();
            services.AddScoped<IConsoleTaskRecieverService, ConsoleTaskRecieverService>();
            services.AddScoped<IIOSignToolsService, IOSignToolsService>();
            services.AddScoped<ICdnService, CdnService>();

            services.AddScoped<ICubipsaConnectionService, CubipsaConnectionService>();
            services.AddScoped<ICubipsaService, CubipsaService>();
            services.AddScoped<IISPNCaseService, ISPNCaseService>();
            services.AddScoped<IISPNCaseService, ISPNCaseService>();
            services.AddScoped<ICsrdService, CsrdService>();
            services.AddScoped<IEisppConnectionService, EisppConnectionService>();
            services.AddScoped<IEisppCommunicationService, EisppCommunicationService>();
            services.AddScoped<IEisppRulesService, EisppRulesService>();

            services.AddSingleton<ICsrdTHS, CsrdTimedHostedService>();
            services.AddSingleton<ICubipsaTHS, CubipsaTimedHostedService>();
            services.AddSingleton<IEisppTHS, EisppTimedHostedService>();
            services.AddSingleton<IeCaseService, IeCaseServiceClient>();
            services.AddSingleton<IIspnTHS, IspnTimedHostedService>();

            services.Configure<FormOptions>(options =>
                  {
                      options.ValueLengthLimit = int.MaxValue;
                      options.MultipartBodyLengthLimit = long.MaxValue; // <-- !!! long.MaxValue
                      options.MultipartBoundaryLengthLimit = int.MaxValue;
                      options.MultipartHeadersCountLimit = int.MaxValue;
                      options.MultipartHeadersLengthLimit = int.MaxValue;
                  });

        }

        /// <summary>
        /// Регистрира контекстите на приложението в IoC контейнера
        /// </summary>
        /// <param name="services">Регистрирани услуги</param>
        /// <param name="Configuration">Настройки на приложението</param>
        public static void AddAppDbContext(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"), m => m.MigrationsAssembly("IOWebApplication.Infrastructure")));

            services.AddScoped(typeof(IRepository), typeof(Repository));

            services.AddSingleton<IMongoClient>(s => new MongoClient(Configuration.GetConnectionString("MongoDbConnection")));
        }

        /// <summary>
        /// Регистрира HttpClient-ите и сертификатите към тях
        /// </summary>
        /// <param name="services">Регистрирани услуги</param>
        /// <param name="Configuration">Настройки на приложението</param>
        public static void ConfigureHttpClients(this IServiceCollection services, IConfiguration config)
        {

            //services.AddHttpClient();

            //services.AddSingleton<ICsrdHttpRequester>(x =>
            //{
            //    var model = new CsrdHttpRequester();
            //    model.requester = new HttpRequester(
            //        config.GetValue<string>("CSRD:CertificatePath"),
            //        config.GetValue<string>("CSRD:CertificatePassword"),
            //        false);
            //    return model;
            //});

            //services.AddSingleton<IISPNHttpRequester>(x =>
            //{
            //    var model = new ISPNHttpRequester();
            //    model.requester = new HttpRequester(
            //        config.GetValue<string>("ISPN:CertificatePath"),
            //        config.GetValue<string>("ISPN:CertificatePassword"),
            //        true);
            //    return model;
            //});

            //CSRD Client
            services.AddHttpClient("csrdHttpClient", client =>
            {
                var endPoint = config.GetValue<string>("CSRD:Endpoint");
                var method = config.GetValue<string>("CSRD:Method");
                client.BaseAddress = new Uri(new Uri(endPoint), method);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                // return Program.HttpClientHandler_CSRD;
                var certificatePath = config.GetValue<string>("CSRD:CertificatePath");
                var certificatePassword = config.GetValue<string>("CSRD:CertificatePassword");
                HttpClientHandler result = new HttpClientHandler();
                if (!string.IsNullOrEmpty(certificatePath))
                {
                    var _cert = new X509Certificate2(certificatePath, certificatePassword);
                    result.ClientCertificates.Add(_cert);
                    result.ClientCertificateOptions = ClientCertificateOption.Manual;
                    result.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                }
                return result;
            });

            //ИСПН Client
            services.AddHttpClient("ispnHttpClient", client =>
            {
                client.BaseAddress = new Uri(config.GetValue<string>("ISPN:URI"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                //return Program.HttpClientHandler_ISPN;
                var certificatePath = config.GetValue<string>("ISPN:CertificatePath");
                var certificatePassword = config.GetValue<string>("ISPN:CertificatePassword");
                HttpClientHandler result = new HttpClientHandler();
                if (!string.IsNullOrEmpty(certificatePath))
                {
                    var _cert = new X509Certificate2(certificatePath, certificatePassword);
                    result.ClientCertificates.Add(_cert);
                    result.ClientCertificateOptions = ClientCertificateOption.Manual;
                    result.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                }
                return result;
            });

            ////EPEP Client
            //services.AddHttpClient<IeCaseServiceClient>("epepHttpClient", client =>
            // {
            //     var endPoint = config.GetValue<string>("EPEP:Endpoint");
            //     client.BaseAddress = new Uri(endPoint);
            // }).ConfigurePrimaryHttpMessageHandler(() =>
            // {
            //     var certificatePath = config.GetValue<string>("EPEP:CertificatePath");
            //     var certificatePassword = config.GetValue<string>("EPEP:CertificatePassword");
            //     HttpClientHandler result = new HttpClientHandler();
            //     if (!string.IsNullOrEmpty(certificatePath))
            //     {
            //         var _cert = new X509Certificate2(certificatePath, certificatePassword);
            //         result.ClientCertificates.Add(_cert);
            //     }
            //     result.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
            //     return result;
            // });
        }

    }
}