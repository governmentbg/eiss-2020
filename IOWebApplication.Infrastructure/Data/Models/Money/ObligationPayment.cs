// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Money
{
    /// <summary>
    /// Плащане към задължение
    /// </summary>
    [Table("money_obligation_payment")]
    public class ObligationPayment : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("obligation_id")]
        public int ObligationId { get; set; }

        [Column("payment_id")]
        public int PaymentId { get; set; }

        [Column("amount")]
        public decimal Amount { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("description")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Column("date_disabled")]
        public DateTime? DateDisabled { get; set; }

        [Column("user_disabled")]
        public string UserDisabledId { get; set; }

        [ForeignKey(nameof(ObligationId))]
        public virtual Obligation Obligation { get; set; }

        [ForeignKey(nameof(PaymentId))]
        public virtual Payment Payment { get; set; }

        [ForeignKey(nameof(UserDisabledId))]
        public virtual ApplicationUser UserDisabled { get; set; }
    }
}
