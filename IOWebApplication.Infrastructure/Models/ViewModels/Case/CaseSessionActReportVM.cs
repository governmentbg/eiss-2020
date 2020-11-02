// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseSessionActReportVM
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public string ActRegNumYear { get; set; }
        public string ActTypeLabel { get; set; }
        public DateTime? RegDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public DateTime? ActInforcedDate { get; set; }
        public string CaseActInfoLabel { get; set; }
        public string DocumentInfo { get; set; }
        public string ActStateName { get; set; }
    }
}
