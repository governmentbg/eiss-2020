using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Наказания за престъпления към лица по НД по дело
    /// </summary>
    [Table("case_person_crime_punishment")]
    public class CasePersonCrimePunishment : UserDateWRT, IExpiredInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("case_person_crime_id")]
        public int? CasePersonCrimeId { get; set; }

		[Column("case_person_sentence_punishment_crime_id")]
		public int CasePersonSentencePunishmentCrimeId { get; set; }

		/// <summary>
		/// nkzpnegdn
		/// Размер на наказание - 
		/// </summary>
		[Display(Name = "Години")]
		[Column("punishment_years")]
		public int PunishmentYears { get; set; }

		/// <summary>
		/// nkzpnemsc
		/// Размер на наказание - Месеци
		/// </summary>
		[Display(Name = "Месеци")]
		[Column("punishment_months")]
		public int PunishmentMonths { get; set; }

		/// <summary>
		/// nkzpnesdc
		/// Размер на наказание - Седмици
		/// </summary>
		[Display(Name = "Седмици")]
		[Column("punishment_weeks")]
		public int PunishmentWeeks { get; set; }

		// <summary>
		/// nkzpneden
		/// Размер на наказание - Дни
		/// </summary>
		[Display(Name = "Дни")]
		[Column("punishment_days")]
		public int PunishmentDays { get; set; }

		/// <summary>
		/// nkzpnerzm
		/// Размер на глоба в лева
		/// </summary>
		[Display(Name = "Размер на глоба лв.")]
		[Column("fine_amount")]
		public double FineAmount { get; set; }

		/// <summary>
		/// nkzpnevid
		/// Вид наказние
		/// Номенклатура nmk_nkzvid
		/// задължително
		/// </summary>
		[Display(Name = "Тип наказание")]
		[Range(1, int.MaxValue, ErrorMessage = "Изберете Тип наказание")]
		[Column("punishment_kind")]
		public int PunishmentKind { get; set; }

		public bool HavePeriod()
		{
			return !(PunishmentYears == 0 && PunishmentMonths == 0 && PunishmentWeeks == 0 && PunishmentDays == 0);
		}

		[ForeignKey(nameof(CasePersonCrimeId))]
		public virtual CasePersonCrime CasePersonCrime { get; set; }

		[ForeignKey(nameof(CasePersonSentencePunishmentCrimeId))]
		public virtual CasePersonSentencePunishmentCrime CasePersonSentencePunishmentCrime { get; set; }

		//################################################################################
		[Column("date_expired")]
        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        [Column("user_expired_id")]
        public string UserExpiredId { get; set; }

        [Column("description_expired")]
        [Display(Name = "Причина за анулиране")]
        public string DescriptionExpired { get; set; }

        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }
		[NotMapped]
		public int? Index { get; set; }
	}
}
