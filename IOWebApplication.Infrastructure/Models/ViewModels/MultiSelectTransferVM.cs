// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class MultiSelectTransferVM
    {
        public int Id { get; set; }
        public int Order { get; set; }

        public int Percent { get; set; }
        public string Text { get; set; }
    }
}
