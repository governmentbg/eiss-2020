using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Documents
{
    public class DocumentResolutionListVM
    {
        public long Id { get; set; }
        public string Label { get; set; }
        public string RegNumber { get; set; }
        public DateTime? RegDate { get; set; }
        public string ResolutionTypeLabel { get; set; }
    }
}
