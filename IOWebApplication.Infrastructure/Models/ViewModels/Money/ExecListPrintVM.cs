using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Money
{
    public class ExecListPrintVM
    {
        public int Id { get; set; }

        public string Debtor { get; set; }
        public string Uic { get; set; }

        public string DebtorAddress { get; set; }

        public string CaseTypeName { get; set; }
        public string CaseNumber { get; set; }
        public string Composition { get; set; }
        public string EventData { get; set; }
        public string Amount { get; set; }
    }
}
