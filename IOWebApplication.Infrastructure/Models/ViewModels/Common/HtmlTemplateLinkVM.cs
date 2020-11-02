// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class HtmlTemplateLinkVM
    {
        public int Id { get; set; }
        public int HtmlTemplateId { get; set; }
        public string CourtTypeLabel { get; set; }
        public string CaseGroupLabel { get; set; }
        public string IsActiveLabel { get; set; }

        public int SourceType { get; set; }

        public string SourceTypeLabel
        {
            get
            {
                return this.SourceType > 0 ? SourceTypeSelectVM.GetSourceTypeName(this.SourceType) : "Всички";
            }
        }
    }
}
