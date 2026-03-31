using System;

namespace IOWebApplication.Infrastructure.Models.Integrations.RNFL
{
    /// <summary>
    /// Данни за акт
    /// </summary>
    public class ApiAct
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
        /// Код на съд, REQ|NOM
        /// </summary>
        public string CourtCode { get; set; }

        /// <summary>
        /// Тип акт, REQ|NOM
        /// </summary>
        public string ActTypeCode { get; set; }

        /// <summary>
        /// Правно основание, REQ|NOM
        /// </summary>
        public string LegalBaseCode { get; set; }

        /// <summary>
        /// Номер на акт, REQ
        /// </summary>
        public string ActNumber { get; set; }

        /// <summary>
        /// Дата на акт, REQ
        /// </summary>
        public DateTime ActDate { get; set; }

        /// <summary>
        /// Дата на на влизане в сила
        /// </summary>
        public DateTime? EnforceDate { get; set; }

        /// <summary>
        /// Незабавно изпълнение на акта
        /// </summary>
        public bool EffectiveImmediately { get; set; }

        /// <summary>
        /// Код на Компетентен съд за обжалване, NOM
        /// </summary>
        public string ApealCourtCode { get; set; }       
    }
}
