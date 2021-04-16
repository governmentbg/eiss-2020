using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// Пробационна мярка
    /// </summary>
    public class ProbationMeasure
    {
		/// <summary>
		/// Далие е в сила наказанието
		/// </summary>
		[XmlIgnore]
		[Display(Name = "Пробационно мярка")]
		public bool IsSelected { get; set; } = true;

		/// <summary>
		/// ADR
		/// Адрес/Място
		/// </summary>
		[XmlElement("ADR")]
		public EisppAddress Address { get; set; }

		/// <summary>
		/// pbcvid
		/// вид пробационна мярка
		/// Номенклатура nmk_vid_pbc
		/// задължително
		/// </summary>
		[Display(Name = "Вид пробационна мярка")]
		[XmlAttribute("pbcvid")]
		public int MeasureType { get; set; }

		/// <summary>
		/// pbcsid
		/// Системен идентификатор
		/// </summary>
		[XmlAttribute("pbcsid")]
		public int MeasureId { get; set; }

		/// <summary>
		/// pbcrzm
		/// количествено изражение на мярка
		/// </summary>
		[Display(Name = "Количествено изражение на мярка")]
		[XmlAttribute("pbcrzm", DataType = "double")]
		public double MeasureAmount { get; set; }

		/// <summary>
		/// pbcotd
		/// валидна от
		/// </summary>
		[XmlAttribute("pbcotd", DataType = "date")]
		public DateTime ValidFrom { get; set; }
		
		[XmlIgnore]
		[Display(Name = "Валидна от")]
		public DateTime? ValidFromVM
		{
			get
			{
				return ValidFrom > defaultDate ? (DateTime?)ValidFrom : (DateTime?)null;
			}
			set
			{
				ValidFrom = value ?? defaultDate;
			}
		}

		/// <summary>
		/// pbcdod
		/// валидна до
		/// </summary>
		[XmlAttribute("pbcdod", DataType = "date")]
		public DateTime ValidTill { get; set; }

		[XmlIgnore]
		[Display(Name = "Валидна до")]
		public DateTime? ValidTillVM
		{
			get
			{
				return ValidTill > defaultDate ? (DateTime?)ValidTill : (DateTime?)null;
			}
			set
			{
				ValidTill = value ?? defaultDate;
			}
		}

		/// <summary>
		/// pbcopi
		/// забележка
		/// </summary>
		[Display(Name = "Забележка")]
		[XmlAttribute("pbcopi")]
		public string Note { get; set; }

		/// <summary>
		/// pbcmsc
		/// размер на мярка в месеци
		/// </summary>
		[Display(Name = "Mесеци")]
		[XmlAttribute("pbcmsc")]
		public int Months { get; set; }

		/// <summary>
		/// pbcmered
		/// мерна единица
		/// номенклатура nmk_pbcu
		/// </summary>
		[Display(Name = "Мерна единица")]
		[XmlAttribute("pbcmered")]
		public int Unit { get; set; }

		/// <summary>
		/// pbcgdn
		/// размер на мярка в години
		/// </summary>
		[Display(Name = "Години")]
		[XmlAttribute("pbcgdn")]
		public int Years { get; set; }

		/// <summary>
		/// pbcden
		/// размер на мярка в дни
		/// </summary>
		[Display(Name = "Дни")]
		[XmlAttribute("pbcden")]
		public int Days { get; set; }

		/// <summary>
		/// Дата, която трябва да се игнорира
		/// </summary>
		private static readonly DateTime defaultDate = default;

		/// <summary>
		/// Игнорира празна дата
		/// </summary>
		/// <returns></returns>
		public bool ShouldSerializeValidFrom()
		{
			return ValidFrom != defaultDate;
		}

		[XmlIgnore]
		public int Index { get; set; }

		/// <summary>
		/// Игнорира празна дата
		/// </summary>
		/// <returns></returns>
		public bool ShouldSerializeValidTill()
		{
			return ValidTill != defaultDate;
		}
	}
}