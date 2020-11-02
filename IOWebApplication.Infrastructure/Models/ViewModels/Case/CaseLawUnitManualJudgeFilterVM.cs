// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseLawUnitManualJudgeFilterVM
    {
        [Display(Name = "Номер дело")]
        public string CaseNumber { get; set; }

        [Display(Name = "Заместване от дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Съдия")]
        public string LawUnitName { get; set; }
    }
}
