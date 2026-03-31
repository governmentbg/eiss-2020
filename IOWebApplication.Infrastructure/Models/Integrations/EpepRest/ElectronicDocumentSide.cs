using System;

namespace IOWebApplication.Infrastructure.Models.Integrations.EpepRest
{
    /// <summary>
    /// Описва ред от обекта ElectronicDocumentSide, Страна към документ
    /// </summary>
    public class ElectronicDocumentSide
    {
        /// <summary>
        /// Идентификатор на страна електронно подаден документ
        /// Полето е задължително
        /// </summary>

        public Guid ElectronicDocumentSideId { get; set; }

        /// <summary>
        /// Идентификатор на роля лице
        /// Полето е задължително
        /// </summary>

        public string SideInvolvementKind { get; set; }

        /// <summary>
        /// Гражданство, 2 буквен код на държава 
        /// </summary>
        public string CitizenshipCode { get; set; }

        /// <summary>
        /// Физическо лице
        /// Полето не е задължително, едно от двете полета Person или Entity трябва да е попълнено
        /// </summary>

        public Person Person { get; set; }

        /// <summary>
        /// Юридическо лице
        /// Полето не е задължително, едно от двете полета Person или Entity трябва да е попълнено
        /// </summary>

        public Entity Entity { get; set; }

        /// <summary>
        /// Идентификатор на представлявана страна
        /// Полето не е задължително
        /// </summary>

        public Guid? RepresentsSideId { get; set; }

        /// <summary>
        /// Списък от адреси
        /// </summary>
        public ElectronicDocumentSideAddress[] Addresses { get; set; }
    }
}