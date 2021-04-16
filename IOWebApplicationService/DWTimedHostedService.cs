using IOWebApplicationService.Infrastructure.Contracts;
using IOWebApplicationService.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace IOWebApplicationService
{
    /// <summary>
    /// Имплементация на универсална услуга с Таймер
    /// (Windows Service / Linux Systemd Service)
    /// </summary>
    public class DWTimedHostedService : BaseTimedHostedService
    {
        private IDWService dwService;

        public DWTimedHostedService(
            IConfiguration configuration,
            IHostingEnvironment environment,
            ILogger<DWTimedHostedService> logger,
            IApplicationLifetime appLifetime,
            IServiceProvider serviceProvider
            //IDWService _dwService
            )
        {
            this.configuration = configuration;
            this.logger = logger;
            this.appLifetime = appLifetime;
            this.environment = environment;
            this.interval = configuration.GetValue<double>("DWTimerInterval");
            this.stopServiceTimeout = configuration.GetValue<int>("StopServiceTimeout");
            this.serviceProvider = serviceProvider;
            //dwService = _dwService;
        }

        public override void TimerElapsedAction()
        {
            using (var scope = serviceProvider.CreateScope())
            {

                //processCompleted = false;
                var scopedDwService = scope.ServiceProvider.GetService<IDWService>();

                scopedDwService.MigrateAllForCourt(null);


                //processCompleted = true;
            }
        }
    }
}