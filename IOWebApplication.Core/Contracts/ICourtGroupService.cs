using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface ICourtGroupService : IBaseService
    {
        IQueryable<CourtGroupVM> CourtGroup_Select(int courtId, int caseGroupId);
        CourtGroupVM GetCourtGroupVMById(int Id);
        bool CourtGroup_SaveData(CourtGroup model);

        IQueryable<MultiSelectTransferPercentVM> CourtGroupForSelect_Select(int courtId, int caseGroupId);

        List<SelectListItem> CourtGroup_SelectForDropDownList(int courtId, int CaseCodeId);

    }
}
