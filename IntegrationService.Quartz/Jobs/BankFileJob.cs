// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace IntegrationService.Quartz.Jobs
{
    [DisallowConcurrentExecution]
    internal class BankFileJob : BaseJob
    {

        private readonly IServiceProvider serviceProvider;

        public BankFileJob(
            IServiceProvider _serviceProvider,
            ILogger<BankFileJob> _logger)
        {
            serviceProvider = _serviceProvider;
            logger = _logger;
        }
        protected override async Task DoJob(IJobExecutionContext context)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<IBankFileService>();
                await service.ReadBankFiles();
            };

        }
    }
}
