// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using System;
using System.Linq;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaseLawUnitTaskChangeService : IBaseService
    {
        IQueryable<CaseLawUnitTaskChangeVM> Select(int? id,DateTime? dateFrom, DateTime? dateTo, string caseNumber, string newTaskUserName);
        SaveResultVM SaveData(CaseLawUnitTaskChange model);
    }
}
