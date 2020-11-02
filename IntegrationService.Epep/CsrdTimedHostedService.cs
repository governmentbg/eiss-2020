// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.DependencyInjection;
using IOWebApplicationService.Infrastructure.Contracts;
using IntegrationService.Epep.Contracts;
using IOWebApplicationService.Infrastructure.Services;

namespace IntegrationService.Epep
{
    public class CsrdTimedHostedService : BaseTimedHostedService, ICsrdTHS
    {
        public CsrdTimedHostedService(
            IConfiguration configuration,
            IHostingEnvironment environment,
            ILogger<CsrdTimedHostedService> logger,
            IApplicationLifetime appLifetime,
            IServiceProvider serviceProvider)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.appLifetime = appLifetime;
            this.environment = environment;
            this.interval = configuration.GetValue<double>("TimerInterval");
            this.fetchCount = configuration.GetValue<int>("FetchCount");
            this.stopServiceTimeout = configuration.GetValue<int>("StopServiceTimeout");
            this.serviceProvider = serviceProvider;
        }

        public override void TimerElapsedAction()
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var csrdCaseService = scope.ServiceProvider.GetService<ICsrdService>();
                csrdCaseService.PushMQWithFetch(fetchCount);
            }
        }
    }
}
