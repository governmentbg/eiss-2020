// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

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
            //services.AddHostedService<CsrdTimedHostedService>();
            //services.AddHostedService<EpepTimedHostedService>();
            //services.AddHostedService<CubipsaTimedHostedService>();
            //services.AddHostedService<IspnTimedHostedService>();


            services.AddScoped<IConsoleTaskExecuteMessageService, ConsoleTaskExecuteMessageService>();
            services.AddScoped<IConsoleTaskRecieverService, ConsoleTaskRecieverService>();
            services.AddScoped<IIOSignToolsService, IOSignToolsService>();
            services.AddScoped<ICdnService, CdnService>();
            //services.AddScoped<IEpepConnectionService, EpepConnectionService>();
            //services.AddScoped<IEpepService, EpepService>();
            services.AddScoped<ICubipsaConnectionService, CubipsaConnectionService>();
            services.AddScoped<ICubipsaService, CubipsaService>();
            services.AddScoped<IISPNCaseService, ISPNCaseService>();
            services.AddScoped<ICsrdService, CsrdService>();
            services.AddScoped<IEisppConnectionService, EisppConnectionService>();
            services.AddScoped<IEisppCommunicationService, EisppCommunicationService>();
            services.AddScoped<IEisppRulesService, EisppRulesService>();
            //services.AddSingleton<IEpepTHS, EpepTimedHostedService>();
            services.AddSingleton<ICsrdTHS, CsrdTimedHostedService>();
            services.AddSingleton<ICubipsaTHS, CubipsaTimedHostedService>();
            services.AddSingleton<IEisppTHS, EisppTimedHostedService>();
            services.AddSingleton<IeCaseService, IeCaseServiceClient>();

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
            services.AddHttpClient();
            //CSRD Client
            services.AddHttpClient("csrdHttpClient", client =>
            {
                var endPoint = config.GetValue<string>("CSRD:Endpoint");
                var method = config.GetValue<string>("CSRD:Method");
                client.BaseAddress = new Uri(new Uri(endPoint), method);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                var certificatePath = config.GetValue<string>("CSRD:CertificatePath");
                var certificatePassword = config.GetValue<string>("CSRD:CertificatePassword");
                HttpClientHandler result = new HttpClientHandler();
                if (!string.IsNullOrEmpty(certificatePath))
                {
                    var _cert = new X509Certificate2(certificatePath, certificatePassword);
                    result.ClientCertificates.Add(_cert);
                }
                result.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                return result;
            });

            //EPEP Client
            services.AddHttpClient<IeCaseServiceClient>("epepHttpClient", client =>
             {
                 var endPoint = config.GetValue<string>("EPEP:Endpoint");
                 client.BaseAddress = new Uri(endPoint);
             }).ConfigurePrimaryHttpMessageHandler(() =>
             {
                 var certificatePath = config.GetValue<string>("EPEP:CertificatePath");
                 var certificatePassword = config.GetValue<string>("EPEP:CertificatePassword");
                 HttpClientHandler result = new HttpClientHandler();
                 if (!string.IsNullOrEmpty(certificatePath))
                 {
                     var _cert = new X509Certificate2(certificatePath, certificatePassword);
                     result.ClientCertificates.Add(_cert);
                 }
                 result.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                 return result;
             });
        }

    }
}