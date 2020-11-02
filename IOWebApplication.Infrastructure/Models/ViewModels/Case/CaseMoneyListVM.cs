// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseMoneyListVM
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public int? CaseSessionId { get; set; }

        public string CaseLawUnitName { get; set; }

        public string MoneyTypeName { get; set; }

        public string MoneySignName { get; set; }

        public decimal Amount { get; set; }

        public DateTime? PaidDate { get; set; }

    }
}
