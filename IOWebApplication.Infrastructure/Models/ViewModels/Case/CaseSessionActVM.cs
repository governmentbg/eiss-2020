// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseSessionActVM
    {
        public int Id { get; set; }
        public int CaseSessionId { get; set; }
        public int CaseId { get; set; }
        public string CaseLabel { get; set; }
        public string CaseSessionLabel { get; set; }
        public string ActTypeLabel { get; set; }
        public string ActResultLabel { get; set; }
        public string ActStateLabel { get; set; }
        public string RegNumber { get; set; }
        public DateTime? RegDate { get; set; }
        public DateTime DateWrt { get; set; }
        public bool IsFinalDoc { get; set; }
        public string EcliCode { get; set; }
        public string Description { get; set; }

    }
}
