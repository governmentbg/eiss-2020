// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид суми по дело
    /// </summary>
    [Table("nom_money_type")]
    public class MoneyType : BaseCommonNomenclature
    {
        /// <summary>
        /// 1:Приход,-1:Разход
        /// </summary>
        [Column("money_sign")]
        public int MoneySign { get; set; }
        
        [Column("money_group_id")]
        public int MoneyGroupId { get; set; }

        public bool? NoMoney { get; set; }

        [ForeignKey(nameof(MoneyGroupId))]
        public virtual MoneyGroup MoneyGroup { get; set; }
    }
}
