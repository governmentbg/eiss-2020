// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;

namespace IOWebApplication.Core.Helper
{
    public static class TransactionScopeBuilder
    {
        /// <summary>
        /// Creates a transactionscope with ReadCommitted Isolation
        /// </summary>
        public static TransactionScope CreateReadCommitted()
        {
            var options = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = TransactionManager.DefaultTimeout
            };

            return new TransactionScope(TransactionScopeOption.Required, options);
        }
    }
}
