// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplicationService.Infrastructure.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace IntegrationService.Quartz.Jobs
{
    /// <summary>
    /// FETCH - Проверка за връчени призовки, потребители и подадени електронни документи през ЕПЕП
    /// </summary>
    [DisallowConcurrentExecution]
    internal class EpepSlowJob : BaseJob
    {
        private readonly IServiceProvider serviceProvider;
        public EpepSlowJob(
             IServiceProvider _serviceProvider,
            ILogger<EpepJob> _logger)
        {
            serviceProvider = _serviceProvider;
            logger = _logger;
        }
        protected override async Task DoJob(IJobExecutionContext context)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<IEpepCaseMigrationService>();
                var result = await service.FetchCaseMigrations();
            }
            using (var scope = serviceProvider.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<IEpepService>();
                await service.UpdateEkStreets();
            }
        }
    }
}
