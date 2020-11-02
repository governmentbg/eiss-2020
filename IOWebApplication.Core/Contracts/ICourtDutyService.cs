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
    public interface ICourtDutyService: IBaseService
    {
        IQueryable<CourtDutyVM> CourtDuty_Select(int courtId, string label);
        bool CourtDuty_SaveData(CourtDuty model);
        CheckListViewVM CheckListViewVM_Fill(int courtId, int dutyId);
        bool CourtDutyLawUnit_SaveData(CheckListViewVM model);

        List<SelectListItem> CourtDuty_SelectForDropDownList(int courtId);

    }
}
