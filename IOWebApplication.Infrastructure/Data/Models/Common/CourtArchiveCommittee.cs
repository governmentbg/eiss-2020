// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Комисии за архивните индекси
    /// </summary>
    [Table("common_court_archive_committee")]
    public class CourtArchiveCommittee
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("label")]
        [Display(Name = "Наименование")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        public string Label { get; set; }

        [Column("description")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Начална дата")]
        [Column("date_start")]
        public DateTime DateStart { get; set; }

        [Display(Name = "Крайна дата")]
        [Column("date_end")]
        public DateTime? DateEnd { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }
    }
}
