// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class CounterVM
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public string CounterTypeName { get; set; }
        public int CounterTypeId { get; set; }
        public string ResetTypeName { get; set; }

        public int CurrentValue { get; set; }
    }
}
