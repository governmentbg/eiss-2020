using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaseLawUnitService : IBaseService
    {
        IQueryable<CaseLawUnitVM> CaseLawUnit_Select(int caseId, int? caseSessionId, bool allData = false, bool isManualRoles = false);

        List<SelectListItem> CaseLawUnit_SelectForDropDownList(int caseId, int? caseSessionId);
        List<SelectListItem> CaseLawUnit_OnlyJudge_SelectForDropDownList(int caseId, int? caseSessionId);
        List<SelectListItem> CaseLawUnit_OnlyJudge_SelectForDropDownList_ValueLawUnitId(int caseId, int? caseSessionId);
        bool FillSessionLawUnitFromCase(int caseId, int caseSessionId);
        CheckListViewVM CheckListViewVM_Fill(int caseId, int caseSessionId);
        bool SessionLawUnitFromCase_SaveData(CheckListViewVM model);
        CaseLawUnitDismisal CaseLawUnitDismisal_GetByCaseLawUnitId(int CaseLawUnitId);
        bool CaseLawUnitDismisal_SaveData(CaseLawUnitDismisal model);
        IQueryable<CaseLawUnitDismisalVM> CaseLawUnitDismisal_Select(int caseId);
        List<SelectListItem> CaseLawUnitFreeDismisal_SelectForDropDownList(int caseId, int roleId);
        IQueryable<CaseLawUnitVM> CaseLawUnitByCaseFromSession_Select(int caseId);
        List<SelectListItem> GetJuryForSession_SelectForDropDownList(int caseSessionId);
        List<SelectListItem> CaseLawUnitForCase_SelectForDropDownList(int caseId);
        bool IsFullComposition(int CaseId);
        bool CaseLawUnit_SaveData(CaseLawUnit model);
        bool CaseLawUnit_RefreshData(int CaseId, int CaseSessionId);
        List<SelectListItem> GetDDL_GetJudgeFromCase(int caseId, int? caseSessionId = null);
        List<CaseLawUnit> GetJudgeFromCase(int caseId, int? caseSessionId = null);
        List<SelectListItem> GetDDL_GetListDepartmentFromRealDepartment(int caseId);
        CaseLawUnitChangeDepRolVM GetCaseLawUnitChangeDepRol(int caseId, int? caseSessionId = null);
        bool GetCaseLawUnitChangeDepRol_Save(CaseLawUnitChangeDepRolVM model);
        List<SelectListItem> CaseLawUnitForCaseObligation_SelectForDropDownList(int caseId);

        bool IsExistLawUnitByCase(int CaseId, DateTime DateFrom);
        bool IsExistJudgeReporterByCase(int CaseId, DateTime DateFrom);


        /// <summary>
        /// Зарежда списък от замествания на съдии по заседание
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        IQueryable<CourtLawUnitSubstitutionVM> LawUnitSubstitution_SelectForSession(int caseSessionId);

        /// <summary>
        /// Прилагане на заместване по заседание
        /// </summary>
        /// <param name="subtsitution_id"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        bool LawUnitSubstitution_Apply(int substsitution_id, int from, int to, int caseSessionId);


        IQueryable<CaseLawUnitManualJudgeVM> LawUnitManualJudge_Select(int? id, DateTime? dateFrom, DateTime? dateTo, string caseNumber, string lawunitName);
        SaveResultVM LawUnitManualJudge_SaveData(CaseLawUnitManualJudge model);
        CaseLawUnit GetJudgeReporter(int caseId);
        List<SelectListItem> GetDDL_LeftSide(int CaseId, bool addDefaultElement = true);
        List<CheckListVM> GetCheckListCaseLawUnitByCase(int caseId);
        List<CheckListVM> GetCheckListCaseLawUnitByCaseAll(int caseId);
        bool IsExistJudgeLawUnitInCase(int CaseId);
    }
}
