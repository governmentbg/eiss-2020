using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class TinyMCEVM
    {
        public int Id { get; set; }
        [AllowHtml]
        public string Text { get; set; }
        [AllowHtml]
        public string Style { get; set; }
        [AllowHtml]
        public string DocStyle { get; set; }
        [AllowHtml]
        public string ItemsJson { get; set; }
        public int SourceType { get; set; }
        public int SourceId { get; set; }
        public int PageOrientation { get; set; }
        public bool SmartShrinkingPDF { get; set; }
        public string Title { get; set; }
    }
}
