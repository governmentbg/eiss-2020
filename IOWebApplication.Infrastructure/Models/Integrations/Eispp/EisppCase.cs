using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// DLO
    /// Дело за ЕИСПП пакет
    /// За събития, различни от 913 „Получаване на дело“, 
    /// в елемент DLO се подават данни за делото, по което е съответното събитие.
    /// За събитие 913 "Получаване на дело” в обект DLO се подават данни за предходно дело, 
    /// получено от досъдебна институция/съдебна инстанция и регистрирано като „Свързано дело“. 
    /// </summary>
    public class EisppCase : EisppBaseCase
    {
        /// <summary>
        /// DLOSTA
        /// Статус на дело
        /// </summary>
        [XmlElement("DLOSTA")]
        public EisppCaseStatus Status { get; set; }

        /// <summary>
        /// FZL
        /// Физическо лице
        /// </summary>
        [XmlElement("FZL")]
        public EisppPerson[] Persons { get; set; }

        /// <summary>
        /// NFL
        /// Юридическо лице
        /// </summary>
        [XmlElement("NFL")]
        public LegalEntity[] LegalEntities { get; set; }

        /// <summary>
        /// PNE
        /// Престъпление
        /// </summary>
        [XmlElement("PNE")]
        public Crime[] Crimes { get; set; }

        /// <summary>
        /// NPRFZLPNE
        /// Връзка между Наказателно производство, лице и престъпление
        /// </summary>
        [XmlElement("NPRFZLPNE")]
        public CPPersonCrime[] CPPersonCrimes { get; set; }

        /// <summary>
        /// DLOOSN
        /// Свързани дела (може би)
        /// </summary>
        [XmlElement("DLOOSN")]
        public EisppBaseCase[] ConnectedCases { get; set; }

        [XmlIgnore]
        public bool IsGeneratedEisppNumber { get; set; }

        public static string GetRulesPath()
        {
            return CriminalProceeding.GetRulesPath() + "DLO.";
        }

    }
}