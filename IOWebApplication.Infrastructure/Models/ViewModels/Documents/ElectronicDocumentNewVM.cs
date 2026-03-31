// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Documents
{
    public class ElectronicDocumentNewVM
    {
        public long Id { get; set; }
        public string DocumentKind { get; set; }
        public string DocumentGroup { get; set; }
        public string EpepUserName { get; set; }
        public string ApplyNumber { get; set; }
        public DateTime ApplyDate { get; set; }
        public string EditBy { get; set; }
    }
}
