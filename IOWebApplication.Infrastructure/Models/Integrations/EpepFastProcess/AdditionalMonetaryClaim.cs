// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;

namespace IOWebApplication.Infrastructure.Models.Integrations.EpepFastProcess
{
    public class AdditionalMonetaryClaim
    {
        public string GUID { get; set; }
        public string CurrencyName { get; set; }
        public string CurrencyCode { get; set; }
        public decimal AmountInNumbers { get; set; }
        public string AmountInWords { get; set; }
        public decimal InterestInNumbersBGNEquivalent { get; set; }
        public string InterestInWordsBGNEquivalent { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public ClaimDestribution[] ClaimDestributions { get; set; }
        public string Description { get; set; }
    }
}
