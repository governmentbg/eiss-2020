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
    /// Шифри по Групи дела по СЪД
    /// </summary>
    [Table("common_court_group_code")]
    public class CourtGroupCode 
    {
        [Column("court_group_id")]
        public int CourtGroupId { get; set; }

        [Column("case_code_id")]
        public int CaseCodeId { get; set; }

        [Column("date_from")]
        [Display(Name = "Дата от")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime DateFrom { get; set; }

        [Column("date_to")]
        [Display(Name = "Дата до")]
        public DateTime? DateTo { get; set; }

        [ForeignKey(nameof(CourtGroupId))]
        public virtual CourtGroup CourtGroup { get; set; }

        [ForeignKey(nameof(CaseCodeId))]
        public virtual CaseCode CaseCode { get; set; }

    }
}
