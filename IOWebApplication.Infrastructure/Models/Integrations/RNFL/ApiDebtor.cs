using System;

namespace IOWebApplication.Infrastructure.Models.Integrations.RNFL
{
    /// <summary>
    /// Данни за длъжник
    /// </summary>
    public class ApiDebtor
    {
        /// <summary>
        /// Идентификатор на лице
        /// </summary>
        public Guid? Gid { get; set; }

        /// <summary>
        /// Идентификатор на партида
        /// </summary>
        public Guid CaseGid { get; set; }

        /// <summary>
        /// Код на тип идентификатор, REQ|NOM, (ЕГН,ЛНЧ)
        /// </summary>
        public string UicTypeCode { get; set; }

        /// <summary>
        /// Идентификатор, REQ
        /// </summary>
        public string Uic { get; set; }

        /// <summary>
        /// Собствено име, REQ
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Бащино име
        /// </summary>
        public string MiddleName { get; set; }

        /// <summary>
        /// Фамилно име, REQ
        /// </summary>
        public string FamilyName { get; set; }

        /// <summary>
        /// Второ фамилно име
        /// </summary>
        public string Family2Name { get; set; }

        /// <summary>
        /// Пълно име
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Дата на раждане, за чужденци
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// Място на раждане, за чужденци
        /// </summary>
        public string BirthPlace { get; set; }

        /// <summary>
        /// Настоящ адрес
        /// </summary>
        public ApiAddress Address { get; set; }

    }
}
