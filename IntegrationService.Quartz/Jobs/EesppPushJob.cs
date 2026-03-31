// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplicationService.Infrastructure.Contracts;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading.Tasks;

namespace IntegrationService.Quartz.Jobs
{
    /// <summary>
    /// PUSH услуга - подаване на данни за искания за правна помощ
    /// </summary>
    [DisallowConcurrentExecution]
    internal class EesppPushJob : BaseJob
    {
        private readonly IEesppService service;
        public EesppPushJob(
            IEesppService _service,
            ILogger<EesppJob> _logger)
        {
            service = _service;
            logger = _logger;
        }
        protected override Task DoJob(IJobExecutionContext context)
        {
            return service.PushMQWithFetch(GetFetchCount(context));
        }
    }
}
