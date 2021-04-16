using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseMoneyExpensePersonVM
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public int CaseMoneyExpenseId { get; set; }
        public string CasePersonLabel { get; set; }
        public decimal PersonAmount { get; set; }
        public string PersonAmountString { get; set; }
        public string CurrencyCode { get; set; }
    }
}
