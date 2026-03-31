// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaseSelectionChangeService : IBaseService
    {
        Task<SaveResultVM> DeclareChange(int id);
        Task<SaveResultVM> EnforceChange(int id);
        Task<SaveResultVM> EnforceChangeJudge(int id);
        CaseSelectionChangePrintVM GetModelForProtokol(int id);
        Task<SaveResultVM> ReplaceJudge_SaveData(CaseSelectionChangeFilterVM model);
        IQueryable<CaseSelectionChangeVM> Select(CaseSelectionChangeFilterVM model);
        IQueryable<CaseSelectionChangeListVM> Select_Cases(CaseSelectionChangeFilterVM model);
        IQueryable<CaseSelectionChangeListVM> Select_CasesJudge(CaseSelectionChangeFilterVM model);



    }
}
