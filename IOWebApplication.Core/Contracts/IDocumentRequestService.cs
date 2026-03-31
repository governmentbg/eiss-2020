// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Models.Integrations.EpepFastProcess;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Documents;
using IOWebApplication.Infrastructure.Models.ViewModels.Money;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface IDocumentRequestService : IBaseService
    {
        Task<SaveResultVM> DocumentPersonLinks_SaveData(DocumentPersonLinkVM model);
        Task<DocumentPersonLinkVM> DocumentPersonLinks_Select(long documentId);
        Task<string> GetCityCodeByCompetencyBases417(long documentId, string competencyBaseCode);
        Task<List<SelectListItem>> GetDDL_AllDocumentRequests(int caseId);
        Task<List<SelectListItem>> GetDDL_ClaimCircumstancesCode(string groupCode, bool addAllItem = false, string allItem = "Изберете");
        Task<List<SelectListItem>> GetDDL_DocumentSideList(long documentId, int caseId, int? sideType = null);
        Task<IBaseRequestVM> GetDocumentRequestById(long documentId, int caseId, bool savedOnly = false);
        void InitDbEuro(DbEuroConfigVM euroConfig);
        Task InitRequestSides(IBaseRequestVM request);
        Task<List<NomenclatureItemVM>> LoadAliasNomenclatures(string[] aliasList, IBaseRequestVM request);
        Task<SaveResultVM> SaveDocumentRequest(IBaseRequestVM request, bool recalcExpense = false);
        Task<SaveResultVM> SelectDocumentDataToCase(int caseId, long documentId);
        Task<List<ValidationErrorModel>> ValidateFastProcessRequestData(FastProcessRequestVM model);
    }
}
