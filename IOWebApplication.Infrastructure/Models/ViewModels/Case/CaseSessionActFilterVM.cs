using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseSessionActFilterVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Година")]
        public int? Year { get; set; }

        [Display(Name = "Номер на дело")]
        public string CaseRegNumber { get; set; }

        [Display(Name = "Финализиращ акт")]
        public bool IsFinalDoc { get; set; }

        [Display(Name = "Нормативен текст")]
        public int ActLawBaseId { get; set; }

        [Display(Name = "Съдия докладчик")]
        public int JudgeReporterId { get; set; }

        [Display(Name = "Вид акт")]
        public int? ActTypeId { get; set; }

        [Display(Name = "Регистрационен номер")]
        public string RegNumber { get; set; }
    }
}
