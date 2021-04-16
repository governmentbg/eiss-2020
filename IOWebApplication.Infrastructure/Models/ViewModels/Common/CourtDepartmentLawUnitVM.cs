using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class CourtDepartmentLawUnitVM
    {
        public int Id { get; set; }
        public string LawUnitName { get; set; }
        public int LawUnitId { get; set; }
        public string JudgeDepartmentRoleLabel { get; set; }
        public int? JudgeDepartmentRoleId { get; set; }
    }
}
