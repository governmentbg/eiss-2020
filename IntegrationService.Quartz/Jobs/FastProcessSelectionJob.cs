// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplicationService.Infrastructure.Contracts;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace IntegrationService.Quartz.Jobs
{
    [DisallowConcurrentExecution]
    internal class FastProcessSelectionJob : BaseJob
    {
        private readonly IFastProcessSelectionCourtService service;

        public FastProcessSelectionJob(
            ILogger<FastProcessSelectionJob> _logger,
            IFastProcessSelectionCourtService _service)
        {
            logger = _logger;
            service = _service;
        }
        protected override async Task DoJob(IJobExecutionContext context)
        {
            DateTime dtNow = DateTime.Now;
            bool result = await service.CreateInitializationSelectionForCourtByDate(dtNow);
        }
    }
}
