using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseSessionDocVM
    {
        public int Id { get; set; }
        public int CaseSessionId { get; set; }
        public long DocumentId { get; set; }
        public string DocumentLabel { get; set; }
        public string SessionDocStateLabel { get; set; }
        public DateTime DateFrom { get; set; }
    }
}
