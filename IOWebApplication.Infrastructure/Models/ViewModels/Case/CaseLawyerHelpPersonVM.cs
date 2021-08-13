using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseLawyerHelpPersonVM
    {
        public int Id { get; set; }
        public string CasePersonText { get; set; }
        public string AssignedLawyerText { get; set; }
    }
}
