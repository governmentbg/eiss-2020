using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseArchiveListVM
    {
        public int Id { get; set; }

        public int CaseId { get; set; }

        [Display(Name = "Дата архив")]
        public string RegNumber { get; set; }

        [Display(Name = "Номер архив")]
        public DateTime RegDate { get; set; }

        [Display(Name = "Номер на Том")]
        public int BookNumber { get; set; }

        [Display(Name = "Година на Том")]
        public int? BookYear { get; set; }

        [Display(Name = "Срок на съхранение години")]
        public int StorageYears { get; set; }

        [Display(Name = "Точен вид дело")]
        public string CaseTypeLabel { get; set; }

        [Display(Name = "Статус")]
        public string CaseStateLabel { get; set; }

        [Display(Name = "Регистрационен номер")]
        public string CaseRegNumber { get; set; }

        [Display(Name = "Дата на регистрация")]
        public DateTime CaseRegDate { get; set; }

        [Display(Name = "Номенклатурен индекс")]
        public string CaseArchiveIndexName { get; set; }

        [Display(Name = "Дата на унищожаване")]
        public DateTime? DateDestroy { get; set; }
    }

    public class CaseForDestroyFilterVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Номер на дело")]
        public string CaseRegNumber { get; set; }

    }
}
