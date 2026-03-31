// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Delivery
{
    public class DeliveryLogVM
    {
        public string PageLabel { get; set; }
        public string PageUrl { get; set; }
        public string Action { get; set; }
        public bool? IsFromMobile { get; set; }
    }
}
