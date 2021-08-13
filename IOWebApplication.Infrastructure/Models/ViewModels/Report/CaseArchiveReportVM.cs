using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class CaseArchiveReportVM
    {
        [Display(Name = "Пореден номер")]
        public int Index { get; set; }

        [Display(Name = "Номер, година и характер на делото")]
        public string CaseData { get; set; }

        [Display(Name = "Дата и година на внасяне на делото в служба Архив")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ArchiveDate { get; set; }

        [Display(Name = "Номер на връзката")]
        public string ArchiveLink { get; set; }

        [Display(Name = "Номер и дата на протокол, възоснова на който се унищожава делото")]
        public string ActData { get; set; }

        [Display(Name = "Номер и дата на запазените документи, кратко описание на съдържанието им")]
        public string Description { get; set; }

        [Display(Name = "Номер на тома и годината, в който са подредени за запазване")]
        public string BookData { get; set; }

        [Display(Name = "Номенклатурен индекс")]
        public string ArchiveIndex { get; set; }

        [Display(Name = "Забележка")]
        public string DescriptionInfo { get; set; }

        [Display(Name = "Страни")]
        public string CasePersonNames { get; set; }

        [Display(Name = "Архивен номер")]
        public string RegNumber { get; set; }
    }

    public class CaseArchiveFilterReportVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Основен вид дело")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Година на образуване")]
        public int? CaseYear { get; set; }


        [Display(Name = "Добавяне на страни")]
        public bool WithPerson { get; set; }
    }
}
