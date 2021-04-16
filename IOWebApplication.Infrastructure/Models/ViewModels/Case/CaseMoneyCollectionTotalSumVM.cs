using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseMoneyCollectionTotalSumVM
    {
        public decimal TotalSum { get; set; }
        public string TotalSumText { get; set; }
        public string CurrencyCode { get; set; }
        public int CurrencyId { get; set; }
    }
}
