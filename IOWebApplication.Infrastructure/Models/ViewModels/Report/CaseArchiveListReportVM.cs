using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class CaseArchiveListReportVM
    {
        public int CaseId { get; set; }

        [Display(Name = "Вид дело")]
        public string CaseTypeName { get; set; }

        [Display(Name = "№/година")]
        public string CaseNumber { get; set; }

        [Display(Name = "Връзка")]
        public string ArchiveLink { get; set; }

        [Display(Name = "Архивен №")]
        public string ArchiveNumber { get; set; }

        [Display(Name = "Година на архивиране")]
        public int ArchiveYear { get; set; }

        [Display(Name = "Дата на архивиране")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ArchiveDate { get; set; }

        [Display(Name = "Номенклатурен индекс")]
        public string ArchiveIndexName { get; set; }

        [Display(Name = "Срок на съхранение")]
        public int StorageYears { get; set; }

    }

    public class CaseArchiveListFilterReportVM
    {
        [Display(Name = "От дата на архивиране")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата на архивиране")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Основен вид дело")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Точен вид дело")]
        public int CaseTypeId { get; set; }

        [Display(Name = "Архивен номер")]
        public string ArchiveNumber { get; set; }

    }
}
