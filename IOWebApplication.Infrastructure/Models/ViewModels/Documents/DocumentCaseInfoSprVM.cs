using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Documents
{
    public class DocumentCaseInfoSprVM
    {
        public string DocumentNumberYear { get; set; }
        public DateTime? DocumentDate { get; set; }
        public string DocumentTypeLabel { get; set; }
        public string CaseInfo { get; set; }
        public int? CaseId { get; set; }
        public bool? IsCase { get; set; }
        public string CaseDocumentInfo { get; set; }
        public string CaseCodeLabel { get; set; }
        public string CaseSessionInfo { get; set; }
    }
}
