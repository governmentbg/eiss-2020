// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading.Tasks;

namespace IntegrationService.Quartz.Jobs
{
    [DisallowConcurrentExecution]
    internal class TransactionResetJob : BaseJob
    {
        private readonly ITransactionService service;
        private readonly IConfiguration config;
        public TransactionResetJob(
            ITransactionService _service,
            IConfiguration _config,
            ILogger<TransactionResetJob> _logger)
        {
            service = _service;
            config = _config;
            logger = _logger;
        }
        protected override async Task DoJob(IJobExecutionContext context)
        {

            int fetchCount = this.GetFetchCount(context);
            await service.ReturnToWaiting(fetchCount, config.GetValue<int>("TransactionReset:WaitMinutes", 30));
        }
    }
}
