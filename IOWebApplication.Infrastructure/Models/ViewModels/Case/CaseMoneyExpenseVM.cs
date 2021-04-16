using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseMoneyExpenseVM
    {
        public int Id { get; set; }
        public string CaseMoneyExpenseTypeLabel { get; set; }
        public string CurrencyLabel { get; set; }
        public string CurrencyCode { get; set; }
        public decimal Amount { get; set; }
        public string AmountString { get; set; }
        public string Description { get; set; }
        public string JointDistribution { get; set; }
        public bool JointDistributionBool { get; set; }

        public IList<CaseMoneyExpensePersonVM> MoneyExpensePeople { get; set; }
    }
}
