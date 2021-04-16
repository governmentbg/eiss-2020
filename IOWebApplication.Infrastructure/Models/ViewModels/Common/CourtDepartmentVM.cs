using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class CourtDepartmentVM
    {
        public int Id { get; set; }
        public string CourtLabel { get; set; }
        public string CaseGroupLabel { get; set; }
        public string ParentLabel { get; set; }
        public string DepartmentTypeLabel { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public int? MasterId { get; set; }
        public int? ParentId { get; set; }
    }
}
