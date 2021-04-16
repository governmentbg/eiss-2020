using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// NPRPNESTA
    /// NPRPNE
    /// Престъпление по НП
    /// </summary>
    public class CiminalProceedingCrime
    {
        /// <summary>
        /// nprpnedta
        /// Дата на статуса на НП за престъпление
        /// </summary>
        [XmlAttribute("nprpnedta", DataType = "date")]
        [Display(Name = "Дата на статуса на НП за престъпление")]
        public DateTime StatusDate { get; set; }

        /// <summary>
        /// nprpnests
        /// Статус на НП за престъпление
        /// Номенклатура nmk_nprpnests
        /// </summary>
        [XmlAttribute("nprpnests")]
        [Display(Name = "Статус на НП за престъпление")]
        public int Status { get; set; }
        
        [XmlIgnore]
        [Display(Name = "Статус на НП за престъпление")]
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