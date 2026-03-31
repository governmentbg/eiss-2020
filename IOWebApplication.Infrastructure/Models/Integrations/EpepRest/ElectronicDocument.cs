using System;
using System.Runtime.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.EpepRest
{
    /// <summary>
    /// Описва ред от обекта ElectronicDocument, използван в уеб услугата Подадени електронни документи
    /// </summary>
    public class ElectronicDocument
    {
        /// <summary>
        /// Идентификатор на електронно подаден документ
        /// Полето е задължително
        /// </summary>

        public Guid ElectronicDocumentId { get; set; }

        /// <summary>
        /// Идентификатор на свързано дело, 
        /// Полето не е задължително
        /// </summary>

        public Guid? CaseId { get; set; }

        /// <summary>
        /// Идентификатор на лице по свързаното дело, 
        /// Полето не е задължително
        /// </summary>

        public Guid? SideId { get; set; }

        /// <summary>
        /// Идентификатор на лице, подало документа
        /// Полето е задължително
        /// </summary>

        public Guid UserRegistrationId { get; set; }

        /// <summary>
        /// Идентификатор на съд
        /// Полето е задължително
        /// </summary>

        public string CourtCode { get; set; }

        /// <summary>
        /// Идентификатор на съд, в който е платено преди разпределение
        /// Полето не е задължително
        /// </summary>
        public string PaidInCourtCode { get; set; }

        /// <summary>
        /// Идентификатор на вид документ
        /// Полето е задължително
        /// </summary>

        public string DocumentKind { get; set; }

        /// <summary>
        /// Идентификатор на основен тип документ
        /// Полето е задължително
        /// </summary>

        public string DocumentType { get; set; }

        /// <summary>
        /// Идентификатор на точен вид документ
        /// Полето е задължително
        /// </summary>

        public string DocumentRequestType { get; set; }

        /// <summary>
        /// Идентификатор на тарифа
        /// Полето е задължително
        /// </summary>

        public string PricelistCode { get; set; }


        /// <summary>
        /// Код на валута: BGN,EUR
        /// Полето не е задължително
        /// </summary>

        public string CurrencyCode { get; set; }

        /// <summary>
        /// Материален интерес, стойност в стотинки
        /// Полето не е задължително
        /// </summary>

        public long? BaseAmount { get; set; }

        /// <summary>
        /// Изчислена такса, стойност в стотинки
        /// Полето не е задължително
        /// </summary>

        public int? TaxAmount { get; set; }

        /// <summary>
        /// Материален интерес В лева, стойност в стотинки
        /// Полето не е задължително
        /// </summary>

        public long? BaseAmountBGN { get; set; }

        /// <summary>
        /// Изчислена такса В лева, стойност в стотинки
        /// Полето не е задължително
        /// </summary>

        public int? TaxAmountBGN { get; set; }

        /// <summary>
        /// Описание
        /// Полето не е задължително
        /// </summary>

        public string Description { get; set; }

        /// <summary>
        /// Номер на електронно подаден документ
        /// Полето е задължително
        /// </summary>

        public string NumberApply { get; set; }

        /// <summary>
        /// Дата на електронно подаден документ
        /// Полето е задължително
        /// </summary>

        public DateTime DateApply { get; set; }

        /// <summary>
        /// Дата на електронно плащане
        /// Полето е задължително
        /// </summary>

        public DateTime? DatePaid { get; set; }

        /// <summary>
        /// 1-Виртуален ПОС(null!!), 2-Банков път
        /// </summary>
        public int? PaymentKind { get; set; }


        /// <summary>
        /// Флаг за подаден документ през API
        /// </summary>
        public bool FromApi { get; set; }

        /// <summary>
        /// Имената на лицето, с API ключа на което е подаден документа
        /// </summary>
        public string CreateUserName { get; set; }

        /// <summary>
        /// Списък на страни по документ
        /// </summary>

        public ElectronicDocumentSide[] Sides { get; set; }

        /// <summary>
        /// Списък на Файлове по документ
        /// </summary>

        public ElectronicDocumentFile[] Files { get; set; }
    }
}