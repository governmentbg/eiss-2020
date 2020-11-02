// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseSessionFastDocumentVM
    {
        public int Id { get; set; }
        public string CasePersonName { get; set; }
        public string SessionDocTypeLabel { get; set; }
        public string SessionDocStateLabel { get; set; }
        public DateTime DateSession { get; set; }
        public DateTime? CaseSessionFastDocumentInitDateSession { get; set; }
    }
}
