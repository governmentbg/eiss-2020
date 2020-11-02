// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class CourtBankAccountVM
    {
        public int Id { get; set; }

        [Display(Name = "Етикет")]
        public string Label { get; set; }

        [Display(Name = "IBAN")]
        public string Iban { get; set; }

        [Display(Name = "Вид сметка")]
        public string MoneyGroupName { get; set; }

        [Display(Name = "Активен")]
        public bool IsActive { get; set; }

        [Display(Name = "COM port ПОС")]
        public string ComPortPos { get; set; }

    }
}
