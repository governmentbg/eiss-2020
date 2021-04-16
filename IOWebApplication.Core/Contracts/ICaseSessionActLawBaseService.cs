using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaseSessionActLawBaseService : IBaseService
    {
        IQueryable<CaseSessionActLawBaseVM> CaseSessionActLawBase_Select(int caseSessionActId);

        bool CaseSessionActLawBase_SaveData(CaseSessionActLawBase model);
    }
}
