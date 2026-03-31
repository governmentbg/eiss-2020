// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Models.ViewModels.Case.Mediation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Таблица с оценка на медиатори към дело
    /// </summary>
    [Table("mediation_case_mediator_appraisal")]
    [Comment("Таблица с оценка на медиатори към дело")]
    public class MediationCaseMediatorAppraisal : UserDateWRT, IExpiredInfo
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
        /// Идентификатор на медиатор в дело
        /// </summary>
        [Column("mediation_case_mediator_id")]
        [Comment("Идентификатор на медиатор в дело")]
        public int MediationCaseMediatorId { get; set; }

        /// <summary>
        /// Идентификатор на среща
        /// </summary>
        [Column("mediation_case_session_id")]
        [Comment("Идентификатор на среща")]
        public int MediationCaseSessionId { get; set; }

        /// <summary>
        /// Идентификатор на страна от делото в среща
        /// </summary>
        [Column("mediation_case_person_id")]
        [Comment("Идентификатор на страна от делото в среща")]
        public int? MediationCasePersonId { get; set; }

        /// <summary>
        /// Тип оценка 1 - подробна / 2 - обобщена
        /// </summary>
        [Column("type_appraisal")]
        [Comment("Тип оценка 1 - подробна / 2 - обобщена")]
        public int? TypeAppraisal { get; set; }

        /// <summary>
        /// Дата на оценка
        /// </summary>
        [Column("date_appraisal")]
        [Comment("Дата на оценка")]
        public DateTime DateAppraisal { get; set; }

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
        /// Среща за медиация
        /// </summary>
        [ForeignKey(nameof(MediationCaseSessionId))]
        public virtual MediationCaseSession MediationCaseSession { get; set; }

        /// <summary>
        /// Медиатор в дело
        /// </summary>
        [ForeignKey(nameof(MediationCaseMediatorId))]
        public virtual MediationCaseMediator CaseMediator { get; set; }

        /// <summary>
        /// Страна от делото в среща
        /// </summary>
        [ForeignKey(nameof(MediationCasePersonId))]
        public virtual MediationCasePerson CasePerson { get; set; }

        /// <summary>
        /// Потребител, който е анулирал записа
        /// </summary>
        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }

        /// <summary>
        /// Оценки
        /// </summary>
        public virtual ICollection<MediationCaseMediatorAppraisalData> Data { get; set; }
    }
}
