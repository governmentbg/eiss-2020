// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaseService : IBaseService
    {
        IQueryable<CaseVM> Case_Select(int courtId, CaseFilter model);
        IQueryable<CaseVM> Case_SelectForSelection(int courtId, CaseFilter model);
        bool Case_SaveData(CaseEditVM model);
        CaseVM Case_GetById(long id);
        CaseMigrationVM Case_GetPriorCase(long documentId);
        CaseMigrationVM Case_GetPriorCaseEISPP(long documentId,string eisppNumber);
        bool TestHistory(int id);
        CaseEditVM Case_SelectForEdit(int id);
        bool CheckCaseOldNumber(int CaseGroupId, string oldNumber, DateTime oldDate);

        IQueryable<CaseFolderItemVM> Case_SelectFolder(int id);
        IEnumerable<LabelValueVM> GetCasesByCourt(int courtId, int? caseId, string query);
        IEnumerable<SelectListItem> GetDDL_SessionActsByCase(int caseId, bool addDefaultElement = false);
        CaseElectronicFolderVM CaseElectronicFolder_Select(int CaseId);
        CaseProceedingsVM CaseProceedings_Select(int CaseId);
        bool SaveDataDepersonalizationHistory(int CaseId, IEnumerable<DepersonalizationHistoryItem> model, int actId);
        IEnumerable<DepersonalizationHistoryItem> GetDepersonalizationHistory(int CaseId);
        IQueryable<CaseVM> CaseReport_Select(int courtId, CaseFilterReport model);
        Task<byte[]> CaseArchive(int CaseId);
        IQueryable<CaseSprVM> CaseReportMaturity_Select(int courtId, CaseFilterReport model);
        IQueryable<CaseSprVM> CaseWithoutFinalAct_Select(int courtId, CaseFilterReport model);
        IQueryable<CaseSprVM> CaseWithoutFinal_Select(int courtId, CaseFilterReport model);
        IQueryable<CaseSprVM> CaseCorruptCrimes_Select(int courtId, CaseFilterReport model);
        IQueryable<CaseSprVM> CaseFinalActMaturity_Select(int courtId, bool WithFinalAct, CaseFilterReport model);
        IQueryable<DocumentProvidedReturnedSprVM> DocumentProvidedReturned_Select(int courtId, CaseFilterReport model);
        IQueryable<CaseSprVM> CaseBeginReport_Select(int courtId, CaseFilterReport model);
        IQueryable<CaseSprVM> CaseActReport_Select(int courtId, CaseFilterReport model);
        IQueryable<CaseSprVM> CaseFirstLifecyclie_Select(int courtId, CaseFilterReport model);
        bool IsRegisterCompany(int caseId);

        byte[] CaseWithoutFinalExportExcel(CaseFilterReport model);
        bool IsCaseRestricted(int CaseId);
    }
}
