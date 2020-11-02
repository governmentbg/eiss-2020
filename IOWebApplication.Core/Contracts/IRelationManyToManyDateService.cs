// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface IRelationManyToManyDateService 
    {
        bool SaveData<T>(int parentId, List<int> codes,
                                Expression<Func<T, int>> parentProp,
                                Expression<Func<T, int>> itemProp,
                                Expression<Func<T, DateTime?>> dateFromProp,
                                Expression<Func<T, DateTime?>> dateToProp,
                                Func<T, bool> setNew)
            where T : class, new();
        bool SaveDataPercent<T>(int parentId, List<MultiSelectTransferPercentVM> codes,
                                Expression<Func<T, bool>> courtIdWhere,
                                Expression<Func<T, int>> parentProp,
                                Expression<Func<T, int>> itemProp,
                                Expression<Func<T, DateTime?>> dateFromProp,
                                Expression<Func<T, DateTime?>> dateToProp,
                                Expression<Func<T, int>> percentProp,
                                Func<T, bool> setNew) 
            where T : class, new();
    }
}
