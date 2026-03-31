// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplicationService.Infrastructure.Contracts;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading.Tasks;

namespace IntegrationService.Quartz.Jobs
{
    /// <summary>
    /// PUSH - Подаване на данни към СИСМА
    /// </summary>
    [DisallowConcurrentExecution]
    internal class SismaJob : BaseJob
    {
        private readonly ISismaService service;
        public SismaJob(
            ISismaService _service,
            ILogger<SismaJob> _logger)
        {
            service = _service;
            logger = _logger;
        }
        protected override async Task DoJob(IJobExecutionContext context)
        {
            await service.FetchResult();
        }
    }
}
