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
    public interface ICourtArchiveService : IBaseService
    {
        IQueryable<CourtArchiveCommittee> CourtArchiveCommittee_Select(int courtId);
        bool CourtArchiveCommittee_SaveData(CourtArchiveCommittee model, List<int> lawUnitIds);
        IQueryable<MultiSelectTransferVM> CourtArchiveCommitteeLawUnit_Select(int committeeId);
        IQueryable<CourtArchiveIndexVM> CourtArchiveIndex_Select(int courtId);
        CourtArchiveIndexEditVM GetByIdVM(int id);
        (bool result, string errorMessage) CourtArchiveIndex_SaveData(CourtArchiveIndexEditVM model, List<int> codeIds);
        IQueryable<MultiSelectTransferVM> CourtArchiveIndexCode_Select(int indexId);
        List<SelectListItem> ArchiveCommittee_SelectDDL(int courtId);
        List<SelectListItem> ArchiveIndex_SelectDDL(int courtId, int caseCodeId);
    }
}
