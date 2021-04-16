using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseLawUnitVM
    {
        public int Id { get; set; }
        public int OrderNumber { get; set; }
        public int CaseId { get; set; }
        public int? CaseSessionId { get; set; }
        public string CaseSessionLabel { get; set; }
        public int LawUnitId { get; set; }
        public string LawUnitName { get; set; }
        public string LawUnitNameShort { get; set; }
        public int JudgeRoleId { get; set; }
        public string JudgeRoleLabel { get; set; }
        public int? JudgeDepartmentRoleId { get; set; }
        public string JudgeDepartmentRoleLabel { get; set; }
        public bool IsExistDismisal { get; set; }
        public string DismisalLabel { get; set; }
        public string DismisalLabelFull { get; set; }
        public string RowLabelFull { get; set; }
        public string DepartmentLabel { get; set; }
        public int? DepartmentId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int? SubstitutionId { get; set; }
        public int SubstitutedLawUnitId { get; set; }
        public string SubstitutedLawUnitName { get; set; }
    }
}
