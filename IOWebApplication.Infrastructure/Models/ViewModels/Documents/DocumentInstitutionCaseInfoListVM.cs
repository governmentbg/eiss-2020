using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Documents
{
    public class DocumentInstitutionCaseInfoListVM
    {
        public long Id { get; set; }
        public string InstitutionLabel { get; set; }
        public string CaseNumber { get; set; }
        public int CaseYear { get; set; }
        public string InstitutionCaseTypeLabel { get; set; }
    }
}
