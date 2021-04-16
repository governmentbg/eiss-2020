using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class SelectCourtVM
    {
        [Display(Name = "Изберете съд")]
        public int CourtId { get; set; }

        public bool IsAdmin { get; set; }
    }
}
