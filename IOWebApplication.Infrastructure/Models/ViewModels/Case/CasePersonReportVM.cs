using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    /// <summary>
    /// Модел за визуализация на данни за лица
    /// </summary>
    public class CasePersonReportVM
    {
        /// <summary>
        /// Идентификатор на запис на дело
        /// </summary>
        public int CaseId { get; set; }

        /// <summary>
        /// Номер на дело
        /// </summary>
        public string CaseNumber { get; set; }

        /// <summary>
        /// Дата на дело
        /// </summary>
        public DateTime CaseDate { get; set; }

        /// <summary>
        /// Идентификатор на лице
        /// </summary>
        public string Uic { get; set; }

        /// <summary>
        /// Име на лице
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Вид лице
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// Статус на дело
        /// </summary>
        public string CaseStateLabel { get; set; }

        /// <summary>
        /// Точен вид дело
        /// </summary>
        public string CaseTypeLabel { get; set; }

        /// <summary>
        /// Шифър на дело
        /// </summary>
        public string CaseCodeLabel { get; set; }

        /// <summary>
        /// Съдия-докладчик
        /// </summary>
        public string JudgeReport { get; set; }
    }
}
