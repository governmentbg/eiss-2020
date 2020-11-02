// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class CourtLawUnitVM
    {
        public int Id { get; set; }
        public int LawUnitId { get; set; }

        public int LawUnitTypeId { get; set; }
        public string LawUnitTypeLabel { get; set; }
        public string CourtLabel { get; set; }
        public string LawUnitName { get; set; }
        public string CourtOrganizationName { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string LawUnitPositionName { get; set; }
        public string PeriodTypeLabel { get; set; }
        public int PeriodTypeId { get; set; }
    }

    public class CourtLawUnitFilter
    {
        [Display(Name = "Длъжностно лице")]
        public int LawUnitId { get; set; }

        [Display(Name = "Вид")]
        public int PeriodTypeId { get; set; }

        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Заседател")]
        public int? LawUnitJuryId { get; set; }
    }
}
