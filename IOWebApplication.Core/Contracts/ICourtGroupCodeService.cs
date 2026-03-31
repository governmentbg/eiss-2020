using IOWebApplication.Infrastructure.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface ICourtGroupCodeService : IBaseService
    {
        IQueryable<MultiSelectTransferVM> CourtGroupCode_Select(int courtId, int courtGroupId, int caseGroupId);
        IQueryable<MultiSelectTransferVM> CourtGroupCodeForSelect_Select(int courtId, int caseGroupId, int caseTypeId);
        Task<bool> CourtGroupCode_SaveData(int courtGroupId, List<int> codes);
    }
}
