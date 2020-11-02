// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models
{
    public class LabelValueVM
    {
        public string Label { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        public bool Selected { get; set; }

        public LabelValueVM()
        {

        }

        public LabelValueVM(object value,string label)
        {
            this.Value = value.ToString();
            this.Label = label;
        }
    }
}
