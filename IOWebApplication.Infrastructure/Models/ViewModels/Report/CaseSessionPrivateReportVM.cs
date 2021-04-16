using IOWebApplication.Infrastructure.Data.Models.Cases;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class CaseSessionPrivateReportVM
    {
        [Display(Name = "№ по ред")]
        public int Index { get; set; }

        [Display(Name = "Номер на делото")]
        public string CaseNumber { get; set; }

        [Display(Name = "Състав на съда")]
        public string CompartmentName { get; set; }

        [Display(Name = "Докладчик")]
        public string JudgeReporterName { get; set; }

        [Display(Name = "Резултат от заседанието")]
        public string CaseSessionResultName { get; set; }

        public DateTime? ActDateOrder { get; set; }

        public int CaseGroupId { get; set; }

        public int SessionStateId { get; set; }

        public bool ActEnforcedFinal { get; set; }
        public bool ActEnforcedNoFinal { get; set; }
        public int CaseTypeId { get; set; }
        public string CaseSessionResult { get; set; }
        public int CaseCodeId { get; set; }
        public int CaseInstanceId { get; set; }
        public int DocumentTypeId { get; set; }

        public IEnumerable<CaseSessionPrivateAct> acts { get; set; }

        public CaseSessionPrivateReportVM()
        {
            acts = new HashSet<CaseSessionPrivateAct>();
        }
    }

    public class CaseSessionPrivateFilterReportVM
    {
        [Display(Name = "От дата")]
        public DateTime DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime DateTo { get; set; }

        [Display(Name = "Основен вид дело")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Съдия докладчик")]
        public int JudgeReporterId { get; set; }

        [Display(Name = "Съдебен състав")]
        public int DepartmentId { get; set; }
    }

    public class CaseSessionPrivateAct
    {
        public string ActTypeName { get; set; }
        public DateTime ActDate { get; set; }
        public string ActNumber { get; set; }
        public string ActDescription { get; set; }
        public DateTime? ActDeclaredDate { get; set; }
    }
}
