using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseMoneyCollectionPersonVM
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public int CaseMoneyCollectionId { get; set; }
        public string CasePersonLabel { get; set; }
        public decimal PersonAmount { get; set; }
        public string PersonAmountString { get; set; }
        public decimal RespectedAmount { get; set; }
        public string RespectedAmountString { get; set; }
        public string CurrencyCode { get; set; }
    }
}
