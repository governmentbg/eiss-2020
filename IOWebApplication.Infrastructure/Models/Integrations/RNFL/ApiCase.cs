using System;

namespace IOWebApplication.Infrastructure.Models.Integrations.RNFL
{
    /// <summary>
    /// Данни за дело/партида
    /// </summary>
    public class ApiCase
    {
        /// <summary>
        /// Идентификатор на производство
        /// </summary>
        public Guid? Gid { get; set; }

        /// <summary>
        /// Код на съд, REQ|NOM
        /// </summary>
        public string CourtCode { get; set; }

        /// <summary>
        /// Номер на дело, REQ
        /// </summary>
        public int CaseNumber { get; set; }

        /// <summary>
        /// Година на дело, REQ
        /// </summary>
        public int CaseYear { get; set; }

        /// <summary>
        /// Тип документ, REQ|NOM
        /// </summary>
        public string DocumentTypeCode { get; set; }

        /// <summary>
        /// Номер на молба, REQ
        /// </summary>
        public string DocumentNumber { get; set; }

        /// <summary>
        /// Дата на регистриране на молба, REQ
        /// </summary>
        public DateTime DocumentDate { get; set; }

        /// <summary>
        /// Вид производство, REQ|NOM
        /// </summary>
        public string ProcessTypeCode { get; set; }
    }
}
