using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    /// <summary>
    /// Модел за визуализация на данни за регистрирани документи
    /// </summary>
    public class DocumentListVM
    {
        /// <summary>
        /// Идентификатор на документа
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Направление на документа
        /// </summary>
        public string DocumentDirectionName { get; set; }

        /// <summary>
        /// Вид документ
        /// </summary>
        public string DocumentTypeName { get; set; }

        public string DocumentRequestName { get; set; }

        /// <summary>
        /// Шифър на дело към централна регистратура
        /// </summary>
        public string CaseCodeName { get; set; }

        /// <summary>
        /// Номер на документ
        /// </summary>
        public string DocumentNumber { get; set; }

        /// <summary>
        /// Дата на документа
        /// </summary>
        public DateTime DocumentDate { get; set; }

        /// <summary>
        /// Регистрирал
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Съд на регистриране
        /// </summary>
        public string CreatedCourtName { get; set; }

        /// <summary>
        /// Свързани лица
        /// </summary>
        public DocumentListPersonVM[] Persons { get; set; }

        /// <summary>
        /// Номер на дело
        /// </summary>
        public string CaseNumber { get; set; }

        /// <summary>
        /// Идентификатор на дело
        /// </summary>
        public int? CaseId { get; set; }

        /// <summary>
        /// Флаг за необразувани дела, които се администрират
        /// </summary>
        public bool? IsCaseRejected { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        public string RejectedStateName { get; set; }

        /// <summary>
        /// Номер на документ
        /// </summary>
        public int DocumentNumberValue { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }
        public DocumentCaseLinkVM[] RegCases { get; set; }
    }

    public class DocumentCaseLinkVM
    {
        public int CaseId { get; set; }
        public string CaseCourt { get; set; }
        public string CaseNumber { get; set; }
    }
}
