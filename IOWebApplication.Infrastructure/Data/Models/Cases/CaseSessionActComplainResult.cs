﻿using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Резултат по Обжалвания към съдебен акт
    /// </summary>
    [Table("case_session_act_complain_result")]
    public class CaseSessionActComplainResult : UserDateWRT
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Обжалва се пред, съд
        /// </summary>
        [Column("complain_court_id")]
        public int? ComplainCourtId { get; set; }

        [Column("complain_case_id")]
        [Display(Name = "Дело")]
        public int? ComplainCaseId { get; set; }

        [Column("case_session_act_complain_id")]
        public int CaseSessionActComplainId { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        [Column("case_session_act_id")]
        [Display(Name = "Акт")]
        public int? CaseSessionActId { get; set; }

        [Column("act_result_id")]
        [Display(Name = "Резултат от обжалване")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
        public int? ActResultId { get; set; }

        [Display(Name = "Номер дело")]
        [Column("case_reg_number_other_system")]
        public string CaseRegNumberOtherSystem { get; set; }

        [Column("case_year_other_system")]
        [Display(Name = "Година дело")]
        public int? CaseYearOtherSystem { get; set; }

        [Column("case_short_number_other_system")]
        [Display(Name = "Код на дело")]
        public string CaseShortNumberOtherSystem { get; set; }

        [Column("case_session_act_other_system")]
        [Display(Name = "Акт")]
        public string CaseSessionActOtherSystem { get; set; }

        [Column("date_from_life_cycle")]
        [Display(Name = "Начална дата на интервал")]
        public DateTime? DateFromLifeCycle { get; set; }

        [Column("description")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Column("date_result")]
        [Display(Name = "Дата на отразяване на резултат")]
        public DateTime? DateResult { get; set; }

        [ForeignKey(nameof(CaseSessionActComplainId))]
        public virtual CaseSessionActComplain CaseSessionActComplain { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(ComplainCourtId))]
        public virtual Court ComplainCourt { get; set; }

        [ForeignKey(nameof(ComplainCaseId))]
        public virtual Case ComplainCase { get; set; }

        [ForeignKey(nameof(CaseSessionActId))]
        public virtual CaseSessionAct CaseSessionAct { get; set; }

        [ForeignKey(nameof(ActResultId))]
        public virtual ActResult ActResult { get; set; }

    }
}
