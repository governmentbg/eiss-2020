using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CasePersonMeasureVM
    {
        public int Id { get; set; }
        public string MeasureCourtLabel { get; set; }
        public string MeasureInstitutionLabel { get; set; }
        public string MeasureTypeLabel { get; set; }
        public DateTime MeasureStatusDate { get; set; }
        public double BailAmount { get; set; }
        public string MeasureStatusLabel { get; set; }
    }
}
