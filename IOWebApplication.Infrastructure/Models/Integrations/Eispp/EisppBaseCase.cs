using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// DLOOSN
    /// Основна информация за дело
    /// </summary>
    public class EisppBaseCase
    {
        /// <summary>
        /// dlovid
        /// вид дело
        /// за събития, различни от 913 „Получаване на дело“ – стойност 853
        /// </summary>
        [XmlAttribute("dlovid")]
        [Display(Name="Вид дело")]
        public int CaseType { get; set; } = 853;

        /// <summary>
        /// dlogdn
        /// година на делото
        /// </summary>
        [XmlAttribute("dlogdn")]
        [Display(Name = "Година")]
        public int Year { get; set; }

        /// <summary>
        /// dlonmr
        /// кратък номер, а не 14-цифрен  номер
        /// </summary>
        [XmlAttribute("dlonmr")]
        [Display(Name = "Номер")]
        public string ShortNumber { get; set; }

        /// <summary>
        /// dlosig
        /// точен вид дело
        /// системен код на елемент от nmk_dlosig
        /// Задължително за всички събития с изключение на събитие 913 „Получаване на дело“
        /// </summary>
        [XmlAttribute("dlosig")]
        [Display(Name = "Точен вид дело")]
        public int ExactCaseType { get; set; }

        [XmlIgnore]
        [Display(Name = "Точен вид дело")]
        public int CaseTypeId { get; set; }

        [XmlIgnore]
        [Display(Name = "Характер на дело")]
        /// <summary>
        /// Характер на дело
        /// за стари дела в основание
        /// </summary>
        public int CaseCharacterId { get; set; }

        /// <summary>
        /// dlosig
        /// шифър дело
        /// да се показва при изпращане и филтрира събития и сетва LegalProceedingType
        /// </summary>
        [XmlIgnore]
        [Display(Name = "Шифър на дело")]
        public int CaseCodeId { get; set; }
        /// <summary>
        /// dlostr
        /// структура, на която е делото
        /// За събития, различни от 913 „Получаване на дело“ – 
        /// системен код на елемента от nmk_strvid, 
        /// съответстващ на текущия съд
        /// Задължително за всички събития с изключение на събитие 913 „Получаване на дело“
        /// </summary>
        [XmlAttribute("dlostr")]
        public int StructureId { get; set; }

        /// <summary>
        /// dloncnone
        /// начин на образуване на дело
        /// Системен код на елемент от nmk_dloncnone
        /// Задължително за всички събития с изключение на събитие 913 „Получаване на дело“
        /// </summary>
        [XmlAttribute("dloncnone")]
        [Display(Name = "Начин на образуване")]
        public int CaseSetupType { get; set; }

        /// <summary>
        /// dlosprvid
        /// вид съдебно производство
        /// системен код на елемент от nmk_dlosprvid
        /// Задължително за всички събития с изключение на събитие 913 „Получаване на дело“
        /// </summary>
        [XmlAttribute("dlosprvid")]
        [Display(Name = "Вид съдебно производство")]
        public int LegalProceedingType { get; set; }

        /// <summary>
        /// dlopmt
        /// Основание за възлагане на дело
        /// Не е задължително
        /// Номенклатура nmk_osnvzg
        /// </summary>
        [XmlAttribute("dlopmt")]
        [Display(Name = "Основание за възлагане на дело")]
        public int CaseReason { get; set; }

        /// <summary>
        /// dlosid
        /// Системен идентификатор на дело
        /// </summary>
        [XmlAttribute("dlosid")]
        public int EisppCaseId { get; set; }

        [XmlIgnore]
        [Display(Name = "Вид дело основание")]
        public string InstitutionCaseTypeName { get; set; }
        
        [XmlIgnore]
        [Display(Name = "Дело от")]
        public string InstitutionName { get; set; }

        [XmlIgnore]
        [Display(Name = "Вид институция")]
        public string InstitutionTypeName { get; set; }
        
        [XmlIgnore]
        public string CaseTypeName { get; set; }

        public bool ShouldSerializeCaseSetupType()
        {
            return CaseSetupType >= 0;
        }
        public bool ShouldSerializeLegalProceedingType()
        {
            return LegalProceedingType >= 0;
        }

        [XmlIgnore]
        [Display(Name = "Свързано дело")]
        public string ConnectedCaseId { get; set; }
        
        [XmlIgnore]
        [Display(Name = "Свързано дело")]
        public bool IsSelected { get; set; }
    }
}
