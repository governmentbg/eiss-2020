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
        /// <summary>
        /// Идентификатор на движението
        /// </summary>
        public int MigrationId { get; set; }

        /// <summary>
        /// Тип движение
        /// </summary>
        public int CaseMigrationTypeId { get; set; }

        /// <summary>
        /// Идентификатор на върнатото дело
        /// </summary>
        public int ReturnCaseId { get; set; }
    }
}
