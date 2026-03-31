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
    /// PUSH - изпращане на данни към АВ, РНФЛ
    /// </summary>
    [DisallowConcurrentExecution]
    internal class RnflPushJob : BaseJob
    {
        private readonly IServiceProvider serviceProvider;
        public RnflPushJob(
            IServiceProvider _serviceProvider,
            ILogger<RnflPushJob> _logger)
        {
            serviceProvider = _serviceProvider;
            logger = _logger;
        }
        protected override async Task DoJob(IJobExecutionContext context)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<IRnflRestService>();
                await service.PushMQWithFetch(GetFetchCount(context));
            }
        }
    }
}
