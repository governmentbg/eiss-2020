// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CasePersonDocumentVM
    {
        public int Id { get; set; }
        public string IssuerCountryName { get; set; }
        public string PersonalDocumentTypeLabel { get; set; }
        public string DocumentNumber { get; set; }
        public DateTime DocumentDate { get; set; }
        public string IssuerName { get; set; }
    }
}
