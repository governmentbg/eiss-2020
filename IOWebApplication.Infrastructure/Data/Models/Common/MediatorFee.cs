// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Ставки за заплащане на медиатори
    /// </summary>
    [Table("common_mediator_fee")]
    [Comment("Ставки за заплащане на медиатори")]
    public class MediatorFee : UserDateWRT
    {
        /// <summary>
        /// Идентификаотр на записа
        /// </summary>
        [Key]
        [Column("id")]
        [Comment("Идентификаотр на записа")]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на вид среща за медиация
        /// </summary>
        [Column("mediation_type_id")]
        [Comment("Идентификатор на вид среща за медиация")]
        public int MediationTypeId { get; set; }

        /// <summary>
        /// Възнаграждение на час
        /// </summary>
        [Column("hour_fee")]
        [Comment("Възнаграждение на час")]
        public decimal HourFee { get; set; }

        /// <summary>
        /// Възнаграждение на час в евро
        /// </summary>
        [Column("hour_fee_eur")]
        [Comment("Възнаграждение на час в евро")]
        public decimal HourFeeEUR { get; set; }

        /// <summary>
        /// Дата от
        /// </summary>
        [Column("date_from")]
        [Comment("Дата от")]
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// Дата до
        /// </summary>
        [Column("date_to")]
        [Comment("Дата до")]
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Вид среща за медиация
        /// </summary>
        [ForeignKey(nameof(MediationTypeId))]
        public virtual MediationType MediationType { get; set; }
    }
}
