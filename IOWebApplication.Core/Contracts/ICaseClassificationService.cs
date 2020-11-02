// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaseClassificationService : IBaseService
    {
        IList<CheckListVM> FillCheckListVMs(int caseId, int? caseSessionId);
        CheckListViewVM CaseClassification_SelectForCheck(int caseId, int? caseSessionId);

        bool CaseClassification_SaveData(CheckListViewVM model);

        List<SelectListItem> CaseClassification_Select(int caseId, int? caseSessionId);
        List<CaseClassification> CaseClassification_SelectObject(int caseId, int? caseSessionId);
    }
}
