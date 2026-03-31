// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplicationService.Infrastructure.Contracts;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading.Tasks;

namespace IntegrationService.Quartz.Jobs
{        
    /// <summary>
    /// PUSH услуга - индексиране на обезличени съдебни актове за пълнотекстово търсене
    /// </summary>
    [DisallowConcurrentExecution]
    internal class ElasticPushJob : BaseJob
    {
        private readonly IElasticIndexService service;
        public ElasticPushJob(
            IElasticIndexService _service,
            ILogger<ElasticPushJob> _logger)
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
