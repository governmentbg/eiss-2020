// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;

namespace IOWebApplication.Infrastructure.Contracts
{
    public interface IElasticSearchMessageService : IDisposable
    {
        void ManageDocument(string methodName, string fileId);
    }
}
