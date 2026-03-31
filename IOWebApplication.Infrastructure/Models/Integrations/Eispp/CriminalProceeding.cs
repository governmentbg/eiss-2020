using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// NPR
    /// Наказателно производство
    /// </summary>
    public class CriminalProceeding
    {
        /// <summary>
        /// nprnmr
        /// ЕИСПП номер на НП, за което е събитието
        /// </summary>
        [XmlAttribute("nprnmr")]
        [Display(Name= "ЕИСПП номер на НП")]
        public string EisppNumber { get; set; }

        /// <summary>
        /// nprsid
        /// Системен идентификатор на НП
        /// </summary>
        [XmlAttribute("nprsid")]
        public int Id { get; set; }

        /// <summary>
        /// nprstr
        /// Структура, образувала НП
        /// </summary>
        [XmlAttribute("nprstr")]
        public int StructureId { get; set; }

        /// <summary>
        /// nprdta
        /// Дата на образуване/отказ на НП
        /// </summary>
        [XmlAttribute("nprdta")]
        public DateTime DateCreate { get; set; }

        /// <summary>
        /// nprdrj
        /// Държава, в която се провежда НП
        /// </summary>
        [XmlAttribute("nprdrj")]
        public int CountryId { get; set; }

    
        /// <summary>
        /// DLO
        /// Дело за ЕИСПП пакет
        /// </summary>
        [XmlElement("DLO")]
        public EisppCase Case { get; set; }

        public static string GetRulesPath()
        {
            return "NPR.";
        }
    }
}
