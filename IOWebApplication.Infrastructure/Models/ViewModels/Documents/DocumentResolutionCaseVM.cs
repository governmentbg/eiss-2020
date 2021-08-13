﻿// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Documents
{
    public class DocumentResolutionCaseVM
    {
        public long Id { get; set; }
        public int CaseId { get; set; }

        public string CaseTypeName { get; set; }
        public string CaseNumber { get; set; }
        public int CaseShortNumber { get; set; }
        public int CaseYear { get; set; }
    }
}
