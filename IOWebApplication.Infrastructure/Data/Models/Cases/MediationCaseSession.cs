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
    /// Среща по дело - медиация
    /// </summary>
    [Table("mediation_case_session")]
    [Comment("Среща по дело - медиация")]
    public class MediationCaseSession : UserDateWRT, IExpiredInfo, IHaveId
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
        /// Идентификатор на център
        /// </summary>
        [Column("mediation_centers_id")]
        [Comment("Идентификатор на център")]
        public int? MediationCenterId { get; set; }

        /// <summary>
        /// Идентификатор на вид среща: Информационна среща; Процедура по медиация.
        /// </summary>
        [Column("mediation_type_id")]
        [Comment("Идентификатор на вид среща: Информационна среща; Процедура по медиация.")]
        public int MediationTypeId { get; set; }

        /// <summary>
        /// Идентификатор на място на което се провежда
        /// </summary>
        [Column("mediation_location_id")]
        [Comment("Идентификатор на място на което се провежда")]
        public int? MediationLocationId { get; set; }

        /// <summary>
        /// Допълнително пояснение за място на което се провежда
        /// </summary>
        [Column("mediation_location_description")]
        [Comment("Допълнително пояснение за място на което се провежда")]
        public string MediationLocationDescription { get; set; }

        /// <summary>
        /// Начало: дата и час
        /// </summary>
        [Column("date_from")]
        [Comment("Начало: дата и час")]
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// Край: дата и час
        /// </summary>
        [Column("date_to")]
        [Comment("Край: дата и час")]
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Статус на срещата
        /// </summary>
        [Column("mediation_state_id")]
        [Comment("Статус на срещата")]
        public int? MediationStateId { get; set; }

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
        /// Център
        /// </summary>
        [ForeignKey(nameof(MediationCenterId))]
        public virtual MediationCenter Center { get; set; }

        /// <summary>
        /// Вид среща
        /// </summary>
        [ForeignKey(nameof(MediationTypeId))]
        public virtual MediationType MediationType { get; set; }

        /// <summary>
        /// Място на среща
        /// </summary>
        [ForeignKey(nameof(MediationLocationId))]
        public virtual MediationLocation MediationLocation { get; set; }

        /// <summary>
        /// Статус на среща
        /// </summary>
        [ForeignKey(nameof(MediationStateId))]
        public virtual MediationState MediationState { get; set; }


        /// <summary>
        /// Потребител, който е анулирал записа
        /// </summary>
        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }

        /// <summary>
        /// Страни по делото
        /// </summary>
        public virtual ICollection<MediationCasePerson> People { get; set; }

        /// <summary>
        /// Резултат към срещата
        /// </summary>
        public virtual ICollection<MediationCaseSessionResult> Results { get; set; }

        /// <summary>
        /// Документи към срещата
        /// </summary>
        public virtual ICollection<MediationCaseSessionDocument> Documents { get; set; }

        /// <summary>
        /// Оценки
        /// </summary>
        public virtual ICollection<MediationCaseMediatorAppraisal> Appraisals { get; set; }

        /// <summary>
        /// Свързани дела към среща за медиация
        /// </summary>
        public virtual ICollection<MediationCaseSessionLinkCase> LinkCases { get; set; }
    }
}
