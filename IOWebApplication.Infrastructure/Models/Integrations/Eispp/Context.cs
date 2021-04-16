using System;
using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// Контекст на пакета
    /// </summary>
    public class Context
    {
        /// <summary>
        /// armStrSid
        /// резервирано за вътрешно използване в ЕИСПП
        /// попълва се автоматично
        /// </summary>
        [XmlAttribute("armStrSid")]
        public string ArmStrSid { get => "0"; }

        /// <summary>
        /// sesSid
        /// системен идентификатор на он-лайн сесията
        /// попълва се автоматично
        /// </summary>
        [XmlAttribute("sesSid")]
        public string SessionId { get => "0";  }

        /// <summary>
        /// usrRab
        /// код по ЕИСПП на съда, който изпраща пакета
        /// </summary>
        [XmlAttribute("usrRab")]
        public int StructureId { get; set; }

        /// <summary>
        /// usrRej
        /// режим на работа на регистрирания потребител
        /// Да се сложи в конфигурационен файл
        /// попълва се автоматично
        /// </summary>
        [XmlAttribute("usrRej")]
        public int WorkingMode { get; set; }

        /// <summary>
        /// usrSid
        /// системен идентификатор на потребител 
        /// Да се сложи в конфигурационен файл
        /// попълва се автоматично
        /// </summary>
        [XmlAttribute("usrSid")]
        public int UserSystemId { get; set; }

        /// <summary>
        /// armSid
        /// идентификатор на работно място
        /// попълва се автоматично
        /// </summary>
        [XmlAttribute("armSid")]
        public int WorkPlaceId { get; set; }

        /// <summary>
        /// resSid
        /// системен идентификатор на ресурса
        /// </summary>
        [XmlAttribute("resSid")]
        public int ResourceId { get; set; }

        /// <summary>
        /// lgtSid
        /// системен идентификатор на транзакцията в ЕИСПП
        /// попълва се автоматично
        /// </summary>
        [XmlAttribute("lgtSid")]
        public int TransactionId { get => 0; }

        /// <summary>
        /// sbeVid
        /// вид на събитието, което ще се регистрира, 
        /// като в случаите извън регистрация, 
        /// корекция и изтриване на събитие се подава стойност 0 (нула)
        /// </summary>
        [XmlAttribute("sbeVid")]
        public int EventType { get; set; }

        /// <summary>
        /// sbeDta
        /// дата на събитието
        /// </summary>
        [XmlAttribute("sbeDta", DataType = "date")]
        public DateTime EventDate { get; set; }

        /// <summary>
        /// okaSid
        /// вид на обработката
        /// попълва се автоматично
        /// </summary>
        [XmlAttribute("okaSid")]
        public int ProcessType { get => 0;  }

        /// <summary>
        /// Дата, която трябва да се игнорира
        /// </summary>
        private static readonly DateTime defaultDate = default;

        /// <summary>
        /// Игнорира празна дата
        /// </summary>
        /// <returns></returns>
        public bool ShouldSerializeEventDate()
        {
            return EventDate != defaultDate;
        }
    }
}
