// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Връзки точен вид дело към шифри
    /// </summary>
    [Table("nom_case_type_code")]
    public class CaseTypeCode
    {
        [Column("case_type_id")]
        public int CaseTypeId { get; set; }

        [Column("case_code_id")]
        public int CaseCodeId { get; set; }

        [ForeignKey(nameof(CaseTypeId))]
        [Display(Name = "Точен вид дело")]
        public virtual CaseType CaseType { get; set; }

        [ForeignKey(nameof(CaseCodeId))]
        [Display(Name = "Шифър")]
        public virtual CaseCode CaseCode { get; set; }
    }
}
