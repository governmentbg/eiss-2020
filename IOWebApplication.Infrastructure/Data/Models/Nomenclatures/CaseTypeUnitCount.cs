// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Състави по точен вид дело
    /// </summary>
    [Table("nom_case_type_unit_count")]
    public class CaseTypeUnitCount
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("case_type_unit_id")]
        public int CaseTypeUnitId { get; set; }

        [Column("judge_role_id")]
        public int JudgeRoleId { get; set; }

        [Column("person_count")]
        public int PersonCount { get; set; }

        [ForeignKey(nameof(JudgeRoleId))]
        public virtual JudgeRole JudgeRole { get; set; }

        [ForeignKey(nameof(CaseTypeUnitId))]
        public virtual CaseTypeUnit CaseTypeUnit { get; set; }
    }
}
