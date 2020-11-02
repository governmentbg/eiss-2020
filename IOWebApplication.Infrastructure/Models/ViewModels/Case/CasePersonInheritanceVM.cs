// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CasePersonInheritanceVM
    {
        public int Id { get; set; }

        [Display(Name = "Постановена от")]
        public string CourtLabel { get; set; }
        
        [Display(Name = "Акт")]
        public string CaseSessionActLabel { get; set; }

        [Display(Name = "Резултат")]
        public string CasePersonInheritanceResultLabel { get; set; }
        
        [Display(Name = "Активна")]
        public string IsActiveText { get; set; }
    }
}
