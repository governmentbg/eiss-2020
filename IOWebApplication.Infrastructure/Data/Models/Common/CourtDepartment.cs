// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Съдебни нива : Колегии, отделения, съд
    /// </summary>
    [Table("common_court_department")]
    public class CourtDepartment
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("case_group_id")]
        public int CaseGroupId { get; set; }

        [Column("master_id")]
        public int? MasterId { get; set; }

        [Column("parent_id")]
        [Display(Name = "Горно ниво")]
        [Range(0, int.MaxValue, ErrorMessage = "Изберете {0}.")]
        public int? ParentId { get; set; }

        [Column("label")]
        [Display(Name = "Наименование")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        public string Label { get; set; }

        [Column("description")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Column("department_type_id")]
        [Display(Name = "Ниво")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
        public int DepartmentTypeId { get; set; }

        [Column("date_from")]
        [Display(Name = "Дата от")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime DateFrom { get; set; }

        [Column("date_to")]
        [Display(Name = "Дата до")]
        public DateTime? DateTo { get; set; }

        [Column("case_instance_id")]
        [Display(Name = "Инстанция")]
        public int? CaseInstanceId { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(ParentId))]
        public virtual CourtDepartment ParentDepartment { get; set; }

        [ForeignKey(nameof(DepartmentTypeId))]
        public virtual DepartmentType DepartmentType { get; set; }

        [ForeignKey(nameof(CaseGroupId))]
        public virtual CaseGroup CaseGroup { get; set; }

        [ForeignKey(nameof(CaseInstanceId))]
        public virtual CaseInstance CaseInstance { get; set; }
    }
}
