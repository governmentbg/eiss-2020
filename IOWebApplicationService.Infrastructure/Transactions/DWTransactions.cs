﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;

namespace IOWebApplicationService.Infrastructure.Transactions
{
    public static class DWTransactions
    {
    public static TransactionScope GetTransactionScope()
    {
      TransactionOptions transactionOptions = new TransactionOptions();
      transactionOptions.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
      return new TransactionScope(TransactionScopeOption.Required, transactionOptions);
    }
  }
}
