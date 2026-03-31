// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Models.ViewModels.Report;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    /// <summary>
    /// Модел за редакция на шаблон на специализирана справка
    /// </summary>
    public class FilterTemplatesSpecializedReportEditVM
    {
        /// <summary>
        /// Идентификатор на записа
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Идентификатор на вид шаблон
        /// </summary>
        public int FilterTemplateTypeId { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        [Required(ErrorMessage = "Въведете {0}.")]
        [Display(Name = "Име на шаблон")]
        public string Label { get; set; }

        /// <summary>
        /// Флаг за активност на записа
        /// </summary>
        [Display(Name = "Активен")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Филтър за специализирана справка
        /// </summary>
        public SpecializedReportFilterVM SpecializedReportFilter { get; set; } = new SpecializedReportFilterVM();
    }
}
