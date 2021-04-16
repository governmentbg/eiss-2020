using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseLoadIndexSprVM
    {
        public int CaseId { get; set; }
        public string CaseName { get; set; }
        public int LawUnitId { get; set; }
        public string LawUnitName { get; set; }
        public decimal CalcValue { get; set; }
    }
}
