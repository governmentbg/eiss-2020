using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class DocumentListVM
    {
        public long Id { get; set; }
        public string DocumentDirectionName { get; set; }
        public string DocumentTypeName { get; set; }
        public string DocumentNumber { get; set; }
        public DateTime DocumentDate { get; set; }
        public string UserName { get; set; }
        public IEnumerable<DocumentListPersonVM> Persons { get; set; }
        public string CaseNumber { get; set; }
        public int? CaseId { get; set; }
        public bool? IsCaseRejected { get; set; }
        public int DocumentNumberValue { get; set; }
        public string Description { get; set; }
    }
}
