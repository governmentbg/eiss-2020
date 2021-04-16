using IOWebApplication.Infrastructure.Data.Models.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface IStatisticsReportService : IBaseService
    {
        List<ExcelReportData> FillExcelData(DateTime fromDate, DateTime toDate, int courtId);

        bool Statistics_SaveData(DateTime fromDate, DateTime toDate, int courtId);

        bool Statistics_DeleteSaveData(DateTime fromDate, DateTime toDate, int courtId);
    }
}
