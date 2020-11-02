// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface ICourtRegionService: IBaseService
    {
        IQueryable<CourtRegionVM> CourtRegion_Select();
        bool CourtRegion_SaveData(CourtRegion model);
        List<SelectListItem> GetDropDownList();
        IQueryable<CourtRegionAreaVM> CourtRegionArea_Select(int CourtRegionId);
        bool CourtRegionArea_SaveData(CourtRegionArea model);

        List<SelectListItem> CourtRegionSelectDDL();
    }
}
