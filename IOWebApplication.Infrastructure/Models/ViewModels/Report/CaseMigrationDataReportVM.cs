using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    /// <summary>
    /// Данни за движение за да се сетне източник на постъпване в зависимост от движението
    /// </summary>
    public class CaseMigrationDataReportVM
    {
        public int MigrationId { get; set; }
        public int CaseMigrationTypeId { get; set; }
        public int ReturnCaseId { get; set; }
    }
}
