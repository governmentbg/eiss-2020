// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading.Tasks;

namespace IntegrationService.Quartz.Jobs
{
    /// <summary>
    /// Корекция на данни 
    /// </summary>
    [DisallowConcurrentExecution]
    internal class DataUpdateJob : BaseJob
    {
        private readonly ICommonService service;
        public DataUpdateJob(
            ICommonService _service,
            ILogger<DataUpdateJob> _logger)
        {
            service = _service;
            logger = _logger;
        }
        protected override async Task DoJob(IJobExecutionContext context)
        {
            await service.SetDeclaredMonthCountForActs(GetFetchCount(context));
        }
    }
}
