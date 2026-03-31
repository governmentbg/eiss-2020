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
    /// FIX - Възстановяване на заявки по ID_List
    /// </summary>
    [DisallowConcurrentExecution]
    internal class MqRecoverJob : BaseJob
    {
        private readonly IServiceProvider serviceProvider;
        public MqRecoverJob(
            IServiceProvider _serviceProvider,
            ILogger<MqRecoverJob> _logger)
        {
            serviceProvider = _serviceProvider;
            logger = _logger;
        }
        protected override async Task DoJob(IJobExecutionContext context)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<IMqRecoverService>();
                await service.PushMQWithFetch(GetFetchCount(context));
            }
        }
    }
}
