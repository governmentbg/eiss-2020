using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Money
{
    public class ExpenseOrderVM
    {
        public int Id { get; set; }

        [Display(Name = "Номер")]
        public string RegNumber { get; set; }

        [Display(Name = "Дата")]
        public DateTime RegDate { get; set; }

        [Display(Name = "Активен")]
        public bool IsActive { get; set; }

        [Display(Name = "Наименование")]
        public string FullName { get; set; }

        [Display(Name = "Сума")]
        public decimal Amount { get; set; }

        [Display(Name = "Статус")]
        public string ExpenseOrderStateName { get; set; }

        [Display(Name = "От сметка")]
        public string MoneyGroupName { get; set; }
    }

    public class ExpenseOrderFilterVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Номер")]
        public string RegNumber { get; set; }

        [Display(Name = "Наименование")]
        public string FullName { get; set; }

    }
}
