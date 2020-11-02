// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class CourtLawUnitGroupVM
    {
        public int CourtLawUnitId { get; set; }

        public int LawUnitId { get; set; }

        public string LawUnitName { get; set; }

        public int PeriodTypeId { get; set; }

        [Display(Name = "Основен вид делo")]
        public int CaseGroupId { get; set; }

        public int LawUnitTypeId { get; set; }

    }
}
