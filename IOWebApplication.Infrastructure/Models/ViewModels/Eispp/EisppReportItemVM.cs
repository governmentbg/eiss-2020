// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Eispp
{
    public class EisppReportItemVM
    {
        public int EventType { get; set; }
        [Display(Name = "Събитие")]
        public string Label { get; set; }
        [Display(Name = "Успешно трансферирани")]
        public int? CountOK { get; set; }
        [Display(Name = "Успешно трансферирани OБЩО")]
        public int? CountOkAll { get; set; }
        [Display(Name = "Нетрансферирани")]
        public int? CountErr { get; set; }
    }
}
