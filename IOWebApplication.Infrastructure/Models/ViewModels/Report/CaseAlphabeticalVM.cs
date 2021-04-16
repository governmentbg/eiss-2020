using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class CaseAlphabeticalVM
    {
        public int CaseGroupId { get; set; }
        [Display(Name = "Основен вид дело")]
        public string CaseGroupLabel { get; set; }
        public int CaseTypeId { get; set; }
        [Display(Name = "Точен вид дело")]
        public string CaseTypeLabel { get; set; }
        public string ShortNumber { get; set; }
        [Display(Name = "Дата")]
        public DateTime RegDate { get; set; }
        [Display(Name = "Дата")]
        public string RegDateString { get; set; }
        [Display(Name = "Име")]
        public string Name { get; set; }
        [Display(Name = "Идентификационен номер")]
        public string IdentityNumber { get; set; }
        [Display(Name = "Номер дело")]
        public string CaseNumberString { get; set; }
        public char FirstLetter { get; set; }
        public int? ReportGroupe { get; set; }
        public int UicTypeId { get; set; }
    }
}
