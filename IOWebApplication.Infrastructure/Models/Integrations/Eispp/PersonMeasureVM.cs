// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.Integrations.Eispp
{
    public class PersonMeasureVM
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public bool IsChecked { get; set; }
    }
}
