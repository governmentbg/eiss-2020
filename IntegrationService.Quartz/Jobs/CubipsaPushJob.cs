// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplicationService.Infrastructure.Contracts;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading.Tasks;

namespace IntegrationService.Quartz.Jobs
{        
    /// <summary>
    /// PUSH услуга - Подаване данни за финализиращи актове към ЦУБИПСА
    /// </summary>
    [DisallowConcurrentExecution]
    internal class CubipsaPushJob : BaseJob
    {
        private readonly ICubipsaService service;
        public CubipsaPushJob(
            ICubipsaService _service,
            ILogger<CubipsaPushJob> _logger)
        {
            service = _service;
            logger = _logger;
        }
        protected override async Task DoJob(IJobExecutionContext context)
        {
            await service.PushMQWithFetch(GetFetchCount(context));
        }
    }
}
