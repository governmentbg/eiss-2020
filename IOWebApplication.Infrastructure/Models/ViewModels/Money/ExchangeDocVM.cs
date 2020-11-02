// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Money
{
    public class ExchangeDocVM
    {
        public int Id { get; set; }

        [Display(Name = "Номер")]
        public string RegNumber { get; set; }

        [Display(Name = "Дата")]
        public DateTime? RegDate { get; set; }

        [Display(Name = "Активен")]
        public bool IsActive { get; set; }

        [Display(Name = "ТД на НАП")]
        public string InstitutionName { get; set; }

        [Display(Name = "Сума")]
        public decimal Amount { get; set; }
    }

    public class ExchangeDocFilterVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Номер")]
        public string RegNumber { get; set; }

        [Display(Name = "ТД на НАП")]
        public int InstitutionId { get; set; }
    }
}
