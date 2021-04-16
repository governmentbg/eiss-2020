using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class DismisalReportVM
    {
        [Display(Name = "Пореден номер")]
        public int Number { get; set; }

        [Display(Name = "Дата на заседание")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? CaseSessionDate { get; set; }

        [Display(Name = "Вид/Номер/Година на дело")]
        public string CaseRegNumber { get; set; }

        [Display(Name = "Докладчик")]
        public string LawUnitName { get; set; }

        [Display(Name = "Мотиви за отводи")]
        public string Description { get; set; }

        [Display(Name = "Имена на новия докладчик")]
        public string LawUnitNewName { get; set; }
    }
}
