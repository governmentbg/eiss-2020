// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Подшифри в дело
    /// </summary>
    [Table("nom_case_code_sub")]
    [Comment("Подшифри в дело")]
    public class CaseCodeSub : BaseCommonNomenclature
    {
        /// <summary>
        /// Идентификатор на шифър
        /// </summary>
        [Column("case_code_id")]
        [Comment("Идентификатор на шифър")]
        public int? CaseCodeId { get; set; }

        /// <summary>
        /// Шифър
        /// </summary>
        [ForeignKey(nameof(CaseCodeId))]
        public virtual CaseCode CaseCode { get; set; }
    }
}
