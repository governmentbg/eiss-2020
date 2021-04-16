using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CasePersonFilterVM
    {
        [Display(Name = "Идентификатор на лице")]
        public string Uic { get; set; }


        [Display(Name = "Имена/Наименование на лице")]
        public string FullName { get; set; }

        [Display(Name = "Номер на дело")]
        public string CaseRegNumber { get; set; }

        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "От дата на свършване")]
        public DateTime? FinalDateFrom { get; set; }

        [Display(Name = "До дата на свършване")]
        public DateTime? FinalDateTo { get; set; }

        [Display(Name = "Несвършило към дата")]
        public DateTime? WithoutFinalDateTo { get; set; }
    }
}
