using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Наложени наказания към присъда
    /// </summary>
    [Table("case_person_sentence_punishment_crime")]
    public class CasePersonSentencePunishmentCrime : UserDateWRT, IExpiredInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int? CaseId { get; set; }

        [Column("case_person_sentence_punishment_id")]
        public int CasePersonSentencePunishmentId { get; set; }

        [Column("case_crime_id")]
        [Display(Name = "Престъпление")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете Престъпление")]
        public int CaseCrimeId { get; set; }

        [Column("person_role_in_crime_id")]
        [Display(Name = "Роля на лицето в престъплението")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете Роля на лицето в престъплението")]
        public int PersonRoleInCrimeId { get; set; }

        [Column("recidive_type_id")]
        [Display(Name = "Рецидив")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете Рецидив")]
        public int RecidiveTypeId { get; set; }

        public int? SentenceTypeId { get; set; }

        //--------Пари-------------------------
        [Column("sentence_money")]
        [Display(Name = "Размер, лв")]
        public decimal SentenseMoney { get; set; }
        //--------Време-------------------------
        [Column("sentence_days")]
        [Display(Name = "Дни")]
        [Range(0, int.MaxValue, ErrorMessage = "Въведете дни в интервала 0-9999")]
        public int SentenseDays { get; set; }
        
        [Column("sentence_weeks")]
        [Display(Name = "Седмици")]
        [Range(0, int.MaxValue, ErrorMessage = "Въведете седмици в интервала 0-9999")]
        public int SentenseWeeks { get; set; }
        
        [Column("sentence_months")]
        [Display(Name = "Месеци")]
        [Range(0, int.MaxValue, ErrorMessage = "Въведете месеци в интервала 0-9999")]
        public int SentenseMonths { get; set; }

        [Column("sentence_years")]
        [Display(Name = "Години")]
        [Range(0, int.MaxValue, ErrorMessage = "Въведете години в интервала 0-9999")]
        public int SentenseYears { get; set; }

        [ForeignKey(nameof(SentenceTypeId))]
        public virtual SentenceType SentenceType { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CasePersonSentencePunishmentId))]
        public virtual CasePersonSentencePunishment CasePersonSentencePunishment { get; set; }

        [ForeignKey(nameof(CaseCrimeId))]
        public virtual CaseCrime CaseCrime { get; set; }

        [ForeignKey(nameof(PersonRoleInCrimeId))]
        public virtual PersonRoleInCrime PersonRoleInCrime { get; set; }

        [ForeignKey(nameof(RecidiveTypeId))]
        public virtual RecidiveType RecidiveType { get; set; }

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
    }
}
