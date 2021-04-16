using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures
{
    public class LawBaseEditVM
    {
        public int Id { get; set; }
        public int? CourtTypeId { get; set; }

        public int? CaseInstanceId { get; set; }

        public int? CaseGroupId { get; set; }

        public int? CaseId { get; set; }

        [Display(Name = "Код")]
        public string Code { get; set; }

        [Display(Name = "Етикет")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        public string Label { get; set; }

        [Display(Name = "Активен")]
        public bool IsActive { get; set; }

        [Display(Name = "Начална дата")]
        public DateTime DateStart { get; set; }

        [Display(Name = "Крайна дата")]
        public DateTime? DateEnd { get; set; }

    }
}
