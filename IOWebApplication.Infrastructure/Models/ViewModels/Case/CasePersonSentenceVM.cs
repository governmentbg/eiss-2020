using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CasePersonSentenceVM
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public string InstanceLabel { get; set; }
        public string CourtLabel { get; set; }
        public string CasePersonName { get; set; }
        public string CaseSessionActLabel { get; set; }
        public string SentenceResultTypeLabel { get; set; }
        public bool? IsActive { get; set; }
        public string IsActiveText { get; set; }

    }
}
