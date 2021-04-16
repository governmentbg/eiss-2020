using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplicationService.Infrastructure.Contracts;
using System;

namespace IOWebApplicationService.Infrastructure.Services
{
    public class StatisticsService : IStatisticsService
    {
        protected IRepository repo;
        private readonly IStatisticsReportService reportService;

        public StatisticsService(IRepository _repo, IStatisticsReportService _reportService)
        {
            repo = _repo;
            reportService = _reportService;
        }

        public void StatisticsTest()
        {
            var last = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1);
            reportService.Statistics_SaveData(new DateTime(last.Year, 1, 1), last, 0);
        }        
    }
}
