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
    /// Обработка на нотификации и срокове в ЕИСС
    /// </summary>
    [DisallowConcurrentExecution]
    internal class EissProcessJob : BaseJob
    {
        private readonly IServiceProvider serviceProvider;
        public EissProcessJob(
            IServiceProvider _serviceProvider,
            ILogger<EissProcessJob> _logger)
        {
            serviceProvider = _serviceProvider;
            logger = _logger;
        }
        protected override async Task DoJob(IJobExecutionContext context)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<IEissProcessService>();
                await service.PushMQWithFetch(GetFetchCount(context));
            }
        }
    }
}
