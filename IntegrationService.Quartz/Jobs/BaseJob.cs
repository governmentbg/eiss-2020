// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading.Tasks;

namespace IntegrationService.Quartz.Jobs
{
    internal class BaseJob : IJob
    {
        internal ILogger<BaseJob> logger;

        protected virtual async Task DoJob(IJobExecutionContext context) { await Task.Yield(); }
        public async Task Execute(IJobExecutionContext context)
        {
            QuartzLog(context, "started.");
            await DoJob(context).ConfigureAwait(false);
            QuartzLog(context, "finished.");
        }

        private void QuartzLog(IJobExecutionContext context, string message = "")
        {
            logger.LogInformation($"{context.JobDetail.Description} {message}");
        }

        protected int GetFetchCount(IJobExecutionContext context)
        {
            if (context == null)
            {
                return 0;
            }
            return context.MergedJobDataMap.GetIntValue("fetchCount");
        }
    }
}
