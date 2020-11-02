// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseLifecycleVM
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public string LifecycleTypeLabel { get; set; }
        public int LifecycleTypeId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int Iteration { get; set; }
        public string IterationText { get; set; }
        public int DurationMonths { get; set; }
        public bool ModelEdit { get; set; }
    }
}
