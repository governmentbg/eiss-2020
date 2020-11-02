// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface IDeliveryAccountService : IBaseService
    {
        (string, string) GenerateBarcodeTying(string userId);
        (string, string) GetBarcodeTying(string Id);
        IQueryable<DeliveryTokenVM> GetDeliveryTokenForUser(string userId);
        bool SaveExpireInfoPlus(ExpiredInfoVM model);
        int TokenForUserNewCount(string userId);
    }
}
