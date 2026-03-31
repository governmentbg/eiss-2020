// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseLawyerHelpAssignedLawyerVM
    {
        public int Id { get; set; }
        public string Lawyer { get; set; }
        public string People { get; set; }
        public string LawyerStateLabel { get; set; }
        public bool IsEdit { get; set; }
        public bool IsFinish { get; set; }
        public string CaseName { get; set; }
        public int CaseId { get; set; }
    }
}
