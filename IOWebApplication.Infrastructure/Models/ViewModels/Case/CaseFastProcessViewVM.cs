// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseFastProcessViewVM
    {
        public virtual CaseFastProcessVM FastProcessVM { get; set; }
        public virtual ICollection<CaseBankAccountVM> CaseBankAccounts { get; set; }
        public virtual ICollection<CaseMoneyClaimVM> CaseMoneyClaims { get; set; }
        public virtual ICollection<CaseMoneyExpenseVM> CaseMoneyExpenses { get; set; }

        public CaseFastProcessViewVM()
        {
            FastProcessVM = new CaseFastProcessVM();
        }
    }
}
