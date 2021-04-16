using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Documents
{
    public class DocumentInfoVM
    {
        public long Id { get; set; }
        public int DirectionId { get; set; }
        public string Title { get; set; }
        public bool IsSecret { get; set; }
        public bool IsRestriction { get; set; }
        public DateTime? DocumentDate { get; set; }
        public virtual ICollection<DocumentResolutionListVM> DocumentResolutions { get; set; }
    }
}
