// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseLoadIndexCourtGroupSprVM
    {
        public int LawUnitId { get; set; }
        public string LawUnitName { get; set; }
        public int? CourtId { get; set; }
        public string CourtLabel { get; set; }
        public int CourtGroupId { get; set; }
        public string CourtGroupLabel { get; set; }
        public decimal CalcValue { get; set; }
        public decimal CalcValueAll { get; set; }
    }
}
