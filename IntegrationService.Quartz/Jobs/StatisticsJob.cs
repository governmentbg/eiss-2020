// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplicationService.Infrastructure.Contracts;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading.Tasks;

namespace IntegrationService.Quartz.Jobs
{
    /// <summary>
    /// PUSH - Генениране на статистика за текуща година
    /// </summary>
    [DisallowConcurrentExecution]
    internal class StatisticsJob : BaseJob
    {
        private readonly IStatisticsService service;
        public StatisticsJob(
            IStatisticsService _service,
            ILogger<StatisticsJob> _logger)
        {
            service = _service;
            logger = _logger;
        }
        protected override async Task DoJob(IJobExecutionContext context)
        {
            await service.PushMQWithFetch(1);
        }
    }
}
