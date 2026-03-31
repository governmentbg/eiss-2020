// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplicationService.Infrastructure.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace IntegrationService.Quartz.Jobs
{
    [DisallowConcurrentExecution]
    internal class TestJob : BaseJob
    {
        private readonly IServiceProvider serviceProvider;
        private readonly long mqId;

        public TestJob(
            ILogger<TestJob> _logger,

            IServiceProvider serviceProvider,
            IConfiguration config)
        {
            logger = _logger;
            this.serviceProvider = serviceProvider;

            mqId = config.GetValue("EPEP:TestFastProcessSelectionId", 0);
        }
        protected override async Task DoJob(IJobExecutionContext context)
        {
            //if (mqId == 0)
            //{
            //    return;
            //}

            using (var scope = serviceProvider.CreateScope())
            {
                //var service = scope.ServiceProvider.GetService<IRnflRestService>();
                //await service.TestClient();
                //await service.SummonRecover_Report();

                var service = scope.ServiceProvider.GetService<IEpepDocumentService>();
                await service.TestAssign(50);
            }
            ;
            await Task.Yield();
        }
    }
}
