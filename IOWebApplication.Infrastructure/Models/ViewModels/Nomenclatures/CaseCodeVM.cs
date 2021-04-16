using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures
{
   public class CaseCodeVM
    {
        public int Id { get; set; }

        [Display(Name = "Номер по ред")]
        public int OrderNumber { get; set; }

        [Display(Name = "Етикет")]
        public string Label { get; set; }

        [Display(Name = "Код")]
        public string Code { get; set; }

        [Display(Name = "Активен")]
        public bool IsActive { get; set; }

        [Display(Name = "Начална дата")]
        public DateTime DateStart { get; set; }

        [Display(Name = "Крайна дата")]
        public DateTime? DateEnd { get; set; }

        [Display(Name = "Правно основание")]
        public string LawBaseDescription { get; set; }
    }
}
