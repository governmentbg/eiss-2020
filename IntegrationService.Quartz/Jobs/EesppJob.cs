// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplicationService.Infrastructure.Contracts;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading.Tasks;

namespace IntegrationService.Quartz.Jobs
{
    /// <summary>
    /// FETCH - Проверка за назначени адвокати от ЕЕСПП
    /// </summary>
    [DisallowConcurrentExecution]
    internal class EesppJob : BaseJob
    {
        private readonly IEesppService service;
        public EesppJob(
            IEesppService _service,
            ILogger<EesppJob> _logger)
        {
            service = _service;
            logger = _logger;
        }
        protected override async Task DoJob(IJobExecutionContext context)
        {
            await service.FetchResult(GetFetchCount(context));
        }
    }
}
