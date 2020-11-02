// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class HtmlTemplateParamLinkVM
    {
        public int Id { get; set; }
        public string HtmlTemplateFile { get; set; }
        public string HtmlTemplateName { get; set; }
        public string HtmlTemplateParamLabel { get; set; }
        public string HtmlTemplateParamDescr { get; set; }
    }
}
