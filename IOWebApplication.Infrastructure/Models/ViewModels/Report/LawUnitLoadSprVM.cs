using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class LawUnitLoadSprVM
    {
        public int LawUnitId { get; set; }
        public string LawUnitLabel { get; set; }
        public int? CourtId { get; set; }
        public string CourtLabel { get; set; }
        public int Year { get; set; }
        public decimal SumLoadIndex { get; set; }
        public decimal SumLoadIndex_Col1 { get; set; }
        public decimal SumLoadIndex_Col2 { get; set; }
        public decimal SumLoadIndex_Col3 { get; set; }
        public decimal SumLoadIndex_Col4 { get; set; }
        public decimal SumLoadIndex_Col5 { get; set; }
        public decimal SumLoadIndex_Col6 { get; set; }
        public decimal SumLoadIndex_Col7 { get; set; }
        public decimal SumLoadIndex_Col8 { get; set; }
        public decimal SumLoadIndex_Col9 { get; set; }
        public decimal CaseLoadIndex { get; set; }
        public decimal LoadIndex { get; set; }
    }
}
