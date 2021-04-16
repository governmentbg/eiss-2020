using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseSessionHallUseVM
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public int CaseSessionId { get; set; }
        public int? CourtHallId { get; set; }
        public string CourtHallName { get; set; }
        public string CaseName { get; set; }
        public string SessionLabel { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string SessionTypeLabel { get; set; }
        public string CourtHallLocation { get; set; }
        public string JudgeReport { get; set; }
    }
}
