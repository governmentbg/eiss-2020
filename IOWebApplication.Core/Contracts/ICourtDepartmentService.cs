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
    public interface ICourtDepartmentService: IBaseService
    {
        IQueryable<CourtDepartmentVM> CourtDepartment_Select(int courtId, string label);
        IQueryable<CourtDepartmentLawUnitVM> CourtDepartmentLawUnit_Select(int courtDepartmentId);
        bool CourtDepartment_SaveData(CourtDepartment model);
        bool CourtDepartmentLawUnit_SaveData(CourtDepartmentLawUnit model);
        List<SelectListItem> GetDropDownList(int courtId);
        CheckListViewVM CheckListViewVM_Fill(int courtId, int departmentId);
        bool CourtDepartmentLawUnit_SaveData(CheckListViewVM model);
        bool StornoCourtDepartment(int CourtDepartmentId);
        List<SelectListItem> Department_SelectDDL(int courtId, int departmentTypeId);
        IQueryable<CourtDepartmentVM> CourtDepartmentByLawUnit_Select(int LawUnitId, int CourtId);
    }
}
