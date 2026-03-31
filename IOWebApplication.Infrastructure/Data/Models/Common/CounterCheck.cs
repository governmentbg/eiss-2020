// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    [Table("common_counter_check")]
    public class CounterCheck
    {
        [Column("source_type")]
        public int SourceType { get; set; }

        [MaxLength(50)]
        [Column("counter_value")]
        public string CouterValue { get; set; }

        [Column("date_check")]
        public DateTime DateCheck { get; set; }
    }
}
