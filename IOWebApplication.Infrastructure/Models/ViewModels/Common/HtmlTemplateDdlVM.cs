using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class HtmlTemplateDdlVM
    {
        public string Value { get; set; }
        public string Alias { get; set; }
        public string Text { get; set; }
        public bool? HaveSessionAct { get; set; }
        public bool? HaveSessionActComplain { get; set; }
        public bool? RequiredSessionActComplain { get; set; }
        public bool? HaveExpertReport { get; set; }
        public bool? HaveMultiActComplain { get; set; }
        public bool? HaveDocumentSenderPerson { get; set; }
    }
}
