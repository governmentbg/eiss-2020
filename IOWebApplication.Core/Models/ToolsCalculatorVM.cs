// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Core.Models
{
    public class ToolsCalculatorVM
    {
        [Display(Name = "Претендирани вземания")]
        public decimal ExpenseClaimTotal { get; set; }

        [Display(Name = "Претендирани разноски")]
        public decimal ExpenseClaimExpense { get; set; }

        [Display(Name = "Уважени вземания")]
        public decimal ExpenseComfirmedTotal { get; set; }




        [Display(Name = "База")]
        public decimal LegalInterestAmount { get; set; }

        [Display(Name = "От дата")]
        public DateTime LegalInterestDateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime LegalInterestDateTo { get; set; }
    }
}
