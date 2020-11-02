// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Връзки към брояч за дела
    /// </summary>
    [Table("common_counter_cases")]
    public class CounterCase
    {
        
        [Column("counter_id")]
        public int CounterId { get; set; }

        [Column("case_group_id")]
        public int CaseGroupId { get; set; }

        [ForeignKey(nameof(CounterId))]
        public virtual Counter Counter { get; set; }

        [ForeignKey(nameof(CaseGroupId))]
        public virtual CaseGroup CaseGroup { get; set; }
    }
}
