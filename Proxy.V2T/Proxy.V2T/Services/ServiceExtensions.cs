// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Contracts.Integration;
using IOWebApplication.Core.Services;
using IOWebApplication.Core.Services.Mocks;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.Integrations.EpepRest;
using IOWebApplication.Infrastructure.Services;
using IOWebApplication.Infrastructure.Services.Mocks;
using IOWebApplicationService.Infrastructure.Services.Intergation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Proxy.EISS.Contracts;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;

namespace Proxy.EISS.Services
{
    public static class ServiceExtensions
    {
        public static void ConfigureBuilder(this WebApplicationBuilder builder)
        {
            builder.Configuration.AddJsonFile("hosting.json", true, true).AddEnvironmentVariables("ASPNETCORE_");
            //builder.WebHost.ConfigureKestrel(kestrel =>
            //{
            //    //kestrel.Limits.MaxRequestBodySize = null;
            //    //kestrel.Limits.MaxRequestLineSize = 100000;
            //    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            //    if (environment == "LocalDebug" || environment == "Development")
            //    {
            //        kestrel.Listen(IPAddress.Loopback, 8080, portOptions =>
            //        {
            //            //var certificate = new X509Certificate("Certificates/eiss.local.pfx", "123456");
            //            try
            //            {
            //                //portOptions.UseHttps("Certificates/eiss.local.pfx", "123456");
            //                // portOptions.UseHttps( certificate.GetRawCertDataString());
            //                var certificate = new X509Certificate2("Certificates/eiss.local.pfx", "123456");
            //                portOptions.UseHttps(new HttpsConnectionAdapterOptions
            //                {
            //                    ServerCertificate = certificate
            //                });
            //            }
            //            catch (Exception ex) { }
            //        });
            //    }
            //});


        }




        public static void ConfigureServices(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddAuthorization();
            services.AddControllers();
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "ЕИСС",
                    Version = "v1",
                    Description = "API услуги на ЕИСС"
                });
                //c.IncludeXmlComments($@"{AppContext.BaseDirectory}Proxy.EISS.xml");
                //c.IncludeXmlComments($@"{AppContext.BaseDirectory}IOWebApplication.Infrastructure.xml");
            });

            services.AddCors(options =>
            {
                options.AddPolicy(name: "allowAll",
                                  policy =>
                                  {
                                      policy.WithOrigins("*").AllowAnyHeader().AllowAnyHeader();
                                  });
            });

            services.AddHttpContextAccessor();
            services.AddScoped<IUrlHelper, MockUrlHelper>();
            //services.AddScoped<IHttpContextAccessor, MockHttpContextAccessor>();
            services.AddScoped<ICounterService, MockCounterService>();

            services.AddScoped<IEpepConnectionService, EpepConnectionService>();
            services.AddScoped<IUserContext, MockUserContext>();
            services.AddScoped<ICaisBuletinService, CaisBuletinService>();
            services.AddScoped<ICaisMapperService, CaisMapperService>();
            services.AddScoped<ICaisConnectionFactory, CaisConnectionFactory>();
            services.AddScoped<IApiService, ApiService>();
            services.AddScoped<IEpepRestClient, EpepRestClient>();
            services.Configure<EpepRestConfigurationVM>(Configuration.GetSection("EPEP"));
            services.Configure<CdnConfigVM>(Configuration);

            services.ConfigureHttpClients(Configuration);
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
            //services.AddSingleton<IMongoClient>(s => new MongoClient(Configuration.GetConnectionString("MongoDbConnection")));

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }
        public static void ConfigureApplication(this WebApplication app, IConfiguration configuration)
        {
            var env = app.Environment;

            app.Use((authContext, next) =>
            {
                authContext.Request.Scheme = "https";
                return next();
            });

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            app.UseCors();
            app.MapControllers();
        }

        /// <summary>
        /// Регистрира HttpClient-ите и сертификатите към тях
        /// </summary>
        /// <param name="services">Регистрирани услуги</param>
        /// <param name="Configuration">Настройки на приложението</param>
        public static void ConfigureHttpClients(this IServiceCollection services, IConfiguration config)
        {
            var newEpep = config.GetValue<bool>("EPEP:RestActive", false);
            if (newEpep)
            {
                //EPEP Rest Client
                services.AddHttpClient(EpepRestClient.FactoryName, client =>
                {
                    client.Timeout = new TimeSpan(0, 1, 30);
                });
            }

            //ЦАЙС Client
            services.AddHttpClient("caisHttpClient", client =>
            {
                client.BaseAddress = new Uri(config.GetValue<string>("CAIS:URI"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
                var soapAction = config.GetValue<string>("CAIS:SoapAction", "http://cs.mjs.bg/EISSServicesModel-v1.0/IEISSIntegrationService/ValidateBulletin");
                client.DefaultRequestHeaders.Add("SOAPAction", soapAction);
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                var certificatePath = config.GetValue<string>("CAIS:CertificatePath");
                var certificatePassword = config.GetValue<string>("CAIS:CertificatePassword");
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
        }

    }
}
