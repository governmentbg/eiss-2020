// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Таблица с медиатори към дело
    /// </summary>
    [Table("mediation_case_mediator")]
    [Comment("Таблица с медиатори към дело")]
    public class MediationCaseMediator : UserDateWRT, IExpiredInfo
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        [Key]
        [Column("id")]
        [Comment("Идентификатор на запис")]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на съд
        /// </summary>
        [Column("court_id")]
        [Comment("Идентификатор на съд")]
        public int? CourtId { get; set; }

        /// <summary>
        /// Идентификатор на дело
        /// </summary>
        [Column("case_id")]
        [Comment("Идентификатор на дело")]
        public int CaseId { get; set; }

        /// <summary>
        /// Идентификатор на медиатор
        /// </summary>
        [Column("mediation_mediator_id")]
        [Comment("Идентификатор на медиатор")]
        public int MediationMediatorId { get; set; }

        /// <summary>
        /// Идентификатор на вид избор на медиатора в делото
        /// </summary>
        [Column("mediation_type_choice_mediator_id")]
        [Comment("Идентификатор на вид избор на медиатора в делото")]
        public int? MediationTypeChoiceMediatorId { get; set; }

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
        /// Съд
        /// </summary>
        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        /// <summary>
        /// Дело
        /// </summary>
        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        /// <summary>
        /// Медиатор
        /// </summary>
        [ForeignKey(nameof(MediationMediatorId))]
        public virtual MediationMediator Mediator { get; set; }

        /// <summary>
        /// Потребител, който е анулирал записа
        /// </summary>
        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }

        /// <summary>
        /// Вид избор на медиатора в делото
        /// </summary>
        [ForeignKey(nameof(MediationTypeChoiceMediatorId))]
        public virtual MediationTypeChoiceMediator MediationTypeChoiceMediator { get; set; }

        /// <summary>
        /// Центрове които обслужва медиатора
        /// </summary>
        public virtual ICollection<MediationCaseMediatorAppraisal> Appraisals { get; set; }
    }
}
