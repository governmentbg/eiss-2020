using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class BlankEditVM
    {
        public int SourceType { get; set; }
        public string SourceId { get; set; }
        public string ReturnUrl { get; set; }
        public string Title { get; set; }
        public string TemplateStyle { get; set; }
        public string HtmlHeader { get; set; }
        public string HtmlContent { get; set; }
        public string FooterTitle { get; set; }
        public string HtmlFooter { get; set; }
        public bool FooterIsEditable { get; set; }
        public bool FooterIsHtml { get; set; }
        public bool HasResetButton { get; set; }
        public bool HasPreviewButton { get; set; }
        public bool AppendWatermarkforTest { get; set; }

        public BlankEditVM()
        {
            AppendWatermarkforTest = true;
            FooterIsHtml = false;
            HasResetButton = false;
        }
    }
}
