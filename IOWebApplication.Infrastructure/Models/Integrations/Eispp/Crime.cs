using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// PNE
    /// Данните за престъпление заедно със специфики на 
    /// престъпление, адрес, пострадал...
    /// </summary>
    public class Crime
    {
		/// <summary>
		/// pnebrjnzv
		/// Брой неизвестни субекти на престъпление
		/// </summary>
		[XmlAttribute("pnebrjnzv")]
		public int UnknownSubjectsCount { get; set; }

		/// <summary>
		/// pnebrjrne
		/// Брой ранени
		/// </summary>
		[XmlAttribute("pnebrjrne")]
		public int WoundedCount { get; set; }

		/// <summary>
		/// pnebrjubt
		/// Брой убити
		/// </summary>
		[XmlAttribute("pnebrjubt")]
		public int KilledCount { get; set; }

		/// <summary>
		/// pnedtadod
		/// Крайна дата на период
		/// </summary>
		[XmlAttribute("pnedtadod", DataType = "date")]
		[Display(Name = "Крайна дата")]
		public DateTime EndDate { get; set; }

		/// <summary>
		/// pnedtaotd
		/// Дата на извършване или начална дата на период
		/// задължително
		/// </summary>
		[XmlAttribute("pnedtaotd", DataType = "date")]
		[Display(Name = "Дата на извършване/начална дата")]
		public DateTime StartDate { get; set; }

		/// <summary>
		/// pnenmr
		/// ЕИСПП номер на престъпление
		/// </summary>
		[XmlAttribute("pnenmr")]
		[Display(Name = "Номер на престъпление")]
		public string EisppNumber { get; set; }

		/// <summary>
		/// pneotdtip
		/// Тип на дата на престъпление
		/// Номенклатура nmk_pneotdtip
		/// задължително
		/// </summary>
		[XmlAttribute("pneotdtip")]
		[Display(Name = "Тип на дата на престъпление")]
		public int StartDateType { get; set; }

		/// <summary>
		/// pnesid
		/// Системен идентификатор
		/// Използва се с префикс 'p' (p34)
		/// Прави връзка с останалите асти на XML пакета
		/// човек - престъпление например
		/// </summary>
		[XmlAttribute("pnesid")]
		public int CrimeId { get; set; }

		/// <summary>
		/// pnestinsn
		/// Нанесени щети (лв)
		/// </summary>
		[XmlAttribute("pnestinsn", DataType = "double")]
		public double DamageAmount { get; set; }

		/// <summary>
		/// pnestiobz
		/// Обезпечени щети (лв)
		/// </summary>
		[XmlAttribute("pnestiobz", DataType = "double")]
		public double CoveredDamageAmount { get; set; }

		/// <summary>
		/// pnestv
		/// Състав по НК
		/// </summary>
		[XmlAttribute("pnestv")]
		[Display(Name = "Състав по НК")]
		public string CrimeConstitution { get; set; }

		/// <summary>
		/// pnetxt
		/// Фактическа обстановка
		/// </summary>
		[XmlAttribute("pnetxt")]
		public string CrimeDescription { get; set; }

		/// <summary>
		/// pnechs
		/// Час на извършване на престъплението
		/// </summary>
		[XmlAttribute("pnechs")]
		public int CrimeCommitTime { get; set; }

		/// <summary>
		/// pnevmerzk
		/// Време за разкриване на престъпление
		/// Номенклатура nmk_pnevmerzk
		/// </summary>
		[XmlAttribute("pnevmerzk")]
		public int CrimeDetectionTime { get; set; }

		/// <summary>
		/// PNESPF
		/// Специфика на престъпление
		/// </summary>
		[XmlElement("PNESPF")]
		public CrimeDetails[] CrimeDetails { get; set; }

		/// <summary>
		/// ADR
		/// Адрес / място
		/// </summary>
		[XmlElement("ADR")]
		public EisppAddress[] Addresses { get; set; }

		/// <summary>
		/// NPRPNESTA
		/// NPRPNE
		/// Престъпление по НП
		/// </summary>
		[XmlElement("NPRPNESTA")]
		public CiminalProceedingCrime CiminalProceedingCrime { get; set; }

		/// <summary>
		/// PSG
		/// Предмет на посегателство
		/// </summary>
		[XmlElement("PSG")]
		public CrimeSubject[] CrimeSubjects { get; set; }

		/// <summary>
		/// PNESTA
		/// Статус на престъпление
		/// </summary>
		[XmlElement("PNESTA")]
		public CrimeStatus CrimeStatus { get; set; }

		/// <summary>
		/// Дата, която трябва да се игнорира
		/// </summary>
		public static readonly DateTime defaultDate = default;

		/// <summary>
		/// Игнорира празна дата
		/// </summary>
		/// <returns></returns>
		public bool ShouldSerializeStartDate()
		{
			return StartDate != defaultDate;
		}

		/// <summary>
		/// Игнорира празна дата
		/// </summary>
		/// <returns></returns>
		public bool ShouldSerializeEndDate()
		{
			return EndDate != defaultDate;
		}

		[XmlIgnore]
        [XmlElement("NPRFZLPNE")]
		public CPPersonCrime[] CPPersonCrimes { get; set; }

		[XmlIgnore]
		[Display(Name = "Крайна дата")]
		public DateTime? EndDateVM { 
			get {
				return EndDate > defaultDate ? (DateTime?)EndDate : (DateTime?)null;
			}
			set {
				EndDate = value ?? defaultDate;
			} 
		}

		[XmlIgnore]
		[Display(Name = "Дата на извършване/начална дата")]
		public DateTime? StartDateVM
		{
			get
			{
				return StartDate > defaultDate ? (DateTime?)StartDate : (DateTime?)null;
			}
			set
			{
				StartDate = value ?? defaultDate;
			}
		}

		[XmlIgnore]
		[Display(Name = "Престъпление")]
		public bool IsSelected { get; set; }
	}
}
