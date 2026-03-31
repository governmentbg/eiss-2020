using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplicationService.Infrastructure.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplicationService.Infrastructure.Services
{
    public class StatisticsService : BaseMQService, IStatisticsService
    {
        private readonly IConfiguration config;
        private readonly IStatisticsReportService reportService;
        private readonly string autoDays;

        public StatisticsService(
            IConfiguration _config,
            IRepository _repo,
            ILogger<CsrdService> _logger,
            IStatisticsReportService _reportService)
        {
            this.repo = _repo;
            config = _config;
            logger = _logger;
            reportService = _reportService;
            this.IntegrationTypeId = NomenclatureConstants.IntegrationTypes.Statistics;
            autoDays = config.GetValue<string>("EissStatistics:AutoCalcDaysInWeek");
        }

        protected override async Task InitMQ()
        {
            

            var mqID = $"{DateTime.Now:yyyy-MM-dd}";

            if (repo.AllReadonly<MQEpep>()
                        .Where(x => x.IntegrationTypeId == this.IntegrationTypeId && x.MQId == mqID && x.DateWrt > DateTime.Now.Date)
                        .Any())
            {
                return;
            }


            var newMq = new MQEpep()
            {
                DateWrt = DateTime.Now,
                IntegrationTypeId = this.IntegrationTypeId,
                IntegrationStateId = EpepConstants.IntegrationStates.New,
                TargetClassName = $"Автоматично стартиране",
                //Има право на 2 грешки, защото операцията продължава много дълго
                ErrorCount = EpepConstants.IntegrationMaxErrorCount - 2,
                MQId = mqID
            };

            repo.Add(newMq);
            await repo.SaveChangesAsync();
        }

        protected override async Task SendMQ(MQEpep mq)
        {
            int courtId = 0;
            if (mq.ParentSourceId > 0)
            {
                courtId = (int)mq.ParentSourceId;
            }
            var lastMonthDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1);
            try
            {
                bool result = await reportService.Statistics_DeleteSaveData(new DateTime(lastMonthDate.Year, 1, 1), lastMonthDate, courtId).ConfigureAwait(false);
                UpdateMQ(mq, result);
            }
            catch (Exception ex)
            {
                mq.ErrorDescription = ex.Message;
                UpdateMQ(mq, false);
            }
        }

        protected override Task<bool> InitChanel()
        {
            return Task.Run(() => true);
        }
    }
}
