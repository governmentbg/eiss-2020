// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.DependencyInjection;
using IOWebApplicationService.Infrastructure.Contracts;
using IOWebApplicationService.Infrastructure.Services;
using IntegrationService.Epep.Contracts;

namespace IntegrationService.Epep
{
    public class CubipsaTimedHostedService : BaseTimedHostedService, ICubipsaTHS
    {
        public CubipsaTimedHostedService(
            IConfiguration configuration,
            IHostingEnvironment environment,
            ILogger<CubipsaTimedHostedService> logger,
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
                this.logger.LogDebug("cubipsaService started");
                var cubipsaService = scope.ServiceProvider.GetService<ICubipsaService>();
                cubipsaService.PushMQWithFetch(fetchCount);
                this.logger.LogDebug("cubipsaService ended");
            }
        }
    }
}
