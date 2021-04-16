using System;
using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// IZPNKZ
    /// Изпълнение на наказание
    /// </summary>
    public class PunishmentExecution
    {
        /// <summary>
        /// izpdta
        /// Дата на състояние на изпълнение на наказанието
        /// </summary>
        [XmlAttribute("izpdta", DataType = "date")]
        public DateTime MyProperty { get; set; }

        /// <summary>
        /// izpnkzsid
        /// Системен идентификатор
        /// </summary>
        [XmlAttribute("izpnkzsid")]
        public int PunishmentId { get; set; }

        /// <summary>
        /// izpste
        /// Състояние на изпълнението на наказание
        /// Номенклатура nmk_izpste
        /// задължително
        /// </summary>
        [XmlAttribute("izpste")]
        public int PunishmentStatus { get; set; }

        /// <summary>
        /// izpsteprc
        /// Причина за състояние на изпълнението на наказание
        /// Номенклатура nmk_izpsteprc
        /// задължително
        /// </summary>
        [XmlAttribute("izpsteprc")]
        public int StatusReason { get; set; }

        /// <summary>
        /// izpstr
        /// Структура, в която се изпълнява наказанието
        /// Номенклатура nmk_strvid
        /// </summary>
        [XmlAttribute("izpstr")]
        public int StructureId { get; set; }
    }
}