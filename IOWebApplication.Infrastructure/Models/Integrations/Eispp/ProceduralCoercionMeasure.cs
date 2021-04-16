using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
	/// <summary>
	/// MPP
	/// Мярка за процесуална принуда
	/// </summary>
	public class ProceduralCoercionMeasure
    {
		/// <summary>
		/// mppdta
		/// Дата на състоянието на МПП
		/// </summary>
		[Display(Name = "Дата на мярката")]
		[XmlAttribute("mppdta", DataType = "date")]
		public DateTime MeasureStatusDate { get; set; }
		
		[XmlIgnore]
		[Display(Name = "Дата на мярката")]
		public DateTime? MeasureStatusDateVM
		{
			get
			{
				return MeasureStatusDate > defaultDate ? (DateTime?)MeasureStatusDate : (DateTime?)null;
			}
			set
			{
				MeasureStatusDate = value ?? defaultDate;
			}
		}

		/// <summary>
		/// mpprzm
		/// Размер на гаранция в лева
		/// </summary>
		[Display(Name = "Гаранция, лв")]
		[XmlAttribute("mpprzm", DataType = "double")]
		public double BailAmount { get; set; }

		/// <summary>
		/// mppsid
		/// Системен идентификатор
		/// </summary>
		[Display(Name = "Гаранция, лв")]
		[XmlAttribute("mppsid")]
		public int MeasureId { get; set; }

		/// <summary>
		/// mppste
		/// Състояние на МПП
		/// задължително
		/// Номенклатура nmk_mppste
		/// </summary>
		[Display(Name = "Статус")]
		[XmlAttribute("mppste")]
		public int MeasureStatus { get; set; }

		/// <summary>
		/// mppstr
		/// Структура, определила състоянието на МПП
		/// Номенклатура nmk_strvid
		/// </summary>
		[XmlAttribute("mppstr")]
		public int MeasureStructure { get; set; }

		/// <summary>
		/// mppvid
		/// Вид на МПП
		/// задължително
		/// Номенклатура nmk_mppvid
		/// </summary>
		[Display(Name = "Вид мярка")]
		[XmlAttribute("mppvid")]
		public int MeasureType { get; set; }

		/// <summary>
		/// Дата, която трябва да се игнорира
		/// </summary>
		private static readonly DateTime defaultDate = default;

		/// <summary>
		/// Игнорира празна дата
		/// </summary>
		/// <returns></returns>
		public bool ShouldSerializeMeasureStatusDate()
		{
			return MeasureStatusDate != defaultDate;
		}

		[XmlIgnore]
		[Display(Name = "Вид институция")]
		public int? MeasureInstitutionTypeId { get; set; }

		[XmlIgnore]
		[Display(Name = "Институция, определила мярката")]
		[Required(ErrorMessage = "Изберете {0}.")]
		public int? MeasureInstitutionId { get; set; }

		[XmlIgnore]
		[Display(Name = "Институция, определила мярката")]
		public string InstitutionName { get; set; }

		[XmlIgnore]
		[Display(Name = "Вид институция")]
		public string InstitutionTypeName { get; set; }

		[XmlIgnore]
		[Display(Name = "Мяркa за процесуална принуда")]
		public bool IsSelected { get; set; }

		[XmlIgnore]
		public bool IsSelectedReadOnly { get; set; } = false;

	}
}