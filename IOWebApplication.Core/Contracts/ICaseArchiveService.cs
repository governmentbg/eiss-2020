using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaseArchiveService : IBaseService
    {
        IQueryable<CaseForArchiveVM> CaseForArchive_Select(int courtId, CaseForArchiveFilterVM model);

        IQueryable<CaseArchiveListVM> CaseArchive_Select(int courtId, CaseArchiveFilterVM model);

        bool CaseArchive_SaveData(CaseArchive model, ref string errorMessage, bool forDestroy);

        CaseArchive CaseArchiveByCaseId_Select(int caseId);

        IQueryable<CaseArchiveListVM> CaseForDestroy_Select(int courtId, CaseForDestroyFilterVM model);
    }
}
