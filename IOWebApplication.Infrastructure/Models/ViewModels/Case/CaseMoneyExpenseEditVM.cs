using IOWebApplication.Infrastructure.Data.Models.Cases;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseMoneyExpenseEditVM
    {
        public int Id { get; set; }

        public int? CourtId { get; set; }

        public int CaseId { get; set; }

        [Display(Name = "Вид")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
        public int CaseMoneyExpenseTypeId { get; set; }

        [Display(Name = "Валута")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
        public int CurrencyId { get; set; }

        [Display(Name = "Сума")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public decimal Amount { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Солидарно разпределение")]
        public bool? JointDistribution { get; set; }

        [Display(Name = "Дроб")]
        public bool? IsFraction { get; set; }

        public IList<CasePersonListDecimalVM> CasePersonListDecimals { get; set; }

        public void ToEntity(CaseMoneyExpense model)
        {
            model.Id = Id;
            model.CourtId = CourtId;
            model.CaseId = CaseId;
            model.CaseMoneyExpenseTypeId = CaseMoneyExpenseTypeId;
            model.CurrencyId = CurrencyId;
            model.Amount = Amount;
            model.Description = Description;
            model.JointDistribution = JointDistribution;
            model.IsFraction = IsFraction;
        }
    }
}
