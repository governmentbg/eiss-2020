using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseSessionActCoordinationVM
    {
        public int Id { get; set; }
        public int CaseSessionActId { get; set; }
        public int LawUnitId { get; set; }
        public int ActCoordinationTypeId{ get; set; }
        public string CaseLawUnitName { get; set; }
        public string ActCoordinationTypeLabel { get; set; }
        public string JudgeRoleLabel { get; set; }
        public string ActTypeName { get; set; }
        public string ActNumber { get; set; }
        public DateTime? ActDate { get; set; }
        public string Content { get; set; }
        public bool CanUpdate { get; set; }
    }
}
