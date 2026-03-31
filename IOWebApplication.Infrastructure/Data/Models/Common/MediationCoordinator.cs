// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Координатори на центрове за медиация
    /// </summary>
    [Table("common_mediation_coordinator")]
    [Comment("Координатори на центрове за медиация")]
    public class MediationCoordinator : UserDateWRT, IExpiredInfo
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        [Key]
        [Column("id")]
        [Comment("Идентификатор на запис")]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на лице
        /// </summary>
        [Column("lawunit_id")]
        [Comment("Идентификатор на лице")]
        public int LawUnitId { get; set; }

        /// <summary>
        /// Идентификатор на потребителя на лицето
        /// </summary>
        [Column("lawunit_user_id")]
        [Comment("Идентификатор на потребителя на лицето")]
        public string LawUnitUserId { get; set; }

        /// <summary>
        /// Длъжност
        /// </summary>
        [Column("position")]
        [Comment("Длъжност")]
        public string Position { get; set; }

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
        [ForeignKey(nameof(LawUnitId))]
        public virtual LawUnit LawUnit { get; set; }

        /// <summary>
        /// Потребител на медиатора
        /// </summary>
        [ForeignKey(nameof(LawUnitUserId))]
        public virtual ApplicationUser LawUnitUser { get; set; }

        /// <summary>
        /// Потребител, който е анулирал записа
        /// </summary>
        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }

        /// <summary>
        /// Центрове които обслужва координатора
        /// </summary>
        public virtual ICollection<MediationCoordinatorCenter> Centers { get; set; }
    }
}
