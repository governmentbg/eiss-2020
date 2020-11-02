// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IOWebApplication.Infrastructure.Data.Models.Base;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{  /// <summary>
   /// Съдебен район
   /// </summary>
    [Table("common_court_region")]
    public class CourtRegion : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("parent_id")]
        [Display(Name = "Горно ниво")]
        public int? ParentId { get; set; }

        [Column("label")]
        [Display(Name = "Наименование")]
        public string Label { get; set; }

        [Display(Name = "Активен")]
        [Column("is_active")]
        public bool IsActive { get; set; }

        [ForeignKey(nameof(ParentId))]
        public virtual CourtRegion ParentRegion { get; set; }
       
        public virtual ICollection<CourtRegionArea> Areas { get; set; }
    }
}
