using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseSessionHallUseFilterVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Зала")]
        public int? CourtHallId { get; set; }

        [Display(Name = "Изглед календар")]
        public bool? IsCalendar { get; set; }

        [Display(Name = "Съдия докладчик")]
        public int JudgeReporterId { get; set; }
    }
}
