// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseEvidenceVM
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public string CaseName { get; set; }
        public string RegNumber { get; set; }
        public string FileNumber { get; set; }
        public DateTime DateAccept { get; set; }
        public DateTime DateWrt { get; set; }
        public string Description { get; set; }
        public string AddInfo { get; set; }
        public string EvidenceStateLabel { get; set; }
        public string EvidenceTypeLabel { get; set; }
    }
}
