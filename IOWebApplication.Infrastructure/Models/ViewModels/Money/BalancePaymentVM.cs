// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Money
{
    public class BalancePaymentVM
    {
        [Display(Name = " ")]
        public int Id { get; set; }

        public int MoneyGroupId { get; set; }

        [Display(Name = "Сума на плащането")]
        public decimal Amount { get; set; }

        [Display(Name = "Прихваната сума до момента")]
        public decimal AmountObligationPay { get; set; }

        [Display(Name = "Плащане")]
        public decimal AmountPay { get; set; }

        public string ObligationIds { get; set; }

        [Display(Name = "Дължима сума")]
        public decimal AmountForPay { get; set; }
    }
}
