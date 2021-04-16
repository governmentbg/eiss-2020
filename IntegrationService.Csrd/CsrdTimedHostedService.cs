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
            this.stopHours = configuration.GetValue<string>("StopHours");
            this.stopServiceTimeout = configuration.GetValue<int>("StopServiceTimeout");
            this.serviceProvider = serviceProvider;
        }

        public override void TimerElapsedAction()
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var csrdCaseService = scope.ServiceProvider.GetService<ICsrdService>();
                var result = csrdCaseService.PushMQWithFetch(fetchCount).GetAwaiter().GetResult();
            }
        }
    }
}
