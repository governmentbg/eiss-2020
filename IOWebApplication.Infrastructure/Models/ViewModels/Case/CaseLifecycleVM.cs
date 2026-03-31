using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseLifecycleVM
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public string LifecycleTypeLabel { get; set; }
        public int LifecycleTypeId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int Iteration { get; set; }
        
        public string IterationText 
        { 
            get
            {
                switch (Iteration)
                {
                    case 1: return "I интервал";
                    case 2: return "II интервал";
                    case 3: return "III интервал";
                    case 4: return "IV интервал";
                    case 5: return "V интервал";
                    case 6: return "VI интервал";
                    case 7: return "VII интервал";
                    case 8: return "VIII интервал";
                    case 9: return "IX интервал";
                    case 10: return "X интервал";
                    default: return string.Empty;
                }
            }
        }
        
        public int DurationMonths { get; set; }
        public string DurationMonthsText { get; set; }
        public bool ModelEdit { get; set; }
    }
}
