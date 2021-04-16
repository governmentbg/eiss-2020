using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// NKZ
    /// Наказание
    /// </summary>
    public class Punishment
    {
		/// <summary>
		/// Пробационна мярка
		/// </summary>
		[XmlElement("PBC")]
		public ProbationMeasure ProbationMeasure { get; set; }

		[XmlIgnore]
		public ProbationMeasure[] ProbationMeasures { get; set; }
		/// <summary>
		/// iptsrkdta
		/// Дата, от която започва да тече изпитателния срок
		/// </summary>
		[Display(Name = "Начало изпитателeн срок")]
		[XmlAttribute("iptsrkdta", DataType = "date")]
		public DateTime ProbationStartDate { get; set; }
		[XmlIgnore]
		[Display(Name = "Дата процесуалната санкция")]
		public DateTime? ProbationStartDateVM
		{
			get
			{
				return ProbationStartDate > defaultDate ? (DateTime?)ProbationStartDate : (DateTime?)null;
			}
			set
			{
				ProbationStartDate = value ?? defaultDate;
			}
		}

		/// <summary>
		/// iptsrkgdn
		/// Размер на изпитателен срок - Години
		/// </summary>
		[Display(Name = "Години")]
		[XmlAttribute("iptsrkgdn")]
		public int ProbationYears { get; set; }

		/// <summary>
		/// iptsrkmsc
		/// Размер на изпитателен срок - Месеци
		/// </summary>
		[Display(Name = "Месеци")]
		[XmlAttribute("iptsrkmsc")]
		public int ProbationMonths { get; set; }

		/// <summary>
		/// iptsrksdc
		/// Размер на изпитателен срок - Седмици
		/// </summary>
		[Display(Name = "Седмици")]
		[XmlAttribute("iptsrksdc")]
		public int ProbationWeeks { get; set; }

		/// <summary>
		/// iptsrkden
		/// Размер на изпитателен срок - Дни
		/// </summary>
		[Display(Name = "Дни")]
		[XmlAttribute("iptsrkden")]
		public int ProbationDays { get; set; }


		/// <summary>
		/// nkzakt
		/// 
		/// Номенклатура nmk_nkzakt
		/// задължително
		/// </summary>
		[Display(Name = "Активност на наказание")]
		[XmlAttribute("nkzakt")]
		public int PunishmentActivity { get; set; }

		/// <summary>
		/// nkzaktdta
		/// Дата на активност на наказание
		/// </summary>
		[Display(Name = "Начало наказание")]
		[XmlAttribute("nkzaktdta", DataType = "date")]
		public DateTime PunishmentActivityDate { get; set; }

		[XmlIgnore]
		[Display(Name = "Начало наказание")]
		public DateTime? PunishmentActivityDateVM
		{
			get
			{
				return PunishmentActivityDate > defaultDate ? (DateTime?)PunishmentActivityDate : (DateTime?)null;
			}
			set
			{
				PunishmentActivityDate = value ?? defaultDate;
			}
		}
		/// <summary>
		/// nkzden
		/// Размер на наказание - Дни
		/// </summary>
		[Display(Name = "Дни")]
		[XmlAttribute("nkzden")]
		public int PunishmentDays { get; set; }

		/// <summary>
		/// nkzgdn
		/// Размер на наказание - Години
		/// </summary>
		[Display(Name = "Години")]
		[XmlAttribute("nkzgdn")]
		public int PunishmentYears { get; set; }

		/// <summary>
		/// nkzmsc
		/// Размер на наказание - 
		/// </summary>
		[Display(Name = "Месеци")]
		[XmlAttribute("nkzmsc")]
		public int PunishmentMonths { get; set; }

		/// <summary>
		/// nkzncn
		/// Начин на изтърпяване на наказание
		/// Номенклатура nmk_nkzncn
		/// задължително
		/// </summary>
		[Display(Name = "Начин на изтърпяване на наказание")]
		[XmlAttribute("nkzncn")]
		public int ServingType { get; set; }

		/// <summary>
		/// nkzrjm
		/// Режим на изтърпяване на наказанието лишаване от свобода
		/// Номенклатура nmk_nkzrjm
		/// </summary>
		[Display(Name = "Режим на изтърпяване")]
		[XmlAttribute("nkzrjm")]
		public int PunishmentRegime { get; set; }

		/// <summary>
		/// nkzrzm
		/// Размер на глоба в лева
		/// </summary>
		[Display(Name = "Размер на глоба лв.")]
		[XmlAttribute("nkzrzm", DataType = "double")]
		public double FineAmount { get; set; }

		/// <summary>
		/// nkzsdc
		/// Размер на наказание - Седмици
		/// </summary>
		[Display(Name = "Седмици")]
		[XmlAttribute("nkzsdc")]
		public int PunishmentWeeks { get; set; }

		/// <summary>
		/// nkzsid
		/// Системен идентификатор
		/// </summary>
		[XmlAttribute("nkzsid")]
		public int PunishmentId { get; set; }

		/// <summary>
		/// nkzstr
		/// Структура, регистрирала наказанието
		/// Номенклатура nmk_strvid
		/// </summary>
		[XmlAttribute("nkzstr")]
		public int StructureId { get; set; }

		/// <summary>
		/// nkztip
		/// Тип наказание
		/// Номенклатура nmk_nkztip
		/// задължително
		/// </summary>
		[Display(Name = "Тип наказание")]
		[XmlAttribute("nkztip")]
		public int PunishmentType { get; set; }

		/// <summary>
		/// nkzvid
		/// Вид наказание
		/// Номенклатура nmk_nkzvid
		/// задължително
		/// </summary>
		[Display(Name = "Вид наказание")]
		[XmlAttribute("nkzvid")]
		public int PunishmentKind { get; set; }

		/// <summary>
		/// Дата, която трябва да се игнорира
		/// </summary>
		private static readonly DateTime defaultDate = default;

		/// <summary>
		/// Игнорира празна дата
		/// </summary>
		/// <returns></returns>
		public bool ShouldSerializeProbationStartDate()
		{
			return ProbationStartDate != defaultDate;
		}

		/// <summary>
		/// Игнорира празна дата
		/// </summary>
		/// <returns></returns>
		public bool ShouldSerializePunishmentActivityDate()
		{
			return PunishmentActivityDate != defaultDate;
		}

		public bool HavePeriod()
        {
			return !(PunishmentYears == 0 && PunishmentMonths == 0 && PunishmentWeeks == 0 && PunishmentDays == 0);
        }
		public bool HaveProbationPeriod()
		{
			return !(ProbationYears == 0 && ProbationMonths == 0 && ProbationWeeks == 0 && ProbationDays == 0);
		}
		[XmlIgnore]
		public int CasePersonSentencePunishmentId { get; set; }

		/// <summary>
		/// Дали е е в сила наказанието
		/// </summary>
		[XmlIgnore]
		[Display(Name = "Наказание")]
		public bool IsSelected { get; set; } = true;

		public void InitProbationMeasure()
        {
			ProbationMeasures = new ProbationMeasure[1]
			{
				ProbationMeasure
		    };
			ProbationMeasure = null;
		}
	}
}