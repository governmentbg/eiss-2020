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
        void DeadLineOnSessionResult(CaseSessionResult sessionResult);
        void DeadLineOnCase(Case caseModel);
        void DeadLineOnSession(CaseSession session);

        IQueryable<CaseDeadLineVM> CaseDeadLineSelect(CaseDeadLineFilterVM filter);
        
        void DeadLineMotive(CaseSessionAct sessionAct);

        void DeadLineOpenSessionResult(CaseSessionMeetingUser user);
        void DeadLineCompanyCaseStartOnDocument(Document document);
        void DeadLineCompanyCaseByCaseId(int caseId);
    }
}
