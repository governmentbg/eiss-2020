// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Съдии към Съдебен състав
    /// </summary>
    [Table("common_court_department_lawunit")]
    public class CourtDepartmentLawUnit
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("department_id")]
        public int CourtDepartmentId { get; set; }

        [Column("lawunit_id")]
        [Display(Name = "Съдия")]
        public int LawUnitId { get; set; }

        [Column("judge_department_role_id")]
        [Display(Name = "Роля")]
        public int? JudgeDepartmentRoleId { get; set; }

        [Column("date_from")]
        [Display(Name = "Дата от")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime DateFrom { get; set; }

        [Column("date_to")]
        [Display(Name = "Дата до")]
        public DateTime? DateTo { get; set; }

        [ForeignKey(nameof(CourtDepartmentId))]
        public virtual CourtDepartment CourtDepartment { get; set; }

        [ForeignKey(nameof(LawUnitId))]
        public virtual LawUnit LawUnit { get; set; }

        [ForeignKey(nameof(JudgeDepartmentRoleId))]
        public virtual JudgeDepartmentRole JudgeDepartmentRole { get; set; }
    }
}
