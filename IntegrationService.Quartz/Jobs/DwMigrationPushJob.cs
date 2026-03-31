// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplicationService.Infrastructure.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace IntegrationService.Quartz.Jobs
{
    /// <summary>
    /// PUSH - Изпращане на променените обекти към MS SQL базата за DW
    /// </summary>
    [DisallowConcurrentExecution]
    internal class DwMigrationPushJob : BaseJob
    {
        private readonly IServiceProvider serviceProvider;
        public DwMigrationPushJob(
            IServiceProvider _serviceProvider,
            ILogger<DwMigrationPushJob> _logger)
        {
            serviceProvider = _serviceProvider;
            logger = _logger;
        }
        protected override async Task DoJob(IJobExecutionContext context)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<IDWService>();
                await service.MigrateAllForCourt();
            }
        }
    }
}
