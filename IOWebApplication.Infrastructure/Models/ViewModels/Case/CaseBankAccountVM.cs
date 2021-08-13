using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseBankAccountVM
    {
        public int Id { get; set; }
        public int CaseBankAccountTypeId { get; set; }
        public string CaseBankAccountTypeLabel { get; set; }
        public string LabelIBAN { get; set; }
        public string LabelBIC { get; set; }
        public string BankName { get; set; }
        public string Description { get; set; }
        public bool IsBankAccount { get; set; }
        public bool? VisibleEL { get; set; }
    }
}
