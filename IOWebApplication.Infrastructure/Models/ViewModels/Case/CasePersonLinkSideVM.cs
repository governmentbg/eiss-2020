using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CasePersonLinkSideVM
    {
        public int CourtId { get; set; }

        public int CaseId { get; set; }

      
        [Display(Name = "Страна")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете")]
        public int RoleKindId { get; set; }

        [Display(Name = "Упълномощено лице")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете")]
        public int CasePersonRelId { get; set; }

        [Display(Name = "Ред на представляване")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете")]
        public int LinkDirectionId { get; set; }

        [Display(Name = "От дата")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }
    }
}
