// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Money
{
    /// <summary>
    /// Разходен касов ордер
    /// </summary>
    [Table("money_expense_order")]
    public class ExpenseOrder : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("reg_number")]
        public string RegNumber { get; set; }

        [Column("reg_date")]
        public DateTime RegDate { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("region_name")]
        public string RegionName { get; set; }

        [Column("firm_name")]
        public string FirmName { get; set; }

        [Column("firm_city")]
        public string FirmCity { get; set; }

        [Column("paid_note")]
        public string PaidNote { get; set; }

        [Column("Iban")]
        public string Iban { get; set; }

        [Column("expense_order_state_id")]
        public int? ExpenseOrderStateId { get; set; }

        [Column("bic")]
        public string BIC { get; set; }

        [Column("bank_name")]
        public string BankName { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(ExpenseOrderStateId))]
        public virtual ExpenseOrderState ExpenseOrderState { get; set; }

        public virtual ICollection<ExpenseOrderObligation> ExpenseOrderObligations { get; set; }

        public ExpenseOrder()
        {
            ExpenseOrderObligations = new HashSet<ExpenseOrderObligation>();
        }

    }
}
