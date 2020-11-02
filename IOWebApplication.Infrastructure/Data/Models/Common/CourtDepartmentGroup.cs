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
    /// Групи дела по СЪД към съдебно ниво
    /// </summary>
    [Table("common_court_department_group")]
    public class CourtDepartmentGroup
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("compartment_id")]
        public int CourtDepartmentId { get; set; }

        [Column("court_group_id")]
        public int CourtGroupId { get; set; }

        [Column("date_from")]
        [Display(Name = "Дата от")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime DateFrom { get; set; }

        [Column("date_to")]
        [Display(Name = "Дата до")]
        public DateTime? DateTo { get; set; }

        [ForeignKey(nameof(CourtDepartmentId))]
        public virtual CourtDepartment CourtDepartment { get; set; }

        [ForeignKey(nameof(CourtGroupId))]
        public virtual CourtGroup CourtGroup { get; set; }

    }
}
