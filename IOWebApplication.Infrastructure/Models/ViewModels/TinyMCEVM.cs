using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class TinyMCEVM
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string Style { get; set; }
        public int SourceType { get; set; }
        public int SourceId { get; set; }
        public int PageOrientation { get; set; }
        public bool SmartShrinkingPDF { get; set; }
        public string Title { get; set; }
    }
}
