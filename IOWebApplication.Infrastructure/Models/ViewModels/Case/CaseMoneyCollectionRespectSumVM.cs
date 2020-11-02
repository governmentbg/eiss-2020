// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseMoneyCollectionRespectSumVM
    {
        public int CaseId { get; set; }
        public int Id { get; set; }
        public string Label { get; set; }
        public decimal Value { get; set; }

        public virtual List<CasePersonListDecimalVM> CasePersonListDecimals { get; set; }
        public virtual List<CaseMoneyCollectionRespectSumVM> CaseMoneyCollectionRespectSumOthers { get; set; }
    }
}
