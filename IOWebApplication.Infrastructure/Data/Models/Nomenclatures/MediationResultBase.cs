// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Основания за резултати от срещи за медиация
    /// </summary>
    [Table("nom_mediation_result_base")]
    [Comment("Основания за резултати от срещи за медиация")]
    public class MediationResultBase : BaseCommonNomenclature
    {
        /// <summary>
        /// Идентификатор на група
        /// </summary>
        [Column("mediation_result_group_id")]
        [Comment("Идентификатор на група")]
        public int? MediationResultGroupId { get; set; }

        /// <summary>
        /// Група
        /// </summary>
        [ForeignKey(nameof(MediationResultGroupId))]
        [Comment("Група")]
        public virtual MediationResultGroup Group { get; set; }
    }
}
