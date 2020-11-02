// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Основание за издаване на ИЛ по основен вид дело
    /// </summary>
    [Table("nom_exec_list_law_base_case_group")]
    public class ExecListLawBaseCaseGroup
    {
        [Column("exec_list_law_base_id")]
        public int ExecListLawBaseId { get; set; }

        [Column("case_group_id")]
        public int CaseGroupId { get; set; }

        [ForeignKey(nameof(ExecListLawBaseId))]
        public virtual ExecListLawBase ExecListLawBase { get; set; }

        [ForeignKey(nameof(CaseGroupId))]
        public virtual CaseGroup CaseGroup { get; set; }

    }
}
