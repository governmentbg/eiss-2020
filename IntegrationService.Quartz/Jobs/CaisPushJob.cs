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
    /// PUSH услуга - Подаване данни за разпределение към ЦАЙС Съдебен статус
    /// </summary>
    [DisallowConcurrentExecution]
    internal class CaisPushJob : BaseJob
    {
        private readonly IServiceProvider serviceProvider;
        public CaisPushJob(
            IServiceProvider _serviceProvider,
            ILogger<CaisPushJob> _logger)
        {
            serviceProvider = _serviceProvider;
            logger = _logger;
        }
        protected override async Task DoJob(IJobExecutionContext context)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<ICaisService>();
                await service.PushMQWithFetch(GetFetchCount(context));
            };

        }
    }
}
