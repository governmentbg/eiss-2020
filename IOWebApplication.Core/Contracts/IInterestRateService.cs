// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Models;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface IInterestRateService : IBaseService
    {
        Task<decimal> CalcOLPRates(decimal amount, DateTime fromDate, DateTime toDate, decimal addProcent);
        string GetInterestTypeName(int interestType);
        Task<SaveResultVM> SaveData(InterestRate model);
        IQueryable<InterestRateDateVM> Select(FilterInterestRate filter);
    }
}
