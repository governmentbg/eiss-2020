using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Documents
{
    public class DocumentDecisionCaseListVM
    {
        public long Id { get; set; }
        public long DocumentId { get; set; }
        public string CaseRegNumber { get; set; }
        public DateTime CaseRegDate { get; set; }
        public string DecisionName { get; set; }
        public string DecisionRequestTypeName { get; set; }
        public string DocumentLable { get; set; }
        public string DocumentShortLable { get; set; }
    }
}
