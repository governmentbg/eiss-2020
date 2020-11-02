// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    [Table("common_pricecol")]
    public class PriceCol
    {
        public PriceCol()
        {
            PriceVals = new HashSet<PriceVal>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("pricedesc_id")]
        public int PriceDescId { get; set; }

        [Column("coltype")]
        [Display(Name = "Вид колона")]
        public int ColType { get; set; }

        [Column("name")]
        [Display(Name = "Наименование")]
        public string Name { get; set; }

        [Column("col_no")]
        [Display(Name = "Номер на колона/зона")]
        public Nullable<int> ColNo { get; set; }

        [Column("order_by")]
        public Nullable<int> OrderBy { get; set; }

        [Column("active")]
        [Display(Name = "Активна колона")]
        public bool Active { get; set; }

        [ForeignKey(nameof(PriceDescId))]
        public virtual PriceDesc PriceDesc { get; set; }

        public virtual ICollection<PriceVal> PriceVals { get; set; }
    }
}
