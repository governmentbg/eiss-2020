// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Models.ViewModels.Report;
using System.Linq;

namespace IOWebApplication.Core.Contracts
{
    public interface IDeactivateItemService : IBaseService
    {
        IQueryable<DeactivateItemVM> Select(DeactivateItemFilterVM filter);
    }
}
