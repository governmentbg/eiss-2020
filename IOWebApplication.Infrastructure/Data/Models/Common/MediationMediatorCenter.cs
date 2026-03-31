// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using Microsoft.EntityFrameworkCore;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Центрове към медиатор
    /// </summary>
    [Table("common_mediation_mediator_center")]
    [Comment("Центрове към медиатор")]
    public class MediationMediatorCenter : UserDateWRT, IExpiredInfo
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        [Key]
        [Column("id")]
        [Comment("Идентификатор на запис")]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на медиатор
        /// </summary>
        [Column("mediation_mediator_id")]
        [Comment("Идентификатор на медиатор")]
        public int MediationMediatorId { get; set; }

        /// <summary>
        /// Идентификатор на център
        /// </summary>
        [Column("mediation_centers_id")]
        [Comment("Идентификатор на център")]
        public int MediationCenterId { get; set; }

        /// <summary>
        /// От
        /// </summary>
        [Column("date_from")]
        [Comment("От")]
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// До
        /// </summary>
        [Column("date_to")]
        [Comment("До")]
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Забележка
        /// </summary>
        [Column("description")]
        [Comment("Забележка")]
        public string Description { get; set; }

        /// <summary>
        /// Дата на анулиране
        /// </summary>
        [Column("date_expired")]
        [Comment("Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        /// <summary>
        /// Идентификатор на потребител, който е анулирал записа
        /// </summary>
        [Column("user_expired_id")]
        [Comment("Идентификатор на потребител, който е анулирал записа")]
        public string UserExpiredId { get; set; }

        /// <summary>
        /// Причина за анулиране
        /// </summary>
        [Column("description_expired")]
        [Comment("Причина за анулиране")]
        public string DescriptionExpired { get; set; }

        /// <summary>
        /// Медиатор
        /// </summary>
        [ForeignKey(nameof(MediationMediatorId))]
        public virtual MediationMediator Mediator { get; set; }

        /// <summary>
        /// Център
        /// </summary>
        [ForeignKey(nameof(MediationCenterId))]
        public virtual MediationCenter Center { get; set; }

        /// <summary>
        /// Потребител, който е анулирал записа
        /// </summary>
        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }
    }
}
