using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.Integrations.Sisma;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface IStatisticsReportService : IBaseService
    {
        Task<List<ExcelReportData>> FillExcelData(DateTime fromDate, DateTime toDate, int courtId);

        Task<bool> Statistics_SaveData(DateTime fromDate, DateTime toDate, int courtId);

        Task<bool> Statistics_DeleteSaveData(DateTime fromDate, DateTime toDate, int courtId);
        Task<byte[]> TestPrintSisma(DateTime fromDate, DateTime toDate, int courtId, int sheetIndex);
        Task<SismaModel> GetSismaData(int reportMonth, int reportYear, int sheetIndex, string reportType);
        Task<List<ExcelReportData>> FillExcelData_Mediation(DateTime fromDate, DateTime toDate, int courtId);
        Task<bool> Statistics_DeleteSaveDataMediation(DateTime fromDate, DateTime toDate, int courtId);
    }
}
