using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Eispp
{
    public class EisppEventGSVM
    {
        /// <summary>
        /// ид дело
        /// </summary>
        public int CaseId { get; set; }

        /// <summary>
        /// ЕИСПП Събитие
        /// </summary>
        [Display(Name = "Събитие")]
        public int EventType { get; set; }

        /// <summary>
        /// Вид дело
        /// </summary>
        [Display(Name = "Вид дело")]
        public string CaseType { get; set; }

        /// <summary>
        /// година на делото
        /// </summary>
        [Display(Name = "Година")]
        public int Year { get; set; }

        /// <summary>
        /// кратък номер, а не 14-цифрен  номер
        /// </summary>
        [Display(Name = "Номер")]
        public int ShortNumber { get; set; }

        /// <summary>
        /// Акт/протокол
        /// </summary>
        [Display(Name = "Акт/Протокол")]
        public int? CaseSessionActId { get; set; }

        /// <summary>
        /// Свързано дело което се получава
        /// </summary>
        [Display(Name = "Свързано дело")]
        public string ConnectedCaseId { get; set; }

        /// <summary>
        /// Причина за изпращане на НД
        /// </summary>
        [Display(Name = "Причина за изпращане на НД")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете причина")]
        public int? ReasonId { get; set; }


        /// <summary>
        /// точен вид дело
        /// системен код на елемент от nmk_dlosig
        /// </summary>
        [Display(Name = "Точен вид дело")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете Точен вид дело")]
        public int? ExactCaseType { get; set; }


        [Display(Name = "ЕИСПП номер на НП")]
        public string EISPPNumber { get; set; }

        /// <summary>
        /// Свързано дело което се получава
        /// </summary>
        [Display(Name = "Движение на дело")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете движение")]
        public int? CaseMigrationId { get; set; }


        /// <summary>
        /// Дали редиректа е от делото или от съд
        /// </summary>
        public string Mode { get; set; }
        public bool IsEdit { get; set; }
        public bool IsChange { get; set; }
        public bool IsOld { get; set; }
        [AllowHtml]
        public string ModelJson { get; set; }
        public bool IsForSend { get; set; }
        public bool IsValidated { get; set; }
        public long? MQEpepId { get; set; }
        public int? EventFromId { get; set; }
        public int? EventId { get; set; }
    }
}