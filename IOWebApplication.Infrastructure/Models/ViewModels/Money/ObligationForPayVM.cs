﻿using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Money
{
    public class ObligationForPayVM
    {
        public int Id { get; set; }

        public bool CheckRow { get; set; }

        public string ObligationNumber { get; set; }

        public DateTime ObligationDate { get; set; }

        public string PersonUic { get; set; }

        public string PersonName { get; set; }

        public string MoneyTypeName { get; set; }

        public decimal Amount { get; set; }

        public decimal AmountPay { get; set; }

        public decimal AmountForPay { get { return this.Amount - this.AmountPay; } }

        public string ObligationInfo { get; set; }

        public string CaseData { get; set; }
        public bool IsActive { get; set; }

        public string RegNumberExpenseOrder { get; set; }

        public int ExpenseOrderId { get; set; }

        public string RegNumberExecList { get; set; }

        public int ExecListId { get; set; }
    }
}
