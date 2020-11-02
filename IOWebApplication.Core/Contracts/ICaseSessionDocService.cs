// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaseSessionDocService: IBaseService
    {
        IQueryable<CaseSessionDocVM> CaseSessionDoc_Select(int CaseSessionId);
        bool CaseSessionDoc_SaveData(CaseSessionDoc model);
        bool CaseSessionDoc_SaveDataAdd(CheckListViewVM model);
        CheckListViewVM CheckListViewVM_Fill(int CaseSessionId);
        IQueryable<CaseSessionDocVM> CaseSessionDocByCaseId_Select(int CaseId);
        List<SelectListItem> GetDDL_CaseSessionDoc(int CaseSessionId, bool addDefaultElement = true, bool addAllElement = false);
        bool IsExistDocumentIdDifferentStatusNerazgledan(long DocumentId);
    }
}
