using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class PublicInformationReportVM
    {
        
        [Display(Name = "Пореден № на заявлението")]
        public int Index { get; set; }

        [Display(Name = "Заявител")]
        public string Notifier { get; set; }

        [Display(Name = "Входящ № и дата на заявлението")]
        public string DocumentData { get; set; }

        [Display(Name = "Кратко описание на поисканата информация")]
        public string Description { get; set; }

        [Display(Name = "№, дата и съдържане на решението предоставен достъп пълен, частичен или отказ")]
        public string Decision { get; set; }
    }

    public class PublicInformationFilterReportVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "От номер")]
        [Range(1, int.MaxValue, ErrorMessage = "Въведете стойност по-голяма от 0")]
        public int? NumberFrom { get; set; }

        [Display(Name = "До номер")]
        [Range(1, int.MaxValue, ErrorMessage = "Въведете стойност по-голяма от 0")]
        public int? NumberTo { get; set; }
    }
}
