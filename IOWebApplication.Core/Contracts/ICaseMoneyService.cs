using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaseMoneyService : IBaseService
    {
        IQueryable<CaseMoneyListVM> CaseMoney_Select(int caseId, int? caseSessionId);

        bool CaseMoney_SaveData(CaseMoney model);

    }
}
