// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Epep
{
    public class MQEpepVM
    {
        public long Id { get; set; }
        public int StateId { get; set; }
        public string OperName { get; set; }
        public DateTime DateWrt { get; set; }
        public DateTime? DateTransfered { get; set; }
        public string ErrorDescription { get; set; }
    }
}
