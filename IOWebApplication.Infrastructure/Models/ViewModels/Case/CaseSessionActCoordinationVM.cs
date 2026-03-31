using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseSessionActCoordinationVM
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public int CaseSessionActId { get; set; }
        public int LawUnitId { get; set; }
        public int LawUnitTypeId { get; set; }
        public int ActCoordinationTypeId { get; set; }
        public int CoordinationType { get; set; }
        public string CaseLawUnitName { get; set; }
        public string ActCoordinationTypeLabel { get; set; }
        public string JudgeRoleLabel { get; set; }
        public string ActTypeName { get; set; }
        public string ActNumber { get; set; }
        public DateTime? ActDate { get; set; }
        public DateTime? CoordinationDeclaredDate { get; set; }
        public string Content { get; set; }
        public bool CanUpdate { get; set; }
    }

    public class CoordinationDepersonalizeVM
    {
        public int Id { get; set; }
        public string JudgeName { get; set; }
        public string JudgeRole { get; set; }
        public bool HasPublicFile { get; set; }
        public bool HasSignedPrivateFile { get; set; }
    }
}
