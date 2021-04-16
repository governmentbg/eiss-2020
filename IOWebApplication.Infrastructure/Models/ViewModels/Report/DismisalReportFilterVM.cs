using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class DismisalReportFilterVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Номер дело")]
        public string NumberCase { get; set; }
        
        [Display(Name = "Съдия-докладчик")]
        public int? LawUnitId { get; set; }

        [Display(Name = "Основен вид дело")]
        public string CaseGroupId { get; set; }

        public string CaseGroupIds { get; set; }

        [Display(Name = "Съдебен състав")]
        public string DepartmentId { get; set; }

        public string DepartmentIds { get; set; }
    }
}
