using System;
using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// Базов клас, съдържаща комуникационни параметри за ЕИСПП
    /// </summary>
    public abstract class EisppProperties
    {
        /// <summary>
        /// vzlizp
        /// Възел изпращач (VZL_EISPPEISS)
        /// попълва се автоматично
        /// </summary>
        [XmlIgnore]
        public string SenderNode { get; set; }

        /// <summary>
        /// vzlplc
        /// Възел получател (VZL_EISPP)
        /// попълва се автоматично
        /// </summary>
        [XmlIgnore]
        public string ReceiverNode { get; set; }

        /// <summary>
        /// vid_saobstenie
        /// вид на пакета според начина на взаимодействие (134)
        /// попълва се автоматично
        /// </summary>
        [XmlIgnore]
        public int MessageType { get; set; }

        /// <summary>
        /// structura_izp
        /// структура, която изпраща пакета
        /// елемент от номенклатурата в Стандарт 7 
        /// </summary>
        [XmlIgnore]
        public int SenderStructure { get; set; }

        /// <summary>
        /// structura_plc
        /// структура, която получава пакета (9191)
        /// попълва се автоматично
        /// </summary>
        [XmlIgnore]
        public int ReceiverStructure { get; set; }

        /// <summary>
        /// correlation_id
        /// Идентификатор на съобщение
        /// попълва се автоматично
        /// </summary>
        [XmlIgnore]
        public string CorrelationId { get; set; }

        /// <summary>
        /// Properties
        /// Параметри на пакета
        /// попълват се автоматично
        /// </summary>
        [XmlArray("Properties")]
        [XmlArrayItem("Property", Type = typeof(Property))]
        public Property[] Properties
        {
            get
            {
                return new Property[]
                {
                    new Property("vzlizp", SenderNode),
                    new Property("vzlplc", ReceiverNode),
                    new Property("vid_saobstenie", MessageType.ToString()),
                    new Property("structura_izp", SenderStructure.ToString()),
                    new Property("structura_plc", ReceiverStructure.ToString()),
                    new Property("correlation_id", CorrelationId)
                };
            }
            set
            {
                foreach (var item in value)
                {
                    SetProperty(item);
                }
            }
        }

        private void SetProperty(Property property)
        {
            try
            {
                switch (property.Name)
                {
                    case "vzlizp":
                        SenderNode = property.Value;
                        break;
                    case "vzlplc":
                        ReceiverNode = property.Value;
                        break;
                    case "vid_saobstenie":
                        MessageType = int.Parse(property.Value);
                        break;
                    case "structura_izp":
                        SenderStructure = int.Parse(property.Value);
                        break;
                    case "structura_plc":
                        ReceiverStructure = int.Parse(property.Value);
                        break;
                    case "correlation_id":
                        CorrelationId = property.Value;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
                //log error;
            }
        }
    }
}
