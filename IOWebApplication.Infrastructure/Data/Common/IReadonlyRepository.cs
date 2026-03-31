// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Linq;
using System.Linq.Expressions;

namespace IOWebApplication.Infrastructure.Data.Common
{
    public interface IReadonlyRepository
    {
        IQueryable<T> AllReadonly<T>() where T : class;
        IQueryable<T> AllReadonly<T>(Expression<Func<T, bool>> search) where T : class;
        void Dispose();
    }
}
