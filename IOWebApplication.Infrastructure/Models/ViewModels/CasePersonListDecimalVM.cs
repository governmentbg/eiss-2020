// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CasePersonListDecimalVM
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public string LabelOne { get; set; }
        public decimal ValueOne { get; set; }
        public string LabelTwo { get; set; }
        public decimal ValueTwo { get; set; }
    }
}
