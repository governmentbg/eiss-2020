using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Мерки към лица по НД по дело
    /// </summary>
    [Table("case_person_measures")]
    public class CasePersonMeasure : UserDateWRT, IExpiredInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        [Column("parent_id")]
        public int? ParentId { get; set; }

        [Column("case_person_id")]
        [Display(Name = "Лице")]
        public int CasePersonId { get; set; }

        [Column("measure_institution_id")]
        [Display(Name = "Институция, определила мярката")]
        public int? MeasureInstitutionId { get; set; }

        [Column("measure_court_id")]
        [Display(Name = "Съд, определил мярката")]
        public int? MeasureCourtId { get; set; }

        [Column("measure_kind_id")]
        [Display(Name = "Вид мярка")]
        public int? MeasureKindId { get; set; }

        [Column("measure_type")]
        [Display(Name = "Мярка")]
        // eispp_tbl_code =214
        public string MeasureType { get; set; }

        [Column("measure_type_label")]
        [Display(Name = "Вид мярка")]
        // eispp_tbl_code =214
        public string MeasureTypeLabel { get; set; }

        [Column("measure_quantity")]
        [Display(Name = "Количество")]
        public int? MeasureQuantity { get; set; }

        [Column("measure_unit")]
        [Display(Name = "Мерна единица")]
        public string MeasureUnit { get; set; }

        [Column("measure_status_date")]
        [Display(Name = "Дата на мярката")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime MeasureStatusDate { get; set; }

        [Column("bail_amount")]
        [Display(Name = "Гаранция, евро")]
        public double BailAmount { get; set; }

        // eispp_tbl_code =215
        [Display(Name = "Статус")]
        [Column("measure_status")]
        [Required(ErrorMessage = "Изберете {0}.")]
        public string MeasureStatus { get; set; }

        // eispp_tbl_code =215
        [Display(Name = "Статус")]
        [Column("measure_status_label")]
        public string MeasureStatusLabel { get; set; }

        /// <summary>
        /// Връзка с MQEpep
        /// </summary>
        [Column("mq_epep_id")]
        public int? MQEpepId { get; set; }

        /// <summary>
        /// Връзка с MQEpep
        /// </summary>
        [Column("mq_epep_is_send")]
        public bool? MQEpepIsSend { get; set; }

        //--------Време-------------------------
        [Column("measure_days")]
        [Range(0, int.MaxValue, ErrorMessage = "Въведете дни в интервала 0-9999")]
        [Display(Name = "Дни")]
        public int MeasureDays { get; set; }

        [Column("measure_weeks")]
        [Display(Name = "Седмици")]
        [Range(0, int.MaxValue, ErrorMessage = "Въведете седмици в интервала 0-9999")]
        public int MeasureWeeks { get; set; }

        [Column("measure_months")]
        [Display(Name = "Месеци")]
        [Range(0, int.MaxValue, ErrorMessage = "Въведете месеци в интервала 0-9999")]
        public int MeasureMonths { get; set; }

        [Column("measure_years")]
        [Display(Name = "Години")]
        [Range(0, int.MaxValue, ErrorMessage = "Въведете години в интервала 0-9999")]
        public int MeasureYears { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(MeasureCourtId))]
        public virtual Court MeasureCourt { get; set; }

        [ForeignKey(nameof(ParentId))]
        public virtual CasePersonMeasure ParentMeasure { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CasePersonId))]
        public virtual CasePerson CasePerson { get; set; }

        [ForeignKey(nameof(MeasureInstitutionId))]
        public virtual Institution MeasureInstitution { get; set; }


        public virtual ICollection<CasePersonSentencePunishmentMeasure> Punishments { get; set; }


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
    }
}
