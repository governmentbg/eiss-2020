// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CheckListVM
    {
        public bool Checked { get; set; }
        public string Value { get; set; }
        public string Label { get; set; }
        public string Warrning { get; set; }
    }
}
