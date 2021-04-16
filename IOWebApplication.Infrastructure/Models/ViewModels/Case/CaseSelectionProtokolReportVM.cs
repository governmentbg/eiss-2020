using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseSelectionProtokolReportVM
    {
        public int Id { get; set; }

        public int CaseId { get; set; }

        public string CaseNumber { get; set; }

        public long DocumentId { get; set; }
        public string DocumentNumber { get; set; }
        public int? DocumentNumberVal { get; set; }

        public DateTime CaseDate { get; set; }

        public string Uic { get; set; }

        public string FullName { get; set; }

        public DateTime SelectionDate { get; set; }

        public string JudgeRoleName { get; set; }

        public string SelectionModeName { get; set; }

        public string CaseStateLabel { get; set; }

        public string SelectionProtokolStateName { get; set; }
        public string UserName { get; set; }

        public string CaseTypeLabel { get; set; }

        public string CaseCodeLabel { get; set; }
    }
}
