using IOWebApplication.Infrastructure.Constants;
using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// SBE
    /// Събитие
    /// </summary>
    public class Event
    {
        /// <summary>
        /// sbevid
        /// вид на събитието
        /// системен код на елемент от nmk_sbevid
        /// </summary>
        [XmlAttribute("sbevid")]
        [Display(Name = "Вид на събитието")]
        public int EventType { get; set; }

        /// <summary>
        /// sbedkpdta
        /// дата на събитието
        /// когато е във връзка със съдебен акт, дата на съдебния акт
        /// </summary>
        [XmlAttribute("sbedkpdta", DataType = "date")]
        [Display(Name = "Дата на събитието")]
        public DateTime EventDate { get; set; }

        /// <summary>
        /// sbedkpstr
        /// структура, създала събитието
        /// елемент от номенклатурата в Стандарт 7
        /// съответстващ на текущия съд
        /// </summary>
        [XmlAttribute("sbedkpstr")]
        public int StructureId { get; set; }

        /// <summary>
        /// elementType
        /// за ново събитие – стойност "sbereg"
        /// за корекция или изтриване – стойност "sbezvk"
        /// </summary>
        [XmlAttribute("elementType")]
        [Display(Name = "Вид събитие")]
        public string EventKind { get; set; } = EISPPConstants.EventKind.NewEvent;

        /// <summary>
        /// sbesid
        /// Системен идентификатор на събитие 
        /// </summary>
        [XmlAttribute("sbesid")]
        public int EventId { get; set; }


        /// <summary>
        /// sbedkpvid
        /// Вид документ номенклатура 224 или 11993
        /// </summary>
        [Display(Name = "Вид документ")]
        [XmlAttribute("sbedkpvid")]
        public int DocumentType { get; set; }


        /// <summary>
        /// DVJDLO
        /// Движение на дело
        /// </summary>
        [XmlElement("DVJDLO")]
        public CaseMigration CaseMigration { get; set; }

        /// <summary>
        /// NPR
        /// Наказателно производство
        /// </summary>
        [XmlElement("NPR")]
        public CriminalProceeding CriminalProceeding { get; set; }

        /// <summary>
        /// DOCREF
        /// Свързани документи
        /// </summary>
        [XmlElement("DOCREF")]
        public DocumentRefference DocRef { get; set; }

        /// <summary>
        /// Срок за обжалване
        /// Свързани документи
        /// </summary>
        [XmlElement("SRK")]
        public EisppSrok EisppSrok { get; set; }

        /// <summary>
        /// SBH
        /// Характеристика на събитие
        /// </summary>
        [XmlElement("SBH")]
        public EventFeature EventFeature { get; set; }

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

        [XmlIgnore]
        public int Index { get; set; } = 0;
        [XmlIgnore]
        public int CaseId { get; set; } = 0;
        [XmlIgnore]
        public int? CasePersonId { get; set; }
        [XmlIgnore]
        public string ConnectedCaseId { get; set; }
        /// <summary>
        /// Акт/протокол
        /// </summary>
        [Display(Name = "Акт/Протокол")]
        [XmlIgnore]
        public int? CaseSessionActId { get; set; }
        [XmlIgnore]
        public int? CaseComplaintId { get; set; }
        /// <summary>
        /// Отменена мяркa
        /// </summary>
        [Display(Name = "Отменена мяркa за процесуална принуда")]
        [XmlIgnore]
        public int? PersonOldMeasureId { get; set; }

        /// <summary>
        /// Наложена мяркa
        /// </summary>
        [Display(Name = "Наложена мяркa за процесуална принуда")]
        [XmlIgnore]
        public int? PersonMeasureId { get; set; }
    }
}

