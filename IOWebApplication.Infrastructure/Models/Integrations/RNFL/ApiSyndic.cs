using System;

namespace IOWebApplication.Infrastructure.Models.Integrations.RNFL
{
    /// <summary>
    /// Данни за синдик, подава се след партидата и акта за назначаване на синдика
    /// </summary>
    public class ApiSyndic
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
        /// Телефон
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Електронна поща
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///Акт за назначаване
        /// </summary>
        public Guid StartActGid { get; set; }

        /// <summary>
        /// Дата на встъпване на правомощията, REQ
        /// </summary>
        public DateTime DateStart { get; set; }

        /// <summary>
        /// Дата на освобождаване
        /// </summary>
        public DateTime? DateEnd { get; set; }

        /// <summary>
        /// Адрес на синдика
        /// </summary>
        public ApiAddress Address { get; set; }
    }
}
