// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface IDeliveryAreaService : IBaseService
    {
        IQueryable<DeliveryAreaVM> DeliveryAreaSelect(DeliveryAreaFilterVM filter);
        bool DeliveryAreaSaveData(DeliveryArea model);
        List<SelectListItem> DeliveryAreaSelectDDL(int forCourtId, bool addNotSet);
        int? GetDeliveryAreaIdByLawUnitId(int courtId, int? lawUnitId);
        List<Select2ItemVM> DeliveryAreaDdlSelect2(int forCourtId);
        DeliveryArea GetById(int id);
        void insertCaseRegion();
        void saveHtmlTemplateLink();
        void updateCaseRegionParent();
        List<SelectListItem> RemoveSelectAddNoChange(List<SelectListItem> fromList);
        List<Select2ItemVM> RemoveSelectAddNoChangeSelect2(List<Select2ItemVM> fromList, string newVal);
        List<SelectListItem> DeliveryAreaListToDdl(List<DeliveryArea> deliveryAreaList);
        List<Select2ItemVM> DeliveryAreaListToDdlSelect2(List<DeliveryArea> deliveryAreaList);
    }
}
