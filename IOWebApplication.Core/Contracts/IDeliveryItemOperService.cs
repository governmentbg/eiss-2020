// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Delivery;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface IDeliveryItemOperService
    {
        IQueryable<DeliveryItemOperListVM> DeliveryItemOperSelect(int deliveryItemId, bool onlyLast);

        List<SelectListItem> DeliveryOperSelect();
        List<SelectListItem> NotificationStateForDeliveryOperSelect(int operId);

        List<SelectListItem> DeliveryOperForNotificationStateSelect(int operId);

        DeliveryItemOperVM getDeliveryItemOper(int id);

        DeliveryItemOperVM makeDeliveryItemOper(int deliveryItemId);
        int GetDeliveryOperId(int deliveryItemId);
        List<SelectListItem> GetDeliveryReasonDDL(int notificationStateId);
        bool CanAdd(int deliveryItemId);
        DateTime? LastDateOper(int deliveryItemId);
        DateTime? GetRegDate(int deliveryItemId);
    }
}
