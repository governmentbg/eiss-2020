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
    /// Периоди за изчисляване на натовареност
    /// </summary>
    [Table("common_court_load_period")]
    public class CourtLoadPeriod
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_load_reset_period_id")]
        public int CourtLoadResetPeriodId { get; set; }

        [Column("court_group_id")]
        public int? CourtGroupId { get; set; }

        [Column("court_duty_id")]
        public int? CourtDutyId { get; set; }

        [Column("date_from")]
        [Display(Name = "Дата от")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime DateFrom { get; set; }

        [Column("date_to")]
        [Display(Name = "Дата до")]
        public DateTime? DateTo { get; set; }

        [ForeignKey(nameof(CourtLoadResetPeriodId))]
        public virtual CourtLoadResetPeriod CourtLoadResetPeriod { get; set; }

        [ForeignKey(nameof(CourtGroupId))]
        public virtual CourtGroup CourtGroup { get; set; }

        [ForeignKey(nameof(CourtDutyId))]
        public virtual CourtDuty CourtDuty { get; set; }
    }
}
