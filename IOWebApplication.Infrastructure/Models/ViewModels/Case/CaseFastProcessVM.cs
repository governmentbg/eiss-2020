// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseFastProcessVM
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public decimal TaxAmount { get; set; }
        public string TaxAmountString { get; set; }
        public string CurrencyLabel { get; set; }
        public string CurrencyCode { get; set; }
    }
}
