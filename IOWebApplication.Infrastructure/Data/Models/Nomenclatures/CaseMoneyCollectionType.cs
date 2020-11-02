// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Точен вид на вземането: Парично вземане: главница, друг вид вземане
    /// </summary>
    [Table("nom_case_money_collection_type")]
    public class CaseMoneyCollectionType : BaseCommonNomenclature
    {
        [Column("case_money_collection_group_id")]
        public int CaseMoneyCollectionGroupId { get; set; }

        [ForeignKey(nameof(CaseMoneyCollectionGroupId))]
        public virtual CaseMoneyCollectionGroup CaseMoneyCollectionGroup { get; set; }
    }
}
