using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseEvidenceSprFilterVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "От номер")]
        [Range(1, int.MaxValue, ErrorMessage = "Въведете стойност по-голяма от 0")]
        public int? FromNumber { get; set; }

        [Display(Name = "До номер")]
        [Range(1, int.MaxValue, ErrorMessage = "Въведете стойност по-голяма от 0")]
        public int? ToNumber { get; set; }
    }
}
