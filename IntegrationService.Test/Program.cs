// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Integration.Eispp;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Http;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Services;
using IOWebApplicationService.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IOWebApplicationService.Infrastructure.Services;
using System;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Services;
using MongoDB.Driver;

namespace IntegrationService.Test
{
    class Program
    {
        private static IConfiguration _configuration;

        static void Main(string[] args)
        {
            var container = Initialize();
            // TestExperts(container);
            //TestEisppIntegration(container);
            TestStatistics(container);
        }

        static void TestExperts(IServiceProvider container)
        {
            var service = container.GetService<IExpertsSearchService>();
            var experts = service.SearchExperts().Result;

            foreach (var item in experts)
            {
                Console.WriteLine("Вещо лице: {0}", item.FullName);
                Console.Write("Специалности: ");

                foreach (var competence in item.Competences)
                {
                    Console.WriteLine("{0} {1}", competence.Code, competence.Name);
                }

                Console.Write("Райони: ");

                foreach (var region in item.CourtRegions)
                {
                    Console.WriteLine(region);
                }

                Console.WriteLine();
            }
        }

        static void TestEisppIntegration(IServiceProvider container)
        {
            var service = container.GetService<IEisppCommunicationService>();
            service.SendReceiveMessages();
        }

        static void TestStatistics(IServiceProvider container)
        {
            var service = container.GetService<IStatisticsReportService>();
            
            service.StatisticsTest();
        }


        static IServiceProvider Initialize()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            return new ServiceCollection()
                .AddLogging()
                .AddSingleton<IConfiguration>(_configuration)
                .AddDbContext<ApplicationDbContext>(options =>
                    options.UseNpgsql(_configuration.GetConnectionString("DefaultConnection"), 
                        m => m.MigrationsAssembly("IOWebApplication.Infrastructure")))
                .AddScoped(typeof(IRepository), typeof(Repository))
                .AddScoped<ICdnService, CdnService>()
                .AddScoped<IEisppConnectionService, EisppConnectionService>()
                .AddScoped<IEisppCommunicationService, EisppCommunicationService>()
                .AddScoped<IExpertsSearchService, ExpertsSearchService>()
                .AddScoped<IHttpRequester, HttpRequester>()
                .AddScoped<ICdnService, CdnService>()
                .AddScoped<IEisppRulesService, EisppRulesService>()
                .AddScoped<IStatisticsReportService, StatisticsReportService>()
                .AddSingleton<IMongoClient>(s => new MongoClient(_configuration.GetConnectionString("MongoDbConnection")))
                .BuildServiceProvider();
        }

    }
}
