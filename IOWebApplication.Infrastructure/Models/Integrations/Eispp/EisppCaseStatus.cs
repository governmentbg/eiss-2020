using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// DLOSTA
    /// Статус на дело
    /// </summary>
    public class EisppCaseStatus
    {
        /// <summary>
        /// dlostasid
        /// Системен идентификатор на статус на дело
        /// </summary>
        [XmlAttribute("dlostasid")]
        public int StatusId { get; set; }

        /// <summary>
        /// dlodtasts
        /// Дата на статус на дело
        /// </summary>
        [XmlAttribute("dlodtasts", DataType = "date")]
        [Display(Name="Дата на статуса")]
        public DateTime StatusDate { get; set; }
        [XmlIgnore]
        [Display(Name = "Дата на статус")]
        public DateTime? StatusDateVM
        {
            get
            {
                return StatusDate > defaultDate ? (DateTime?)StatusDate : (DateTime?)null;
            }
            set
            {
                StatusDate = value ?? defaultDate;
            }
        }

        /// <summary>
        /// dlosts
        /// Статус на дело
        /// </summary>
        [XmlAttribute("dlosts")]
        [Display(Name = "Статус на дело")]
        public int CaseStatus { get; set; }

        /// <summary>
		/// Дата, която трябва да се игнорира
		/// </summary>
		private static readonly DateTime defaultDate = default;

        /// <summary>
        /// Игнорира празна дата
        /// </summary>
        /// <returns></returns>
        public bool ShouldSerializeStatusDate()
        {
            return StatusDate != defaultDate;
        }
    }
}
