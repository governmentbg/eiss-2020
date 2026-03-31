// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplicationService.Infrastructure.Contracts;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading.Tasks;

namespace IntegrationService.Quartz.Jobs
{
    /// <summary>
    /// PUSH услуга - Подаване на данни за съобщения към ЕИСПП
    /// </summary>
    [DisallowConcurrentExecution]
    internal class EisppPushJob : BaseJob
    {
        private readonly IEisppCommunicationService service;
        public EisppPushJob(
            IEisppCommunicationService _service,
            ILogger<EisppPushJob> _logger)
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
