// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class ExcelReportTemplateVM
    {
        public int Id { get; set; }

        public string CourtTypeLabel { get; set; }

        [Display(Name = "Наименование")]
        public string Label { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Дата от")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime DateFrom { get; set; }

        [Display(Name = "Дата до")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Файл")]
        public string FileName { get; set; }

        public string ReportTypeLabel { get; set; }
    }
}
