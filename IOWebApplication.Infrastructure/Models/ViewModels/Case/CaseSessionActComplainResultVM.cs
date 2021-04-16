using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseSessionActComplainResultVM
    {
        public int Id { get; set; }
        public string CaseName { get; set; }
        public string CaseSessionActName { get; set; }
        public int CaseSessionActId { get; set; }
        public string ActResultLabel { get; set; }
        public DateTime? DateResult { get; set; }
    }
}
