using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    /// <summary>
    /// Модел за визуализация на данни за справка съдебни актове
    /// </summary>
    public class CaseSessionActReportVM
    {
        /// <summary>
        /// Идентификатор на записа
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на делото
        /// </summary>
        public int CaseId { get; set; }

        /// <summary>
        /// Номер/Година на съдебен акт
        /// </summary>
        public string ActRegNumYear { get; set; }

        /// <summary>
        /// Вид съдебен акт
        /// </summary>
        public string ActTypeLabel { get; set; }

        /// <summary>
        /// Дата на акта
        /// </summary>
        public DateTime? RegDate { get; set; }

        /// <summary>
        /// Дата на връщане
        /// </summary>
        public DateTime? ReturnDate { get; set; }

        /// <summary>
        /// Дата на влизане в законна сила
        /// </summary>
        public DateTime? ActInforcedDate { get; set; }

        /// <summary>
        /// Вид дело/документ
        /// </summary>
        public string CaseActInfoLabel { get; set; }

        /// <summary>
        /// Информация за документ
        /// </summary>
        public string DocumentInfo { get; set; }
        public DateTime? DocumentDate { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        public string ActStateName { get; set; }

        /// <summary>
        /// Съдия-докладчик
        /// </summary>
        public string JudgeReport { get; set; }

        /// <summary>
        /// Резултат/степен на уважаване на иска
        /// </summary>
        public string ActComplainResultLabel { get; set; }

        /// <summary>
        /// Дата на обявяване за решаване - датата на заседанието ако има резултат Обявено за решаване
        /// </summary>
        public DateTime? DateAnnouncementDecision { get; set; }

        /// <summary>
        /// Точен вид дело
        /// </summary>
        public string CaseTypeLabel { get; set; }

        /// <summary>
        /// Номер на дело
        /// </summary>
        public string CaseNumber { get; set; }
    }
}
