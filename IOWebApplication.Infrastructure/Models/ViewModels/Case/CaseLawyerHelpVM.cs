using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseLawyerHelpVM
    {
        public int Id { get; set; }

        public int? CourtId { get; set; }

        public int CaseId { get; set; }
        public string LawyerHelpBaseText { get; set; }
        public string LawyerHelpTypeText { get; set; }
        public string CaseSessionActText { get; set; }
    }
}
