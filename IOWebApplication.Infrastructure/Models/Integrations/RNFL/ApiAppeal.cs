using System;

namespace IOWebApplication.Infrastructure.Models.Integrations.RNFL
{
    /// <summary>
    /// Данни за жалба
    /// </summary>
    public class ApiAppeal
    {
        /// <summary>
        /// Идентификатор на акт
        /// </summary>
        public Guid? Gid { get; set; }

        /// <summary>
        /// Идентификатор на партида
        /// </summary>
        public Guid CaseGid { get; set; }

        /// <summary>
        /// Идентификатор на акт
        /// </summary>
        public Guid ActGid { get; set; }

        /// <summary>
        /// Тип жалба, REQ|NOM
        /// </summary>
        public string AppealTypeCode { get; set; }

        /// <summary>
        /// Дата на жалба, REQ
        /// </summary>
        public DateTime AppealDate { get; set; }

        /// <summary>
        /// Подател, REQ
        /// </summary>
        public string SenderName { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Код на съд въззивно/касационно дело, NOM
        /// </summary>
        public string AppealCourtCode { get; set; }

        /// <summary>
        /// Номер на въззивно/касационно дело
        /// </summary>
        public int? AppealCaseNumber { get; set; }

        /// <summary>
        /// Година на въззивно/касационно дело
        /// </summary>
        public int? AppealCaseYear { get; set; }

        /// <summary>
        /// Дата на акт
        /// </summary>
        public DateTime? AppealActDate { get; set; }
    }
}
