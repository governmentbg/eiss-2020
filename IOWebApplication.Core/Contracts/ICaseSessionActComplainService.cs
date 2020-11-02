// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaseSessionActComplainService: IBaseService
    {
        #region CaseSessionActComplain
        IQueryable<CaseSessionActComplainVM> CaseSessionActComplain_Select(int CaseSessionActId);
        IQueryable<CaseSessionActComplainSprVM> CaseSessionActComplainSpr_Select(DateTime DateFrom, DateTime DateTo, DateTime? DateFromActReturn, DateTime? DateToActReturn, DateTime? DateFromSendDocument, DateTime? DateToSendDocument, int CaseGroupId, int CaseTypeId, string CaseRegNumber, string ActRegNumber, int CaseRegNumFrom, int CaseRegNumTo, int ActComplainIndexId, int ActResultId, int JudgeReporterId);
        /// <summary>
        /// Проверка по текущото дело, дали има обжалване с този съпровождащ документ
        /// </summary>
        /// <param name="CaseId">ИД на текущо дело</param>
        /// <param name="ComplainDocumentId">ИД на съпровождащ документ</param>
        /// <returns></returns>
        bool IsExistComplain(int CaseId, long ComplainDocumentId);
        bool CaseSessionActComplain_SaveData(CaseSessionActComplain model);
        bool CaseSessionActComplain_CreateFromDocument(long DocumentId);
        List<SelectListItem> GetDropDownList_GetDocumentCaseInfo(int CaseSessionId, bool addDefaultElement = true, bool addAllElement = false);
        List<CheckListVM> GetCheckListCaseSessionActComplains(int CaseSessionActComplainId, int CaseSessionActId);
        bool IsExistComplainByDocumentIdDifferentStatusRecived(long DocumentId);
        #endregion

        #region CaseSessionActComplainResult
        IQueryable<CaseSessionActComplainResultVM> CaseSessionActComplainResult_Select(int CaseSessionActComplainId);
        bool CaseSessionActComplainResult_SaveData(CaseSessionActComplainResultEditVM model);
        List<SelectListItem> GetDropDownList_CaseSessionActFromCaseSessionActComplainResult(int CaseId, bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> GetDropDownList_ActResultFromCaseSessionActComplainResult(int CaseSessionActId, bool addDefaultElement = true, bool addAllElement = false);
        CaseSessionActComplainResultEditVM CaseSessionActComplainResult_GetById(int Id);
        #endregion

        #region CaseSessionActComplainPerson
        IQueryable<CaseSessionActComplainPersonVM> CaseSessionActComplainPerson_Select(int CaseSessionActComplainId);
        bool CaseSessionActComplainPerson_SaveData(CheckListViewVM model);
        CheckListViewVM CheckListViewVM_FillCasePerson(int CaseSessionActComplainId);
        List<SelectListItem> GetDropDownListForAct(int caseSessionActId, bool addDefaultElement = true, bool addAllElement = false);
        #endregion
    }
}
