// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Money
{
    public class PaymentListVM
    {
        public int Id { get; set; }

        [Display(Name = "Номер")]
        public string PaymentNumber { get; set; }

        [Display(Name = "Вид сметка")]
        public string MoneyGroupName { get; set; }

        [Display(Name = "Сума")]
        public decimal Amount { get; set; }

        [Display(Name = "Прихваната сума")]
        public decimal AmountPayObligation { get; set; }

        public decimal AmountFree { get { return this.Amount - this.AmountPayObligation; } }

        [Display(Name = "Платено на")]
        public DateTime PaidDate { get; set; }

        [Display(Name = "Вносител")]
        public string SenderName { get; set; }

        [Display(Name = "Активен")]
        public bool IsActive { get; set; }

        [Display(Name = "Авансово плащане")]
        public bool IsAvans { get; set; }

        [Display(Name = "Начин на плащане")]
        public string PaymentTypeName { get; set; }

        [Display(Name = "Потребител")]
        public string UserName { get; set; }

        [Display(Name = "Дело")]
        public string CaseNumbers { get; set; }
    }
}
