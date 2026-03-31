using System;

namespace IOWebApplication.Infrastructure.Models.Integrations.EpepRest
{
    /// <summary>
    /// Описва ред от обекта Summon, използван в уеб услугата Призовка/съобщение
    /// </summary>

    public class Summon
    {
        /// <summary>
        /// Идентификатор
        /// Полето не е задължително
        /// </summary>

        public Guid? SummonId { get; set; }

        /// <summary>
        /// Идентификатор на връзката (дело, заседание, акт ...)
        /// Полето е задължително
        /// </summary>

        public Guid ParentId { get; set; }

        /// <summary>
        /// Идентификатор на връзката страна
        /// Полето е задължително
        /// </summary>

        public Guid SideId { get; set; }

        /// <summary>
        /// Вид на призовката/съобщението
        /// Полето е задължително
        /// </summary>

        public string SummonKind { get; set; }

        /// <summary>
        /// Тип на призовката/съобщението
        /// Полето е задължително
        /// </summary>

        public string SummonTypeCode { get; set; }

        /// <summary>
        /// Номер на призовката/съобщението
        /// Полето не е задължително
        /// </summary>

        public string Number { get; set; }

        /// <summary>
        /// Дата на създаване
        /// Полето е задължително
        /// </summary>

        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Дата на връчване
        /// Полето не е задължително
        /// </summary>

        public DateTime? DateServed { get; set; }

        /// <summary>
        /// Адресат на призовката/съобщението
        /// Полето е задължително
        /// </summary>

        public string Addressee { get; set; }

        /// <summary>
        /// Адрес на призовката/съобщението
        /// Полето не е задължително
        /// </summary>

        public string Address { get; set; }

        /// <summary>
        /// Предмет на призовката/съобщението
        /// Полето е задължително
        /// </summary>

        public string Subject { get; set; }

        /// <summary>
        /// Идентификатор на входящ документ
        /// Полето не е задължително
        /// </summary>
        [Obsolete("Следва да се използва списъчното поле IncomingDocuments")]
        public Guid? IncommingDocumentId { get; set; }

        /// <summary>
        /// Списък идентификатори на съпровождащи документи към призовката
        /// </summary>
        public Guid[] IncomingDocuments { get; set; }
    }
}