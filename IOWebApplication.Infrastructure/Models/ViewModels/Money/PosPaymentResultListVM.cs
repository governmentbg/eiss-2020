// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Money
{
    public class PosPaymentResultListVM
    {
        public int Id { get; set; }

        public string CourtBankAccountName { get; set; }
        public string MoneyGroupName { get; set; }

        public decimal Amount { get; set; }

        public DateTime PaidDate { get; set; }

        public string SenderName { get; set; }
    }
}
