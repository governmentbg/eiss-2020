using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class DivorceReportVM
    {
        [Display(Name = "№ по ред")]
        public int Index { get; set; }

        [Display(Name = "Дата на изготвяне на съобщението")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DivorceRegDate { get; set; }

        [Display(Name = "№ и дата на писмото, с което е изпратено съобщението")]
        public string OutDocumentData { get; set; }

        [Display(Name = "Вид, №/година на дело")]
        public string CaseData { get; set; }

        [Display(Name = "Съдебен акт")]
        public string SessionActData { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата на влизане в законна сила")]
        public DateTime? CaseSessionActInforcedDate { get; set; }
    }

    public class DivorceFilterReportVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Номер на дело")]
        public string CaseNumber { get; set; }
    }
}
