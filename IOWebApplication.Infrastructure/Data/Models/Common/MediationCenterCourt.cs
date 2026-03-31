// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using static Nest.JoinField;
using Microsoft.EntityFrameworkCore;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Кои съдилища се обслужват от даден център
    /// </summary>
    [Table("common_mediation_center_court")]
    [Comment("Кои съдилища се обслужват от даден център")]
    public class MediationCenterCourt
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        [Key]
        [Column("id")]
        [Comment("Идентификатор на запис")]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на център за медиация
        /// </summary>
        [Column("mediation_center_id")]
        [Comment("Идентификатор на център за медиация")]
        public int? MediationCenterId { get; set; }

        /// <summary>
        /// Идентификатор на съд
        /// </summary>
        [Column("court_id")]
        [Comment("Идентификатор на съд")]
        public int CourtId { get; set; }

        /// <summary>
        /// Родител
        /// </summary>
        [ForeignKey(nameof(MediationCenterId))]
        public virtual MediationCenter MediationCenter { get; set; }

        /// <summary>
        /// Съд
        /// </summary>
        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }
    }
}
