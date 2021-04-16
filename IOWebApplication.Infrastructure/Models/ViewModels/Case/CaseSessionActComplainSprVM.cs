using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseSessionActComplainSprVM
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public string JudgeName { get; set; }
        public string IndexLabel { get; set; }
        public DateTime? DateReturn { get; set; }
        public string ComplainDocumentNumber { get; set; }
        public DateTime? ComplainDocumentDate { get; set; }
        public string Instance { get; set; }
        public string ComplainDocumentType { get; set; }
        public string ActName { get; set; }
        public string CaseGroupLabel { get; set; }
        public string CaseNumber { get; set; }
        public string Result { get; set; }
    }
}
