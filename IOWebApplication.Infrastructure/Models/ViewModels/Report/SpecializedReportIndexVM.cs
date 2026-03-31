// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    /// <summary>
    /// Модел за страница за преглед на извлечени данни за справка от заявка 13
    /// </summary>
    public class SpecializedReportIndexVM
    {
        /// <summary>
        /// Шаблон
        /// </summary>
        [Display(Name = "Шаблони")]
        public int FilterTemplatesId { get; set; }

        /// <summary>
        /// Филтър за справка от заявка 13
        /// </summary>
        public SpecializedReportFilterVM Filter { get; set; } = new SpecializedReportFilterVM();
    }
}
