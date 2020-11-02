// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид допълнително парично вземане: лихва, такса, друго
    /// </summary>
    [Table("nom_case_money_collection_kind")]
    public class CaseMoneyCollectionKind : BaseCommonNomenclature
    {
        [Column("case_money_collection_group_id")]
        public int CaseMoneyCollectionGroupId { get; set; }

        [ForeignKey(nameof(CaseMoneyCollectionGroupId))]
        public virtual CaseMoneyCollectionGroup CaseMoneyCollectionGroup { get; set; }
    }
}
