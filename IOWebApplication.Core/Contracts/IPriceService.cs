// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.Eispp.ActualData;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Eispp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IOWebApplication.Core.Contracts
{
    public interface IPriceService : IBaseService
    {
        /// <summary>
        /// Вземане на стойност от тарифа
        /// </summary>
        /// <param name="courtId">id на съд, за когото важи, null - за всички</param>
        /// <param name="keyword">ключ на тарифа, уникален идентификатор</param>
        /// <param name="mainData">основна стойност за сравнение, при повече от един ред</param>
        /// <param name="dateNow">към дата, по подразбиране- днешна дата</param>
        /// <param name="baseValue">основна стойност, при изчисляване на процент</param>
        /// <param name="colNumber">Номер на колона, при тарифи с повече от една колона стойност</param>
        /// <param name="rowKeyword">Зона за ред</param>
        /// <returns></returns>
        decimal GetPriceValue(int? courtId, string keyword, decimal mainData = 0M, DateTime? dateNow = null, decimal baseValue = 0M, int colNumber = 0, string rowKeyword = null);


        IQueryable<PriceDesc> PriceDesc_Select(int? courtId, string name);
        bool PriceDesc_SaveData(PriceDesc model);
        bool PriceDesc_Clone(int id, int? courtId);
        bool PriceDesc_Delete(int id);


        IQueryable<PriceColVM> PriceCol_Select(int pricedesc_id);
        bool PriceCol_SaveData(PriceCol model);
        bool PriceCol_Delete(int id);

        IEnumerable<PriceVal> PriceVal_Select(int pricedesc_id);
        bool PriceVal_SaveData(int pricedesc_id, List<PriceVal> model);
    }
}
