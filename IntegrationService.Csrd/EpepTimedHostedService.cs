// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.DependencyInjection;
using IOWebApplicationService.Infrastructure.Contracts;
using IntegrationService.Csrd.Contracts;
using IOWebApplicationService.Infrastructure.Services;

namespace IntegrationService.Csrd
{
    public class EpepTimedHostedService : BaseTimedHostedService, IEpepTHS
    {
        public EpepTimedHostedService(
            IConfiguration configuration,
            IHostingEnvironment environment,
            ILogger<EpepTimedHostedService> logger,
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
                //this.logger.LogDebug("epepService started");
                var epepService = scope.ServiceProvider.GetService<IEpepService>();
                epepService.PushMQWithFetch(this.fetchCount);
                //this.logger.LogDebug("epepService ended");
            }
        }
    }
}
