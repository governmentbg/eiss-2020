using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class CourtOrganizationEditVM
    {
        public int Id { get; set; }
        public int CourtId { get; set; }

        [Display(Name = "Горно ниво")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете горно ниво")]
        public int? ParentId { get; set; }

        [Display(Name = "Наименование")]
        [Required(ErrorMessage = "Въведете наименование")]
        public string Label { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Ниво")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете ниво")]
        public int OrganizationLevelId { get; set; }

        [Display(Name = "Деловодна регистратура")]
        public bool? IsDocumentRegistry { get; set; }

        [Display(Name = "Дата от")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime DateFrom { get; set; }

        [Display(Name = "Дата до")]
        public DateTime? DateTo { get; set; }

        public virtual List<CheckListVM> CourtOrganizationCaseGroups { get; set; }
    }
}
