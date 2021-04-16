using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Documents
{
    public class DocumentSelectPersonsVM
    {
        public int SourceType { get; set; }
        public string SourceId { get; set; }
        public string SourceTypeName { get; set; }
        public IList<DocumentSelectPersonItemVM> Persons { get; set; }
        public DocumentSelectPersonsVM()
        {
            Persons = new List<DocumentSelectPersonItemVM>();
        }
    }
}
