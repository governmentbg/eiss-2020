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
    /// Регистрира документи от регистъра за централизирано разпределение
    /// </summary>
    [DisallowConcurrentExecution]
    internal class EpepRegisterJob : BaseJob
    {
        private readonly IServiceProvider serviceProvider;

        public EpepRegisterJob(
            IServiceProvider _serviceProvider,
            ILogger<EpepRegisterJob> _logger)
        {
            serviceProvider = _serviceProvider;
            logger = _logger;
        }
        protected override async Task DoJob(IJobExecutionContext context)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<IEpepDocumentService>();
                int fetchCount = GetFetchCount(context);
                if (fetchCount == 25)
                {
                    logger.LogError("Старт разпределяне");
                }
                await service.FastProcessRegisterInCourt(GetFetchCount(context));
                if (fetchCount == 25)
                {
                    logger.LogError("Край разпределяне");
                }
            };

        }
    }
}
