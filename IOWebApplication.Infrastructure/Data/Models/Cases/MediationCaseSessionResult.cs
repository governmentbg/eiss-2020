// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Резултати в среща за медиация
    /// </summary>
    [Table("mediation_case_session_result")]
    [Comment("Резултати в среща за медиация")]
    public class MediationCaseSessionResult : UserDateWRT, IExpiredInfo
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
        /// Идентификатор на среща
        /// </summary>
        [Column("mediation_case_session_id")]
        [Comment("Идентификатор на среща")]
        public int MediationCaseSessionId { get; set; }

        /// <summary>
        /// Идентификатор на резултат от среща
        /// </summary>
        [Column("mediation_result_id")]
        [Comment("Идентификатор на резултат от среща")]
        public int MediationResultId { get; set; }

        /// <summary>
        /// Идентификатор на основание за резултат от среща
        /// </summary>
        [Column("mediation_result_base_id")]
        [Comment("Идентификатор на основание за резултат от среща")]
        public int? MediationResultBaseId { get; set; }

        /// <summary>
        /// Флаг за основен резултат
        /// </summary>
        [Column("is_main")]
        [Comment("Флаг за основен резултат")]
        public bool IsMain { get; set; }

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
        /// Резултат от среща
        /// </summary>
        [ForeignKey(nameof(MediationResultId))]
        public virtual MediationResult Result { get; set; }

        /// <summary>
        /// Основание за резултат от среща
        /// </summary>
        [ForeignKey(nameof(MediationResultBaseId))]
        public virtual MediationResultBase ResultBase { get; set; }

        /// <summary>
        /// Потребител, който е анулирал записа
        /// </summary>
        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }
    }
}
