using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    /// <summary>
    /// NKZPNE
    /// Наказание за престъпление
    /// </summary>
    public class CrimePunishment
    {
		/// <summary>
		/// Далие е в сила наказанието
		/// </summary>
		[XmlIgnore]
		[Display(Name = "Наказание към престъплениe")]
		public bool IsSelected { get; set; } = true;

		/// <summary>
		/// Номер
		/// </summary>
		[XmlIgnore]
		public int Id { get; set; }

		/// <summary>
		/// nkzpnegdn
		/// Размер на наказание - 
		/// </summary>
		[Display(Name = "Години")]
		[XmlAttribute("nkzpnegdn")]
		public int PunishmentYears { get; set; }

		/// <summary>
		/// nkzpnemsc
		/// Размер на наказание - Месеци
		/// </summary>
		[Display(Name = "Месеци")]
		[XmlAttribute("nkzpnemsc")]
		public int PunishmentMonths { get; set; }

		/// <summary>
		/// nkzpnesdc
		/// Размер на наказание - Седмици
		/// </summary>
		[Display(Name = "Седмици")]
		[XmlAttribute("nkzpnesdc")]
		public int PunishmentWeeks { get; set; }

		// <summary>
		/// nkzpneden
		/// Размер на наказание - Дни
		/// </summary>
		[Display(Name = "Дни")]
		[XmlAttribute("nkzpneden")]
		public int PunishmentDays { get; set; }

		/// <summary>
		/// nkzpnerzm
		/// Размер на глоба в лева
		/// </summary>
		[Display(Name = "Размер на глоба лв.")]
		[XmlAttribute("nkzpnerzm", DataType = "double")]
		public double FineAmount { get; set; }


		/// <summary>
		/// nkzpnesid
		/// Системен идентификатор
		/// </summary>
		[XmlAttribute("nkzpnesid")]
		public int CrimePunishmentId { get; set; }

		/// <summary>
		/// nkzpnevid
		/// Вид наказние
		/// Номенклатура nmk_nkzvid
		/// задължително
		/// </summary>
		[Display(Name = "Тип наказание")]
		[XmlAttribute("nkzpnevid")]
		public int PunishmentKind { get; set; }

		public bool HavePeriod()
		{
			return !(PunishmentYears == 0 && PunishmentMonths == 0 && PunishmentWeeks == 0 && PunishmentDays == 0);
		}
	}
}