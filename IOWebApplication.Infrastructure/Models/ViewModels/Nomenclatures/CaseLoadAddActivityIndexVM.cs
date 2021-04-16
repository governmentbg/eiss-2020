using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures
{
    public class CaseLoadAddActivityIndexVM
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public string CourtTypeLabel { get; set; }
        public decimal LoadIndex { get; set; }
    }
}
