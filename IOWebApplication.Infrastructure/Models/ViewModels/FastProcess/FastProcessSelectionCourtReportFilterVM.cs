using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class FastProcessSelectionCourtReportFilterVM
    {

        [Display(Name = "Година")]
        public int YearId { get; set; }
        [Display(Name = "Месец")]
        public int MonthId { get; set; }

        
    }


}
