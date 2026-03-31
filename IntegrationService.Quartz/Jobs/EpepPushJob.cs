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
    /// PUSH - изпращане на всички данни от ЕИСС към ЕПЕП
    /// </summary>
    [DisallowConcurrentExecution]
    internal class EpepPushJob : BaseJob
    {
        private readonly IServiceProvider serviceProvider;
        public EpepPushJob(
            IServiceProvider _serviceProvider,
            ILogger<EpepPushJob> _logger)
        {
            serviceProvider = _serviceProvider;
            logger = _logger;
        }
        protected override async Task DoJob(IJobExecutionContext context)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<IEpepService>();
                var result = await service.PushMQWithFetch(GetFetchCount(context));
            }           
        }
    }
}
