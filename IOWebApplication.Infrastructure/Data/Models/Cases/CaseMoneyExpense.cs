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
    /// Претендиран разноски по заповедни производства
    /// </summary>
    [Table("case_money_expense")]
    public class CaseMoneyExpense : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        [Column("case_money_expense_type_id")]
        [Display(Name = "Вид")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
        public int CaseMoneyExpenseTypeId { get; set; }

        [Column("currency_id")]
        [Display(Name = "Валута")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
        public int CurrencyId { get; set; }

        [Column("amount")]
        [Display(Name = "Сума")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public decimal Amount { get; set; }

        [Column("description")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Column("joint_distribution")]
        [Display(Name = "Солидарно разпределение")]
        public bool? JointDistribution { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CaseMoneyExpenseTypeId))]
        public virtual CaseMoneyExpenseType CaseMoneyExpenseType { get; set; }

        [ForeignKey(nameof(CurrencyId))]
        public virtual Currency Currency { get; set; }
    }
}
