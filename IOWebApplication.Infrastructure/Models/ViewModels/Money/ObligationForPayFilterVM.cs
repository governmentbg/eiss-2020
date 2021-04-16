using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Money
{
    public class ObligationForPayFilterVM
    {
        [Display(Name = "Идентификатор")]
        public string PersonUicSearch { get; set; }

        [Display(Name = "Наименование/Част от Наименование")]
        public string PersonNameSearch { get; set; }

        [Display(Name = "Статус")]
        public int Status { get; set; }

        [Display(Name = "Разходен ордер")]
        public string ExpenseOrderSearch { get; set; }

        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Вид")]
        public int Sign { get; set; }

        [Display(Name = "Вид сума")]
        public int MoneyTypeId { get; set; }

        [Display(Name = "Номер на дело")]
        public string CaseRegNumber { get; set; }
    }
}
