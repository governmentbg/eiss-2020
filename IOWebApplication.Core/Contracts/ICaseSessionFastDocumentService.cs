// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaseSessionFastDocumentService: IBaseService
    {
        IQueryable<CaseSessionFastDocumentVM> CaseSessionFastDocument_Select(int CaseSessionId);
        IQueryable<CaseSessionFastDocument> CaseSessionFastDocument_SelectByInitId(int id);
        bool CaseSessionFastDocument_SaveData(CaseSessionFastDocument model);
        CheckListViewVM CaseSessionFastDocument_SelectForSessionCheck(int CaseSessionFromId, int CaseSessionToId);
        bool CaseSessionFastDocument_SaveSelectForSessionCheck(CheckListViewVM model);
    }
}
