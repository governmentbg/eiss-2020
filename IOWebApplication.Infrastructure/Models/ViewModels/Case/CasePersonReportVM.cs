using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CasePersonReportVM
    {
        public int CaseId { get; set; }

        public string CaseNumber { get; set; }

        public DateTime CaseDate { get; set; }

        public string Uic { get; set; }

        public string FullName { get; set; }

        public string RoleName { get; set; }

        public string CaseStateLabel { get; set; }
    }
}
