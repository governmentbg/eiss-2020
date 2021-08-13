using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseForArchiveVM
    {
        public int Id { get; set; }

        [Display(Name = "Точен вид дело")]
        public string CaseTypeLabel { get; set; }

        [Display(Name = "Статус")]
        public string CaseStateLabel { get; set; }

        [Display(Name = "Регистрационен номер")]
        public string RegNumber { get; set; }

        [Display(Name = "Кратък номер")]
        public string ShortRegNumber { get; set; }

        [Display(Name = "Дата на регистрация")]
        public DateTime RegDate { get; set; }

        [Display(Name = "Шифър")]
        public string CaseCodeLabel { get; set; }        
    }

    public class CaseForArchiveFilterVM
    {
        [Display(Name = "Основен вид")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Точен вид дело")]
        public int CaseTypeId { get; set; }

        [Display(Name = "Номер на дело")]
        public string CaseNumber { get; set; }
    }
}
