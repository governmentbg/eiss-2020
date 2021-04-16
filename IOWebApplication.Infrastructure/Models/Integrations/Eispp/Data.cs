using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// Данни в пакета за ЕИСПП
    /// </summary>
    public class Data
    {
        /// <summary>
        /// Създава обект за десериализация
        /// </summary>
        public Data()
        {

        }

        /// <summary>
        /// Създава обект за регистриране на събитие
        /// </summary>
        /// <param name="ev"></param>
        public Data(Event ev)
        {
            Events = new Event[] { ev };
        }

        /// <summary>
        /// Създава обект за корекция на събитие
        /// </summary>
        /// <param name="oldEv"></param>
        /// <param name="newEv"></param>
        public Data(Event oldEv, Event newEv)
        {
            Events = new Event[] { oldEv, newEv };
        }

        /// <summary>
        /// KST
        /// Контекст на заявката
        /// </summary>
        [XmlElement("KST")]
        public Context Context { get; set; }

        /// <summary>
        /// данните за регистриране на едно
        /// събитие, когато е корекция се подават две събития, едното с
        /// elementType="sbezvk", а другото elementType="sbereg"
        /// </summary>
        [XmlArray("VHD")]
        [XmlArrayItem("SBE", Type = typeof(Event))]
        public Event[] Events { get; set; }

        /// <summary>
        /// RZT
        /// Резултат от обработка на пакет
        /// </summary>
        [XmlElement("RZT")]
        public EisppResult Result { get; set; }

        ///// <summary>
        ///// SNOSAO
        ///// Сигнални съобщения
        ///// </summary>
        //[XmlElement("SNOSAO")]
        //public SignalMessage SignalMessage { get; set; }

        //<xs:attribute name="adrstr" />
        //<xs:attribute name="adrnprnmr" />
        //<xs:attribute name="adrdlovid" />
        //<xs:attribute name="adrdlosig" />
        //<xs:attribute name="adrdlogdn" />
        //<xs:attribute name="adrdlonmr" />
    }
}
