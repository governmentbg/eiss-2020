// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using Microsoft.Extensions.Logging;

namespace IOWebApplication.Core.Services.Tasks
{
    public class ConsoleTaskExecuteMessageService : IConsoleTaskExecuteMessageService
    {
        private readonly ILogger logger;

        public ConsoleTaskExecuteMessageService(ILogger<ConsoleTaskExecuteMessageService> logger)
        {
            this.logger = logger;
        }

        public void DimitarMethod1()
        {
            this.logger.LogInformation("DimitarMethod 1 Called !!!!!!!!!!!");
        }

        public void DimitarMethod2()
        {
            this.logger.LogInformation("DimitarMethod 2 Called !!!!!!!!!!!");
        }
    }
}
