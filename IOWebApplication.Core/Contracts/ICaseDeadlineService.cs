// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaseDeadlineService : IBaseService
    {
        CaseDeadline DeadLineDeclaredForResolveStart(CaseSessionResult sessionResult);
        List<CaseDeadline> DeadLineDeclaredForResolveExpire(CaseSessionResult sessionResult);
        void DeadLineDeclaredForResolve(CaseSessionResult sessionResult);
        void DeadLineDeclaredForResolveComplete(Case aCase);
        IQueryable<CaseDeadLineVM> CaseDeadLineSelect(CaseDeadLineFilterVM filter);
        void DeadLineOpenSessionResult(CaseSession session);
        void DeadLineeOpenSessionResultComplete(CaseSessionResult sessionResult);
        void DeadLineMotive(CaseSessionAct sessionAct);
        void DeadLineCompanyCase(Case companyCase);
        void DeadLineOpenSessionResult(CaseSessionMeetingUser user);
        void DeadLineCompanyCaseCompleteOnResult(CaseSessionResult sessionResult);
        void DeadLineCompanyCaseStartOnDocument(Document document);
        void DeadLineCompanyCaseByCaseId(int caseId);
    }
}
