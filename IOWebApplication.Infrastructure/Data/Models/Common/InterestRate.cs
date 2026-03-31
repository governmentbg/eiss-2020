// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Лихвени проценти
    /// </summary>
    [Table("common_interest_rate")]
    public class InterestRate : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Вид лихвен процент:1-ОЛП
        /// </summary>
        [Column("interest_type")]
        public int InterestType { get; set; }

        [Column("date")]
        [Display(Name = "Дата")]
        public DateTime Date { get; set; }

        [Column("rate")]
        [Display(Name = "Процент")]
        public decimal Rate { get; set; }

        [Column("is_active")]
        [Display(Name = "Активен запис")]
        public bool IsActive { get; set; }
    }
}
