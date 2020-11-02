// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Основание за образуване на делото
    /// </summary>
    [Table("nom_case_reason")]
    public class CaseReason : BaseCommonNomenclature
    {        
        [Column("case_group_id")]
        [Display(Name ="Основен вид дело")]
        public int CaseGroupId { get; set; }       

        [ForeignKey(nameof(CaseGroupId))]
        public virtual CaseGroup CaseGroup { get; set; }
    }
}
