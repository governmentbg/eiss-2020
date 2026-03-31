// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Таблица с подробни данни за оценка на медиатори към дело
    /// </summary>
    [Table("mediation_case_mediator_appraisal_data")]
    [Comment("Таблица с подробни данни за оценка на медиатори към дело")]
    public class MediationCaseMediatorAppraisalData
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        [Key]
        [Column("id")]
        [Comment("Идентификатор на запис")]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на оценка
        /// </summary>
        [Column("mediation_case_mediator_appraisal_id")]
        [Comment(" Идентификатор на оценка")]
        public int MediationCaseMediatorAppraisalId { get; set; }

        /// <summary>
        /// Идентификатор на точка за оценяване
        /// </summary>
        [Column("mediation_point_mediator_appraisal_id")]
        [Comment("Идентификатор на точка за оценяване")]
        public int MediationPointMediatorAppraisalId { get; set; }

        /// <summary>
        /// Оценка
        /// </summary>
        [Column("rating")]
        [Comment("Оценка")]
        public int Rating { get; set; }

        /// <summary>
        /// Оценка
        /// </summary>
        [ForeignKey(nameof(MediationCaseMediatorAppraisalId))]
        public virtual MediationCaseMediatorAppraisal Appraisal { get; set; }

        /// <summary>
        /// Оценка
        /// </summary>
        [ForeignKey(nameof(MediationPointMediatorAppraisalId))]
        public virtual MediationPointMediatorAppraisal PointAppraisal { get; set; }
    }
}
