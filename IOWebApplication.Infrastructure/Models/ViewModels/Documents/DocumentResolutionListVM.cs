// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Documents
{
    public class DocumentResolutionListVM
    {
        public long Id { get; set; }
        public string Label { get; set; }
        public string RegNumber { get; set; }
        public DateTime? RegDate { get; set; }
        public string ResolutionTypeLabel { get; set; }
    }
}
