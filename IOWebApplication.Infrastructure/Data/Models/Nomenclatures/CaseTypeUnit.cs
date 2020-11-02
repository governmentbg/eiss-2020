// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Състави по точен вид дело
    /// </summary>
    [Table("nom_case_type_unit")]
    public class CaseTypeUnit : BaseCommonNomenclature
    {
        [Column("case_type_id")]
        public int CaseTypeId { get; set; }

        [ForeignKey(nameof(CaseTypeId))]
        [Display(Name = "Точен вид дело")]
        public virtual CaseType CaseType { get; set; }

        public virtual ICollection<CaseTypeUnitCount> CaseTypeUnitCounts { get; set; }
    }
}
