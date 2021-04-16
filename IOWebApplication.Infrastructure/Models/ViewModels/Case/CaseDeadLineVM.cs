using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseDeadLineVM
    {
        public int Id { get; set; }

        public int CaseId { get; set; }

        public string CaseInfo { get; set; }

        public int SourceType { get; set; }

        public long SourceId { get; set; }

        public string DeadlineGroup { get; set; }

        public string DeadlineType { get; set; }

        public string LawUnitName { get; set; }
        public string SecretaryName { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime? DateComplete { get; set; }

        public string SourceUrl { get; set; }
    }
}
