using System;
using System.Runtime.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.EpepRest
{
    /// <summary>
    /// Описва ред от обекта IncomingDocument, използван в уеб услугата Входящ документ
    /// </summary>

    public class IncomingDocument
    {
        /// <summary>
        /// Идентификатор
        /// Полето не е задължително
        /// </summary>

        public Guid? IncomingDocumentId { get; set; }

        /// <summary>
        /// Идентификатор на дело
        /// Полето е задължително
        /// </summary>

        public Guid? CaseId { get; set; }

        /// <summary>
        /// Физическо лице
        /// Полето не е задължително, едно от двете полета Person или Entity трябва да е попълнено
        /// </summary>

        public Person? Person { get; set; }

        /// <summary>
        /// Юридическо лице
        /// Полето не е задължително, едно от двете полета Person или Entity трябва да е попълнено
        /// </summary>

        public Entity? Entity { get; set; }

        /// <summary>
        /// Код на съд
        /// Полето е задължително
        /// </summary>

        public string CourtCode { get; set; }

        /// <summary>
        /// Входящ номер
        /// Полето не е задължително
        /// </summary>

        public int IncomingNumber { get; set; }

        /// <summary>
        /// Входяща дата
        /// Полето не е задължително
        /// </summary>

        public DateTime IncomingDate { get; set; }

        /// <summary>
        /// Код на типа на входящ документ
        /// Полето не е задължително
        /// </summary>

        public string IncomingDocumentTypeCode { get; set; }

        /// <summary>
        /// Идентификатор на електронно подаден документ, по който е регистриран 
        /// Полето не е задължително
        /// </summary>

        public Guid? ElectronicDocumentId { get; set; }

        /// <summary>
        /// Входящ номер от Централна Регистратура, за Заявления по чл.410/417 от ГПК
        /// Полето не е задължително
        /// </summary>
        public int? AssignedDocumentNumber { get; set; }

        /// <summary>
        /// Година на регистриране в Централна Регистратура, за Заявления по чл.410/417 от ГПК
        /// Полето не е задължително
        /// </summary>
        public int? AssignedDocumentYear { get; set; }

        /// <summary>
        /// Код на съд, в който е въведен документа за Централно разпределяне, за Заявления по чл.410/417 от ГПК
        /// Полето е не задължително
        /// </summary>

        public string AssignedCourtCode { get; set; }
    }
}