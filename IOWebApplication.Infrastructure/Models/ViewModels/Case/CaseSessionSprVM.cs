// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseSessionSprVM
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public string CaseGroupLabel { get; set; }
        public string CaseRegNum { get; set; }
        public DateTime CaseRegDate { get; set; }
        public string JudgeReport { get; set; }
        public DateTime? ActDeclaredDate { get; set; }
        public string SessionTypeLabel { get; set; }
        public DateTime SessionDateFrom { get; set; }
        public string SessionResultLabel { get; set; }
        public DateTime? SessionDateReturn { get; set; }
        public DateTime? SessionDateEntryIntoForce { get; set; }
        public string CasePersons { get; set; }
    }
}
