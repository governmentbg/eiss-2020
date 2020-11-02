// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Основания за налагане на глоба
    /// </summary>
    [Table("nom_money_fine_type")]
    public class MoneyFineType : BaseCommonNomenclature
    {
        public virtual ICollection<MoneyFineCaseGroup> MoneyFineCaseGroups { get; set; }

        public MoneyFineType()
        {
            MoneyFineCaseGroups = new HashSet<MoneyFineCaseGroup>();
        }
    }
}
