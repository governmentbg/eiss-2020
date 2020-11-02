// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaseLifecycleService: IBaseService
    {
        IQueryable<CaseLifecycleVM> CaseLifecycle_Select(int CaseId);
        bool CaseLifecycle_SaveData(CaseLifecycle model);
        bool CaseLifecycle_SaveFirst(int CaseId, DateTime dateTime);
        bool CaseLifecycle_SaveFirst_ForCaseType(int CaseSessionId);
        bool CaseLifecycle_CloseInterval(int CaseId, int CaseSessionActId, DateTime DateToLifeCycle);
        bool CaseLifecycle_UndoCloseInterval(int CaseId, int CaseSessionActId);
        bool CaseLifecycle_IsExistLifcycleAfter(int CaseId, int CaseSessionActId);
        bool CaseLifecycle_NewIntervalSave(int CaseId, DateTime DateFromLifeCycle);
        int CalcMonthFromDateFromDateTo(DateTime DateFrom, DateTime DateTo);
        int GetCalcMonthToDate(int CaseId, DateTime DateTo);
        bool CaseLifecycle_IsAllLifcycleClose(int CaseId);
    }
}
