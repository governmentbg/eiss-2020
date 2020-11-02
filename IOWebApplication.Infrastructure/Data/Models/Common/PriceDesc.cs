// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    [Table("common_pricedesc")]
    public  class PriceDesc
    {
        public PriceDesc()
        {
            PriceCols = new HashSet<PriceCol>();
            PriceVals = new HashSet<PriceVal>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        [Display(Name = "Съд")]
        public int? CourtId { get; set; }

        [Column("keyword")]
        [Display(Name = "Код на цена")]
        public string Keyword { get; set; }

        [Column("name")]
        [Display(Name = "Наименование")]
        public string Name { get; set; }

        [Column("datefrom")]
        [Display(Name = "В сила от")]
        public DateTime DateFrom { get; set; }

        [Column("dateto")]
        [Display(Name = "В сила до")]
        public System.Nullable<DateTime> DateTo { get; set; }


        public virtual ICollection<PriceCol> PriceCols { get; set; }
        public virtual ICollection<PriceVal> PriceVals { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }
    }
}
