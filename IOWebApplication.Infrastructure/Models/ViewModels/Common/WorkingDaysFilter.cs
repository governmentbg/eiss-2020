using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class WorkingDaysFilter
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }
    }
}
