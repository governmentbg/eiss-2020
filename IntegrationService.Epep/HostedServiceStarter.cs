// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IntegrationService.Epep.Contracts;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace IntegrationService.Epep
{
    public class HostedServiceStarter : IHostedService
    {
        private readonly IEpepTHS epep;

        public HostedServiceStarter(
            IEpepTHS _epep
        )
        {
            epep = _epep;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                Task.Run(() => epep.StartAsync(cancellationToken));

            }, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                Task.Run(() => epep.StopAsync(cancellationToken));
            }, cancellationToken);
        }
    }

}
