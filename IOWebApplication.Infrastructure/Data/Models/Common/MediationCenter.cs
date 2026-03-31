// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Nest.JoinField;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Центрове за медиация
    /// </summary>
    [Table("common_mediation_center")]
    [Comment("Центрове за медиация")]
    public class MediationCenter : UserDateWRT, IExpiredInfo
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        [Key]
        [Column("id")]
        [Comment("Идентификатор на запис")]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на родител
        /// </summary>
        [Column("parent_id")]
        [Comment("Идентификатор на родител")]
        public int? ParentId { get; set; }

        /// <summary>
        /// Име на центъра
        /// </summary>
        [Column("name")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        [Comment("Име на центъра")]
        public string Name { get; set; }

        /// <summary>
        /// Адрес
        /// </summary>
        [Column("address_text")]
        [Comment("Адрес")]
        public string AddressText { get; set; }

        /// <summary>
        /// Контактни данни
        /// </summary>
        [Column("contact_details")]
        [Comment("Контактни данни")]
        public string ContactDetails { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        [Column("description")]
        [Comment("Описание")]
        public string Description { get; set; }

        /// <summary>
        /// От дата
        /// </summary>
        [Column("date_from")]
        [Comment("От дата")]
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// До дата
        /// </summary>
        [Column("date_to")]
        [Comment("До дата")]
        public DateTime? DateTo { get; set; }

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
        /// Родител
        /// </summary>
        [ForeignKey(nameof(ParentId))]
        public virtual MediationCenter Parent { get; set; }

        /// <summary>
        /// Съдилища които обслужва центъра
        /// </summary>
        public virtual ICollection<MediationCenterCourt> Courts { get; set; }

        /// <summary>
        /// Координатори
        /// </summary>
        public virtual ICollection<MediationCoordinatorCenter> Coordinators { get; set; }

        /// <summary>
        /// Медиатори
        /// </summary>
        public virtual ICollection<MediationMediatorCenter> Mediators { get; set; }
    }
}
