// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Модел за шаблони за филтър на справка
    /// </summary>
    [Table("common_filter_templates")]
    public class FilterTemplates: UserDateWRT
    {
        /// <summary>
        /// Идентификатор на записа
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на вид шаблон
        /// </summary>
        [Column("filter_template_type_id")]
        public int FilterTemplateTypeId { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        [Column("label")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public string Label { get; set; }

        /// <summary>
        /// Идентификатор на съд
        /// </summary>
        [Column("court_id")]
        public int? CourtId { get; set; }

        /// <summary>
        /// Идентификатор за потребител
        /// </summary>
        [Column("for_user_id")]
        public string ForUserId { get; set; }

        /// <summary>
        /// Флаг за активност на записа
        /// </summary>
        [Column("is_active")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Модел за филтър
        /// </summary>
        [Column("data", TypeName = "jsonb")]
        public string Data { get; set; }

        /// <summary>
        /// Вид шаблон
        /// </summary>
        [ForeignKey(nameof(FilterTemplateTypeId))]
        public virtual FilterTemplateType FilterTemplateType { get; set; }

        /// <summary>
        /// Съд
        /// </summary>
        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        /// <summary>
        /// За потребител
        /// </summary>
        [ForeignKey(nameof(ForUserId))]
        public virtual ApplicationUser ForUser { get; set; }
    }
}
