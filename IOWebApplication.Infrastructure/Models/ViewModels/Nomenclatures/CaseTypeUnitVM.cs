// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures
{
    public class CaseTypeUnitVM
    {
        public int Id { get; set; }
        public int OrderNumber { get; set; }
        public string Label { get; set; }
        public bool IsActive { get; set; }
        public string IsActiveLabel { get; set; }
        public DateTime DateStart { get; set; }

        public IEnumerable<ListNumberVM> Counts { get; set; }
    }
}
