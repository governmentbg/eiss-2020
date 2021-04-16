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
    [Table("case_person_sentence_punishment")]
    public class CasePersonSentencePunishment : UserDateWRT, IExpiredInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int? CaseId { get; set; }

        [Column("case_person_sentence_id")]
        public int CasePersonSentenceId { get; set; }

        /// <summary>
        /// Сумарен ред за присъди
        /// </summary>
        [Column("is_summary_punishement_id")]
        [Display(Name = "Обобщаваща присъда")]
        public bool IsSummaryPunishment { get; set; }

        /// <summary>
        /// Вид присъда: лишаване от свобода,глоба и 
        /// </summary>
        [Column("sentence_type_id")]
        [Display(Name = "Наложено наказание по НК")]
        public int? SentenceTypeId { get; set; }

        //--------Пари-------------------------
        [Column("sentence_money")]
        [Display(Name = "Размер, лв")]
        public decimal SentenseMoney { get; set; }
        //--------Време-------------------------
        [Column("sentence_days")]
        [Range(0, int.MaxValue, ErrorMessage = "Въведете дни в интервала 0-9999")]
        [Display(Name = "Дни")]
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
        //--------Описание-------------------------
        [Column("sentence_text")]
        [Display(Name = "Описание")]
        public string SentenceText { get; set; }

        /// <summary>
        /// Режим :общ,лек и строг
        /// </summary>
        [Column("sentence_regime_type_id")]
        [Display(Name = "Режим на лишаване от свобода")]
        public int? SentenceRegimeTypeId { get; set; }

        [Column("date_from")]
        [Display(Name = "От дата")]
        public DateTime DateFrom { get; set; }

        [Column("date_to")]
        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Column("description")]
        [Display(Name = "Допълнително пояснение")]
        public string Description { get; set; }

        [Column("probation_start_date")]
        [Display(Name = "Начало изпитателeн срок")]
        public DateTime? ProbationStartDate { get; set; }

        [Column("probation_years")]
        [Display(Name = "Изпитателeн срок години")]
        public int? ProbationYears { get; set; }

        [Column("probation_months")]
        [Display(Name = "Изпитателeн срок месеци")]
        public int? ProbationMonths { get; set; }

        [Column("probation_weeks")]
        [Display(Name = "Изпитателeн срок седмици")]
        public int? ProbationWeeks { get; set; }

        [Column("probation_days")]
        [Display(Name = "Изпитателeн срок дни")]
        public int? ProbationDays { get; set; }

        [Column("is_main_punishment")]
        [Display(Name = "Основно наказание")]
        public bool IsMainPunishment { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CasePersonSentenceId))]
        public virtual CasePersonSentence CasePersonSentence { get; set; }

        [ForeignKey(nameof(SentenceTypeId))]
        public virtual SentenceType SentenceType { get; set; }

        [ForeignKey(nameof(SentenceRegimeTypeId))]
        public virtual SentenceRegimeType SentenceRegimeType { get; set; }

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

        public virtual ICollection<CasePersonSentencePunishmentCrime> CasePersonSentencePunishmentCrimes { get; set; }
    }
}
