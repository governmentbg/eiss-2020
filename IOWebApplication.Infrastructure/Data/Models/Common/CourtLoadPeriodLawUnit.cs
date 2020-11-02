// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Натовареност по съдии, в период за отчитане на натовареност
    /// </summary>
    [Table("common_court_load_period_lawunit")]
    public class CourtLoadPeriodLawUnit
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_load_period_id")]
        public int CourtLoadPeriodId { get; set; }

        [Column("lawunit_id")]
        public int? LawUnitId { get; set; }

        /// <summary>
        /// Само дата, без час
        /// </summary>
        [Column("selection_date")]
        public DateTime SelectionDate { get; set; }

        /// <summary>
        /// Наличен за деня
        /// </summary>
        [Column("is_available")]
        public bool IsAvailable { get; set; }

        [Column("day_cases")]
        public decimal DayCases { get; set; }

        [Column("average_cases")]
        public decimal AverageCases { get; set; }

        [Column("total_day_cases")]
        public decimal TotalDayCases { get; set; }

        [Column("load_index")]
        public decimal LoadIndex { get; set; }

        [ForeignKey(nameof(CourtLoadPeriodId))]
        public virtual CourtLoadPeriod CourtLoadPeriod { get; set; }

        [ForeignKey(nameof(LawUnitId))]
        public virtual LawUnit LawUnit { get; set; }
    }
}
