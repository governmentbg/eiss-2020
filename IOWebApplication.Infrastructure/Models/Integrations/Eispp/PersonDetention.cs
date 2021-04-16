using System;
using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// ZDJ
    /// Задържане на лице
    /// </summary>
    public class PersonDetention
    {
        /// <summary>
        /// zdjsid
        /// Системен идентификатор
        /// </summary>
        [XmlAttribute("zdjsid")]
        public int DetentionId { get; set; }

        /// <summary>
        /// zdjstr
        /// Структура, задържала / освободила лицето
        /// Номенклатура nmk_strvid
        /// </summary>
        [XmlAttribute("zdjstr")]
        public int DetentionStructure { get; set; }

        /// <summary>
        /// zdjtip
        /// Тип задържане / освобождаване
        /// Номенклатура nmk_zdjtip
        /// </summary>
        [XmlAttribute("zdjtip")]
        public int DetentionType { get; set; }

        /// <summary>
        /// zdjvid
        /// Вид задържане на лице
        /// Номенклатура nmk_zdjvid
        /// </summary>
        [XmlAttribute("zdjvid")]
        public int DetentionKind { get; set; }

        /// <summary>
        /// zdjvme
        /// Час и дата на задържане / освобождаване
        /// </summary>
        [XmlAttribute("zdjvme", DataType = "dateTime")]
        public DateTime DetentionTime { get; set; }

        /// <summary>
        /// zdjopi
        /// Описание на задържане
        /// </summary>
        [XmlAttribute("zdjopi")]
        public string DetentionDescription { get; set; }

        /// <summary>
		/// Дата, която трябва да се игнорира
		/// </summary>
		private static readonly DateTime defaultDate = default;

        /// <summary>
        /// Игнорира празна дата
        /// </summary>
        /// <returns></returns>
        public bool ShouldSerializeDetentionTime()
        {
            return DetentionTime != defaultDate;
        }
    }
}