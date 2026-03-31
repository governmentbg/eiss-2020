// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Integration.Cais;
using Nest;
using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Documents
{
    public class DocumentContentElementVM
    {
        public int[] SourceTypes { get; set; }
        public long SourceId { get; set; }

        public string DirLabel { get; set; }
        public string DocumentTypeLabel { get; set; }
        public string DocumentNumber { get; set; }
        public DateTime DocumentDate { get; set; }
    }
}
