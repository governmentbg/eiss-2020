using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class CourtDutyVM
    {
        public int Id { get; set; }
        public string CourtLabel { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int? CountLawUnit { get; set; }
    }
}
