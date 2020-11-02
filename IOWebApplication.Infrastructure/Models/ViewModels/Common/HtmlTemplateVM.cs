// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class HtmlTemplateVM
    {
        public int Id { get; set; }
        public string HtmlTemplateTypeLabel { get; set; }
        public string Alias { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public byte[] Content { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public bool? IsCreate { get; set; }
    }
}
