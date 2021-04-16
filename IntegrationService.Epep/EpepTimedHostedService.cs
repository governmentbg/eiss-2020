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
    public class EpepTimedHostedService : BaseTimedHostedService, IEpepTHS
    {
        //IEpepService epepService;
        public EpepTimedHostedService(
            //IEpepService epepService,
            IConfiguration configuration,
            IHostingEnvironment environment,
            ILogger<EpepTimedHostedService> logger,
            IApplicationLifetime appLifetime,
            IServiceProvider serviceProvider)
        {
            //this.epepService = epepService;
            this.configuration = configuration;
            this.logger = logger;
            this.appLifetime = appLifetime;
            this.environment = environment;
            this.interval = configuration.GetValue<double>("TimerInterval");
            this.fetchCount = configuration.GetValue<int>("FetchCount");
            this.stopHours = configuration.GetValue<string>("StopHours");
            this.stopServiceTimeout = configuration.GetValue<int>("StopServiceTimeout");
            this.serviceProvider = serviceProvider;
        }

        public override void TimerElapsedAction()
        {

            //epepService.PushMQWithFetch(this.fetchCount).ConfigureAwait(false);
            using (var scope = serviceProvider.CreateScope())
            {
                //fetchCount
                var epepService = scope.ServiceProvider.GetService<IEpepService>();
                // var epepResult = epepService.Correction().GetAwaiter().GetResult();
                var epepResult = epepService.PushMQWithFetch(fetchCount).GetAwaiter().GetResult();
            }
        }
    }
}
