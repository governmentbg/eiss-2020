// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;

namespace IOWebApplication.Core.Models.BreadcrumbsModels
{
    public class BCCaseModel
    {
        public string CaseTypeCode { get; set; }
        public int CaseStateId { get; set; }
        public string ShortNumber { get; set; }
        public DateTime RegDate { get; set; }
    }
}
