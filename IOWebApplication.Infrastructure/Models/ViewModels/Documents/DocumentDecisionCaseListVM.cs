// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Documents
{
    public class DocumentDecisionCaseListVM
    {
        public long Id { get; set; }

        public string CaseRegNumber { get; set; }

        public DateTime CaseRegDate { get; set; }

        public string DecisionName { get; set; }
    }
}
