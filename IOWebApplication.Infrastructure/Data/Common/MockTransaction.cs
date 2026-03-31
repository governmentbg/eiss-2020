// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Data.Common
{
    public class MockTransaction : IDbContextTransaction
    {
        public Guid TransactionId => Guid.NewGuid();

        public void Commit()
        {
            
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
           return Task.CompletedTask;
        }

        public void Dispose()
        {
            
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public void Rollback()
        {
            
        }

        public Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
