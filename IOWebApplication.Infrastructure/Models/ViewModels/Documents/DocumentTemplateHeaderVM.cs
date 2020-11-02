// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Documents
{
    public class DocumentTemplateHeaderVM
    {
        public int Id { get; set; }
        public int? CaseId { get; set; }
        public long? DocumentId { get; set; }
        public string DocumentNumber { get; set; }
        public string DocumentReccipientName { get; set; }
        public string DocumentReccipientAddress { get; set; }
        public DateTime DocumentDate { get; set; }
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string JudgeName { get; set; }
        public string CourtName { get; set; }
        public string CourAddress { get; set; }
        public string DocumentTypeLabel { get; set; }
        public string HeaderTemplateName { get; set; }
        public int HtmlTemplateTypeId { get; set; }
        public string CourtLogo { get; set; }
    }
}
