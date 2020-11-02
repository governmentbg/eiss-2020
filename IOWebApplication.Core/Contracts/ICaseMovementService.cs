// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaseMovementService: IBaseService
    {
        IEnumerable<CaseMovementVM> Select(int CaseId);
        CaseMovementVM GetById_CaseMovementVM(int Id);
        bool CreateMovement(CaseMovementVM model);
        bool StornoMovement(CaseMovementVM model);
        bool EditAcceptMovement(CaseMovementVM model);
        bool AcceptMovement(int Id);
        bool IsAddNewMovement(int CaseId);
        int CreateReturnMovement(int Id);
        IEnumerable<CaseMovementVM> Select_ToDo();
        int Select_ToDoCount();
        IQueryable<CaseMovementVM> Select_Spr(int courtId, string CaseRegNum, string UserId);
        string GetLastMovmentForCaseId(int CaseId);
    }
}
