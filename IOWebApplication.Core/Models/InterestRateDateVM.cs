// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Core.Models
{
    public class InterestRateDateVM
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Rate { get; set; }
    }

    public class FilterInterestRate
    {
        [Display(Name = "Вид процент")]
        public int? InterestType { get; set; }

        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }
    }
}
