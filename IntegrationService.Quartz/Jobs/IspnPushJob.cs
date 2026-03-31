// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplicationService.Infrastructure.Contracts;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace IntegrationService.Quartz.Jobs
{
    /// <summary>
    /// PUSH услуга - Подаване данни за дела по несъстоятелност към ИСПН
    /// </summary>
    [DisallowConcurrentExecution]
    internal class IspnPushJob : BaseJob
    {
        private readonly IISPNCaseService service;
        public IspnPushJob(
            IISPNCaseService _service,
            ILogger<IspnPushJob> _logger)
        {
            service = _service;
            logger = _logger;
        }
        protected override async Task DoJob(IJobExecutionContext context)
        {
            try
            {
                await service.PushMQWithFetch(GetFetchCount(context));
            }
            catch (Exception ex)
            {
                logger.LogError($"IspnPushJob:{ex.Message};{ex?.InnerException?.Message}");
            }
        }
    }
}
