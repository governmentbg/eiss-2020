using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.DependencyInjection;
using IOWebApplicationService.Infrastructure.Contracts;
using IOWebApplicationService.Infrastructure.Services;
using IntegrationService.Csrd.Contracts;

namespace IntegrationService.Csrd
{
    public class EproTimedHostedService : BaseTimedHostedService, IEproTHS
    {
        public EproTimedHostedService(
            IConfiguration configuration,
            IHostingEnvironment environment,
            ILogger<EproTimedHostedService> logger,
            IApplicationLifetime appLifetime,
            IServiceProvider serviceProvider)
        {
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
            using (var scope = serviceProvider.CreateScope())
            {
                this.logger.LogDebug("eproService started");
                var eproService = scope.ServiceProvider.GetService<IEproService>();
                var _result = eproService.PushMQWithFetch(fetchCount).GetAwaiter().GetResult();
                
                this.logger.LogDebug("eproService ended");
            }
        }
    }
}
