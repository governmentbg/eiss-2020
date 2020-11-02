// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseLoadIndexVM
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public int? CaseSessionId { get; set; }
        public int LawUnitId { get; set; }
        public string LawUnitName { get; set; }
        public string NameActivity { get; set; }
        public decimal BaseIndex { get; set; }
        public string LoadValue { get; set; }
        public decimal CalcValue { get; set; }
        public bool IsMainActivity { get; set; }
    }
}
