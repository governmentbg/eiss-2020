// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Групиране на Видове суми - за справки и функционалности
    /// </summary>
    [Table("nom_money_type_grouping")]
    public class MoneyTypeGrouping
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("money_type_id")]
        public int MoneyTypeId { get; set; }

        [Column("money_type_group")]
        public int MoneyTypeGroup { get; set; }

        [ForeignKey(nameof(MoneyTypeId))]
        public virtual MoneyType MoneyType { get; set; }
    }
}
