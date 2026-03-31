using System;

namespace IOWebApplication.Infrastructure.Models.Integrations.RNFL
{
    /// <summary>
    /// Данни за призовка
    /// </summary>
    public class ApiSummon
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
        /// Вид призовка, REQ|NOM
        /// </summary>
        public string SummonTypeCode { get; set; }

        /// <summary>
        /// Дата на призовката, REQ
        /// </summary>
        public DateTime SummonDate { get; set; }

        /// <summary>
        /// Адресат, REQ
        /// </summary>
        public string Addressee { get; set; }

        /// <summary>
        /// Описание на призовката
        /// </summary>
        public string Description { get; set; }       
    }
}
