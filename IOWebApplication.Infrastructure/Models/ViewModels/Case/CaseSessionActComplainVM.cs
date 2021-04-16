using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseSessionActComplainVM
    {
        public int Id { get; set; }
        public string ComplainDocumentName { get; set; }
        public string ComplainStateLabel { get; set; }
        public string CasePersonName { get; set; }
    }
}
