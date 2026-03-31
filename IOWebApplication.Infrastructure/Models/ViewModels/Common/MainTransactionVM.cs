// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class MainTransactionVM
    {
        public string Title { get; set; }
        public int SourceType { get; set; }
        public int WaitingCount { get; set; }
        public long ContinueSourceId { get; set; }

        public string ContinueUrl { get; set; }
    }
}
