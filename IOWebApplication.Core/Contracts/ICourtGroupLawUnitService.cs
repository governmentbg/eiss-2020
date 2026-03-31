using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface ICourtGroupLawUnitService
    {
        IQueryable<MultiSelectTransferPercentVM> CourtGroupLawUnitSaved(int courtId, int courtGroupId, int groupKind);
        IQueryable<MultiSelectTransferPercentVM> CourtGroupLawUnitForSelect(int courtId, int groupKind);
        Task<bool> CourtGroupLawUnitSaveData(int courtId, int courtGroupId, List<MultiSelectTransferPercentVM> lawUnits);
        IQueryable<CourtLawUnitLoadVM> CourtGroup_LawUnitsHistory_Select(int courtGroupId);
        List<SelectListItem> GetLawUnitsByCourtGroup(int courtGroupId);
    }
}
