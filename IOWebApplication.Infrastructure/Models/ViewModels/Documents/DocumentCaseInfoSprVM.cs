using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Documents
{
    /// <summary>
    /// Модел за справка  съпровождащи документи
    /// </summary>
    public class DocumentCaseInfoSprVM
    {
        /// <summary>
        /// Номер и година на документ
        /// </summary>
        public string DocumentNumberYear { get; set; }

        /// <summary>
        /// Дата на документ
        /// </summary>
        public DateTime? DocumentDate { get; set; }

        /// <summary>
        /// Вид документ
        /// </summary>
        public string DocumentTypeLabel { get; set; }

        /// <summary>
        /// Информация за дело
        /// </summary>
        public string CaseInfo { get; set; }

        /// <summary>
        /// Идентификатор на дело
        /// </summary>
        public int? CaseId { get; set; }

        /// <summary>
        /// Флаг дали има дело
        /// </summary>
        public bool? IsCase { get; set; }

        /// <summary>
        /// Информация за иницииращ документ на делото
        /// </summary>
        public string CaseDocumentInfo { get; set; }

        /// <summary>
        /// Шифър на делото
        /// </summary>
        public string CaseCodeLabel { get; set; }

        /// <summary>
        /// Информация за заседание
        /// </summary>
        public string CaseSessionInfo { get; set; }

        /// <summary>
        /// Съдия-докладчик
        /// </summary>
        public string JudgeReport { get; set; }
    }
}
