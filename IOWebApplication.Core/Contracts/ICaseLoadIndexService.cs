// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Report;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaseLoadIndexService : IBaseService
    {
        IQueryable<CaseLoadIndexVM> CaseLoadIndex_Select(int CaseId, int? CaseSessionId);
        IQueryable<CaseLoadIndexSprVM> CaseLoadIndexSpr_Select(DateTime DateFrom, DateTime DateTo, int? LawUnitId);
        bool CaseLoadIndex_SaveData(CaseLoadIndex model);
        bool CaseLoadIndexAutomationElementGroupeND_SaveData(int CaseSessionId);
        /// <summary>
        /// Автоматичен запис на натоварване свързано със заседание/резултат от заседание/акт
        /// </summary>
        /// <param name="CaseSessionId"></param>
        /// <returns></returns>
        bool CaseLoadIndexAutomationElementGroupe_SRA_SaveData(int CaseSessionId);
        /// <summary>
        /// Автоматичен запис на натоварване при образуване на дело
        /// </summary>
        /// <param name="CaseSessionId"></param>
        /// <returns></returns>
        bool CaseLoadIndexAutomationElementGroupe_CC_SaveData(int CaseId);
        bool IsExistCaseLoadActivity(int ModelId, int CaseId, bool isMainActivity, int JudgeRepLawUnitId, int? caseLoadElementTypeId, int? caseLoadAddActivityId);
        List<SelectListItem> GetDDL_CaseLoadElementGroup(int CaseId, bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> GetDDL_CaseLoadElementType(int CaseLoadElementGroupeId, bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> GetDDL_CaseLoadElementType_Replace(int CurrentId, bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> GetDDL_CaseLoadAddActivity(int CaseId, bool addDefaultElement = true, bool addAllElement = false);
        IQueryable<CaseLoadElementGroupVM> CaseLoadElementGroup_Select();
        bool CaseLoadElementGroup_SaveData(CaseLoadElementGroup model);
        IQueryable<CaseLoadElementTypeVM> CaseLoadElementType_Select(int CaseLoadElementGroupId);
        bool CaseLoadElementType_SaveData(CaseLoadElementType model);
        IQueryable<CaseLoadElementTypeRuleVM> CaseLoadElementTypeRule_Select(int CaseLoadElementTypeId);
        bool CaseLoadElementTypeRule_SaveData(CaseLoadElementTypeRule model);
        IQueryable<CaseLoadAddActivityVM> CaseLoadAddActivity_Select();
        bool CaseLoadAddActivity_SaveData(CaseLoadAddActivity model);
        IQueryable<CaseLoadAddActivityIndexVM> CaseLoadAddActivityIndex_Select(int CaseLoadAddActivityId);
        bool CaseLoadAddActivityIndex_SaveData(CaseLoadAddActivityIndex model);
        IEnumerable<LabelValueVM> Get_CaseLoadAddActivity(string term, int? id);
        IQueryable<JudgeLoadActivityVM> JudgeLoadActivity_Select();
        bool JudgeLoadActivity_SaveData(JudgeLoadActivity model);
        IQueryable<JudgeLoadActivityIndexVM> JudgeLoadActivityIndex_Select(int JudgeLoadActivityId);
        bool JudgeLoadActivityIndex_SaveData(JudgeLoadActivityIndex model);
        IQueryable<CourtLawUnitActivityVM> CourtLawUnitActivity_Select(int CourtId);
        bool CourtLawUnitActivity_SaveData(CourtLawUnitActivity model);
        bool IsExistCourtLawUnitActivity(int LawUnitId, int JudgeLoadActivityId, int ModelId, DateTime ActivityDate);
        IQueryable<LawUnitLoadSprVM> CourtLawUnitActivitySpr_Select(DateTime DateFrom, DateTime DateTo, int? LawUnitId);
        IQueryable<LawUnitLoadSprVM> LawUnitActivitySpr_Select(DateTime DateFrom, DateTime DateTo, int? LawUnitId);
        bool ElementTypeRule_Expired(ExpiredInfoVM model);
    }
}
