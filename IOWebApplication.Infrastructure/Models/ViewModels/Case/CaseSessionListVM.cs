// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseSessionListVM
    {
        public int Id { get; set; }
        public DateTime DateFrom { get; set; }
        public string SessionTypeLabel { get; set; }
        public string CourtHallName { get; set; }
        public string SessionStateLabel { get; set; }
        public string SessionResultLabel { get; set; }
        public string ActComplainResultLabel { get; set; }
    }
}
