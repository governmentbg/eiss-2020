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
    /// Суми по дела/заседание/участник в заседание
    /// </summary>
    [Table("case_money")]
    public class CaseMoney : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        [Column("case_session_id")]
        public int? CaseSessionId { get; set; }

        [Column("case_lawunit_id")]
        [Display(Name = "Участник")]
        public int? CaseLawUnitId { get; set; }

        [Column("money_type_id")]
        [Display(Name = "Вид сума")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете")]
        public int MoneyTypeId { get; set; }

        [Column("amount")]
        [Display(Name = "Сума")]
        public decimal Amount { get; set; }

        [Column("paid_date")]
        [Display(Name = "Платено на")]
        public DateTime? PaidDate { get; set; }

        [Column("description")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CaseSessionId))]
        public virtual CaseSession CaseSession { get; set; }

        [ForeignKey(nameof(CaseLawUnitId))]
        public virtual CaseLawUnit CaseSessionLawUnit { get; set; }

        [ForeignKey(nameof(MoneyTypeId))]
        public virtual MoneyType MoneyType { get; set; }

    }
}
