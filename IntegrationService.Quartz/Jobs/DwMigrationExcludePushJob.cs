// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Helper;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplicationService.Infrastructure.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IntegrationService.Quartz.Jobs
{
    /// <summary>
    /// PUSH - Изпращане на променените обекти към MS SQL базата за DW
    /// </summary>
    [DisallowConcurrentExecution]
    internal class DwMigrationExcludePushJob : BaseJob
    {
        private readonly IServiceProvider serviceProvider;
        private readonly int[] excludeCourtIds;
        public DwMigrationExcludePushJob(
            IServiceProvider _serviceProvider,
            ILogger<DwMigrationExcludePushJob> _logger,
            IConfiguration _config)
        {
            serviceProvider = _serviceProvider;
            logger = _logger;
            excludeCourtIds = _config.GetValue<string>("DW:ExcludeCourtIds").StringToIntArray();
        }
        protected override async Task DoJob(IJobExecutionContext context)
        {
            if (!excludeCourtIds.Any())
            {
                return;
            }
            using (var scope = serviceProvider.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<IDWService>();
                await service.MigrateAllForCourt(excludeCourtIds);
            }
        }
    }
}
