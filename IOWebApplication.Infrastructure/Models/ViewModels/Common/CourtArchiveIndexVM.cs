using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class CourtArchiveIndexVM
    {
        public int Id { get; set; }

        [Display(Name = "Индекс")]
        public string Code { get; set; }

        [Display(Name = "Наименование")]
        public string Label { get; set; }

        [Display(Name = "Експертна комисия")]
        public string CourtArchiveCommitteeName { get; set; }

        [Display(Name = "Срок на съхранение години")]
        public int StorageYears { get; set; }

        [Display(Name = "Начална дата")]
        public DateTime DateStart { get; set; }

        [Display(Name = "Крайна дата")]
        public DateTime? DateEnd { get; set; }

    }
}
