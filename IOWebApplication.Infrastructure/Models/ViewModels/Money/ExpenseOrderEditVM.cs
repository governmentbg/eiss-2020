using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Money
{
    public class ExpenseOrderEditVM
    {
        public int Id { get; set; }

        [Display(Name = "Регион")]
        public string RegionName { get; set; }

        [Display(Name = "Служител при")]
        public string FirmName { get; set; }

        [Display(Name = "Населено място")]
        public string FirmCity { get; set; }

        [Display(Name = "По вн. документ No")]
        public string PaidNote { get; set; }

        [Display(Name = "Банкова сметка")]
        [RegularExpression("[a-zA-Z]{2}[0-9]{2}[a-zA-Z0-9]{4}[0-9]{6}([a-zA-Z0-9]?){0,16}", ErrorMessage = "Невалиден IBAN.")]
        public string Iban { get; set; }

        [Display(Name = "BIC")]
        public string BIC { get; set; }

        [Display(Name = "Име на банката")]
        public string BankName { get; set; }

        [Display(Name = "Статус")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете")]
        public int? ExpenseOrderStateId { get; set; }

        public bool ForPopUp { get; set; }

        public string ObligationIdStr { get; set; }

        [Display(Name = "Съдия")]
        public int LawUnitSignId { get; set; }
    }
}
