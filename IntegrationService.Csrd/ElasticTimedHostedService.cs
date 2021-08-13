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
    public class ElasticTimedHostedService : BaseTimedHostedService, IElasticTHS
    {
        public ElasticTimedHostedService(
            IConfiguration configuration,
            IHostingEnvironment environment,
            ILogger<ElasticTimedHostedService> logger,
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
                this.logger.LogDebug("elasticService started");
                var elasticService = scope.ServiceProvider.GetService<IElasticIndexService>();
                var _result = elasticService.PushMQWithFetch(fetchCount).GetAwaiter().GetResult();
                
                this.logger.LogDebug("elasticService ended");
            }
        }
    }
}
