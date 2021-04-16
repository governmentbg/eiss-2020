using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// SRK
    /// Данни за срок за обжалване
    /// </summary>
    public class EisppSrok
    {

        /// <summary>
        /// srksid
        /// Идентификатор на срок
        /// </summary>
        [XmlAttribute("srksid")]
        public int SrokId { get; set; }

        /// <summary>
        /// srkden
        /// Срок в дни
        /// </summary>
        [XmlAttribute("srkden")]
        [Range(0, int.MaxValue, ErrorMessage = "Въведете дни в интервала 0-9999")]
        [Display(Name = "Дни")]
        public int Days { get; set; }

        /// <summary>
        /// srksdc
        /// Срок в седмици
        /// </summary>
        [XmlAttribute("srksdc")]
        [Display(Name = "Седмици")]
        [Range(0, int.MaxValue, ErrorMessage = "Въведете седмици в интервала 0-9999")]
        public int Weeks { get; set; }

        /// <summary>
        /// srkmsc
        /// Срок в месеци
        /// </summary>
        [XmlAttribute("srkmsc")]
        [Display(Name = "Месеци")]
        [Range(0, int.MaxValue, ErrorMessage = "Въведете месеци в интервала 0-9999")]
        public int Months { get; set; }

        /// <summary>
        /// srkgdn
        /// Срок в години
        /// </summary>
        [XmlAttribute("srkgdn")]
        [Display(Name = "Години")]
        [Range(0, int.MaxValue, ErrorMessage = "Въведете години в интервала 0-9999")]
        public int Years { get; set; }

        /// <summary>
        /// srkdta
        /// Срок за обжалване 
        /// srkdtadod ??? не се ползва
        /// </summary>
        [Display(Name = "Oбжалване до")]
        [XmlAttribute("srkdta", DataType = "date")]
        [Required]
        public DateTime SrokDate { get; set; }

        [XmlIgnore]
        [Display(Name = "Oбжалване до")]
        public DateTime? SrokDateVM
        {
            get
            {
                return SrokDate > defaultDate ? (DateTime?)SrokDate : (DateTime?)null;
            }
            set
            {
                SrokDate = value ?? defaultDate;
            }
        }

        /// <summary>
        /// srkvid
        /// вид на срок
        /// системен код на елемент от nmk_srkvid
        /// </summary>
        [XmlAttribute("srkvid")]
        [Display(Name = "Вид на събитието")]
        public int SrokType { get; set; }

        /// <summary>
		/// Дата, която трябва да се игнорира
		/// </summary>
		private static readonly DateTime defaultDate = default;

        /// <summary>
        /// Игнорира празна дата
        /// </summary>
        /// <returns></returns>
        public bool ShouldSerializeSrokDate()
        {
            return SrokDate != defaultDate;
        }
    }
}
