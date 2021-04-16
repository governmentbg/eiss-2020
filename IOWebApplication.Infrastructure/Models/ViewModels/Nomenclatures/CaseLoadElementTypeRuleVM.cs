using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures
{
    public class CaseLoadElementTypeRuleVM
    {
        public int Id { get; set; }
        public string SessionTypeLabel { get; set; }
        public string SessionResultLabel { get; set; }
        public string ActTypeLabel { get; set; }
        public string IsCreateMotiveText { get; set; }
        public string IsCreateCaseText { get; set; }
    }
}
