using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// PNESTA
    /// Статус на престъпление
    /// </summary>
    public class CrimeStatus
    {
        /// <summary>
        /// pnekcq
        /// Квалификация по НК
        /// Номенклатура nmk_pnekcq
        /// </summary>
        [XmlAttribute("pnekcq")]
        [Display(Name = "Квалификация по НК")]
        public int CrimeQualification { get; set; }

        /// <summary>
        /// pnestpdvs
        /// Степен на довършеност на престъпление
        /// номенклатура nmk_pnestpdvs
        /// </summary>
        [XmlAttribute("pnestpdvs")]
        [Display(Name = "Степен довършеност")]
        public int CompletitionDegree { get; set; }

        /// <summary>
        /// pnests
        /// Статус на престъпление
        /// Номенклатура nmk_pnests
        /// </summary>
        [XmlAttribute("pnests")]
        [Display(Name = "Статус")]
        public int Status { get; set; }

        /// <summary>
        /// pnestsdta
        /// Дата на Статус на престъпление
        /// </summary>
        [XmlAttribute("pnestsdta", DataType = "date")]
        [Display(Name = "Дата на статус")]
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