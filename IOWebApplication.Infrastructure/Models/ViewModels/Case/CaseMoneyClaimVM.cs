// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseMoneyClaimVM
    {
        public int Id { get; set; }
        public string CaseMoneyClaimGroupLabel { get; set; }
        public string CaseMoneyClaimTypeLabel { get; set; }
        public string ClaimNumber { get; set; }
        public DateTime? ClaimDate { get; set; }
        public string Description { get; set; }
        public string Motive { get; set; }

        public decimal FullSum { get; set; }
        public string FullSumText { get; set; }

        public virtual ICollection<CaseMoneyCollectionVM> CaseMoneyCollections { get; set; }
    }
}
