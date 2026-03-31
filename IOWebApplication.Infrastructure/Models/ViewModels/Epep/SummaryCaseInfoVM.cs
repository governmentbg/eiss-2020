// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Epep
{
    public class SummaryCaseInfoVM
    {
        public Integration.Epep.SummaryCase Case { get; set; }
        public DateTime DateWrt { get; set; }
        public int MigrationId { get; set; }
    }
}
