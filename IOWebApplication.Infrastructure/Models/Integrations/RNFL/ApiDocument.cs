using System;

namespace IOWebApplication.Infrastructure.Models.Integrations.RNFL
{
    /// <summary>
    /// Данни за документ
    /// </summary>
    public class ApiDocument
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
        /// Вид документ
        /// </summary>
        public string DocumentTypeCode { get; set; }

        /// <summary>
        /// Номер на документ
        /// </summary>
        public string DocumentNumber { get; set; }

        /// <summary>
        /// Дата на документ
        /// </summary>
        public DateTime DocumentDate { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Име на подаващия документа
        /// </summary>
        public string ApplicantName { get; set; }
    }
}
