using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class CourtGroupVM
    {
        public int Id { get; set; }
        public int OrderNumber { get; set; }
        public string Label { get; set; }
        public string CaseGroupLabel { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public int CountCode { get; set; }
        public int CountLawUnit { get; set; }
    }
}
