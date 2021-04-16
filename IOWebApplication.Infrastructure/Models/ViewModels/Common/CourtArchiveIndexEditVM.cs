using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class CourtArchiveIndexEditVM
    {
        public int Id { get; set; }

        public int CourtId { get; set; }

        [Display(Name = "Индекс")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        public string Code { get; set; }

        [Display(Name = "Наименование")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        public string Label { get; set; }

        [Display(Name = "Експертна комисия")]
        public int? CourtArchiveCommitteeId { get; set; }

        [Display(Name = "Срок на съхранение години")]
        public int StorageYears { get; set; }

        [Display(Name = "Начална дата")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        public DateTime DateStart { get; set; }

        [Display(Name = "Крайна дата")]
        public DateTime? DateEnd { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

    }
}
