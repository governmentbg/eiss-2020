// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures
{
    public class CaseLoadCorrectionActivityVM
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public string CaseGroupLabel { get; set; }
        public string CaseInstanceLabel { get; set; }
        public int? CorrectionGroup { get; set; }
        public decimal LoadIndex { get; set; }
    }
}
