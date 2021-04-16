using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class CourtLawUnitActivityVM
    {
        public int Id { get; set; }
        public string LawUnitLabel { get; set; }
        public DateTime ActivityDate { get; set; }
        public string JudgeLoadActivityLabel { get; set; }
        public decimal LoadIndex { get; set; }
    }
}
