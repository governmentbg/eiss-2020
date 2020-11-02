// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Интервали по дело: в процес, спряно, преустановено
    /// </summary>
    [Table("case_lifecycle")]
    public class CaseLifecycle : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        /// <summary>
        /// 1-разглежда се,2-спряно
        /// </summary>
        [Column("lifecycle_type_id")]
        [Display(Name = "Вид интервал")]
        public int LifecycleTypeId { get; set; }

        [Column("iteration")]
        [Display(Name = "Повторение")]
        public int Iteration { get; set; }

        [Column("date_from")]
        [Display(Name = "От дата")]
        public DateTime DateFrom { get; set; }

        [Column("date_to")]
        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Column("duration_months")]
        [Display(Name = "Продължителност в месеци")]
        public int DurationMonths { get; set; }

        [Column("case_session_act_id")]
        [Display(Name = "Акт затворил интервала")]
        public int? CaseSessionActId { get; set; }

        [Column("case_session_result_id")]
        [Display(Name = "Акт създал интервала")]
        public int? CaseSessionResultId { get; set; }

        [Column("description")]
        [Display(Name = "Забележка")]
        public string Description { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(LifecycleTypeId))]
        public virtual LifecycleType LifecycleType { get; set; }

        [ForeignKey(nameof(CaseSessionActId))]
        public virtual CaseSessionAct CaseSessionAct { get; set; }

        [ForeignKey(nameof(CaseSessionResultId))]
        public virtual CaseSessionResult CaseSessionResult { get; set; }
    }
}
