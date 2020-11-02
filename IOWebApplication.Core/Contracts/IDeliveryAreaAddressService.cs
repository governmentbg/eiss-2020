// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Common;
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
    public interface IDeliveryAreaAddressService : IBaseService
    {
        IQueryable<DeliveryAreaAddressVM> DeliveryAreaAddressSelect(DeliveryAreaAddressFilterVM filter);
        bool DeliveryAreaAddressSaveData(DeliveryAreaAddress model);
        List<SelectListItem> GetEkatte(int deliveryAreaId);
        string VerifyNumberFrom(int? NumberFrom, int? NumberType, int? NumberTo);
        string VerifyNumberTo(int? NumberFrom, int? NumberType, int? NumberTo);

        /// <summary>
        /// Намира район за адрес
        /// Първо търси DeliveryAreaAddress с въведени Улица квартал
        /// След това по населено място
        /// </summary>
        /// <returns></returns>
        DeliveryAreaFindVM DeliveryAreaAddressFind(Address address);

        DeliveryAreaFindVM DeliveryAreaCasePersonAddressIdFind(int CasePersonAddressId);

        DeliveryAreaFindVM DeliveryAreaAddressIdFind(int AddressId);
        IQueryable<DeliveryAreaAddressVM> DeliveryAreaAddressDuplication(int courtId);
        DeliveryAreaAddressTestVM DeliveryAreaAddressFindTest(DeliveryAreaAddressTestVM model);
        IQueryable<MultiSelectTransferVM> EkStreetForSelect_Select(string cityId, int courtId);
        bool DeliveryAreaAddressSaveListData(int deliveryAreaId, List<int> streets, string cityId);
        List<SelectListItem> ExpiredTypeDDL();
    }
}
