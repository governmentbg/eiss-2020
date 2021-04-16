﻿using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseLoadCorrectionVM
    {
        public int Id { get; set; }
        public DateTime CorrectionDate { get; set; }
        public string CaseLoadCorrectionActivityLabel { get; set; }
        public decimal CorrectionLoadIndex { get; set; }
    }
}
