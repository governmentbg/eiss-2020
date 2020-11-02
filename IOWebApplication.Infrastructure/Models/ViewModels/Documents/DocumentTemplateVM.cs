// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Documents
{
    public class DocumentTemplateVM
    {
        public int Id { get; set; }
        public string DocumentTypeLabel { get; set; }
        public string AuthorName { get; set; }
        public string StateName { get; set; }
        public long? DocumentId { get; set; }
        public string DocumentNumber { get; set; }
        public DateTime DateWrt { get; set; }
    }
}
