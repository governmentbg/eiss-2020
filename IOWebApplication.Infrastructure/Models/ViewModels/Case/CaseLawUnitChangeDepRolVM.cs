// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseLawUnitChangeDepRolVM
    {
        public int CaseId { get; set; }
        public int? CaseSessionId { get; set; }

        [Display(Name = "Председател")]
        public int? CaseLawUnitId { get; set; }

        [Display(Name = "Състав")]
        public int? DepartmentId { get; set; }
    }
}
