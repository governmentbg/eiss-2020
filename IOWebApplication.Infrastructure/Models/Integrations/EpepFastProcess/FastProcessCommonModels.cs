// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

namespace IOWebApplication.Infrastructure.Models.Integrations.EpepFastProcess
{
    public class DocumentTaxCalcVM
    {
        public int MoneyFeeTypeId { get; set; }
        public decimal MaterialInterest { get; set; }
        public decimal MaterialInterestBGN { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TaxAmountBGN { get; set; }
    }
}
