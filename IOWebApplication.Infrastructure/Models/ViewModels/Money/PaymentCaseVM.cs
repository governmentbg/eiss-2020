// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Money
{
    public class PaymentCaseVM
    {
        public int Id { get; set; }

        public string PersonNames { get; set; }

        public decimal AmountForCase { get; set; }

        public decimal AmountForPayment { get; set; }

        public string MoneyTypeNames { get; set; }

        public DateTime PaidDate { get; set; }

        public string PaymentTypeName { get; set; }
    }
}
