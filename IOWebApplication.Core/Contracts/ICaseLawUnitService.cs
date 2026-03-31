using IOWebApplication.Infrastructure.Data.Models.Cases;
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
    public interface ICaseLawUnitService : IBaseService
    {
        /// <summary>
        /// Извличане на данни за съдебен състав по дело/заседание
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="caseSessionId">Идентификатор на заседание</param>
        /// <param name="allData">Да връща всички данни</param>
        /// <param name="isManualRoles">Дали да са ръчни роли</param>
        /// <returns></returns>
        IQueryable<CaseLawUnitVM> CaseLawUnit_Select(int caseId, int? caseSessionId, bool allData = false, bool isManualRoles = false);

        /// <summary>
        /// Извличане на кюери за съкратени данни за лица в дело
        /// </summary>
        /// <param name="typePersonGet">Кои лица искаме да извлечем: 1 - Без ръчни роли на служители / 2 - само ръчни роли / 3 - съдия докладчик / всички</param>
        /// <param name="dateTime">Дата към която да се гледат лицата</param>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="caseSessionId">Идентификатор на заседание</param>
        /// <returns></returns>
        IQueryable<CaseLawUnitSmallVM> GetQueryCaseLawUnit(int typePersonGet, DateTime dateTime, int caseId, int? caseSessionId);

        /// <summary>
        /// Извличане на съкратени данни за служители
        /// </summary>
        /// <param name="typePersonGet">Кои лица искаме да извлечем: 1 - Без ръчни роли на служители / 2 - само ръчни роли / всички</param>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="caseSessionId">Идентификатор на заседание</param>
        /// <returns></returns>
        List<CaseLawUnitSmallVM> CaseLawUnitSmallSelect(int typePersonGet, int caseId, int? caseSessionId);

        /// <summary>
        /// Извличане на съкратени данни за служители
        /// </summary>
        /// <param name="typePersonGet">Кои лица искаме да извлечем: 1 - Без ръчни роли на служители / 2 - само ръчни роли / всички</param>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="caseSessionId">Идентификатор на заседание</param>
        /// <returns></returns>
        Task<List<CaseLawUnitSmallVM>> CaseLawUnitSmallSelectAsync(int typePersonGet, int caseId, int? caseSessionId);

        /// <summary>
        /// Извличане на данни за съдебен състав по дело
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="caseSessionId">Идентификатор на заседание</param>
        /// <returns></returns>
        IQueryable<CaseLawUnitVM> CaseLawUnitAll_Select(int caseId, int? caseSessionId);

        List<SelectListItem> CaseLawUnit_SelectForDropDownList(int caseId, int? caseSessionId);
        List<SelectListItem> CaseLawUnit_OnlyJudge_SelectForDropDownList(int caseId, int? caseSessionId);
        List<SelectListItem> CaseLawUnit_OnlyJudge_SelectForDropDownList_ValueLawUnitId(int caseId, int? caseSessionId);
        bool FillSessionLawUnitFromCase(int caseId, int caseSessionId);
        CheckListViewVM CheckListViewVM_Fill(int caseId, int caseSessionId);
        bool SessionLawUnitFromCase_SaveData(CheckListViewVM model);
        CaseLawUnitDismisal CaseLawUnitDismisal_GetByCaseLawUnitId(int CaseLawUnitId);
        bool CaseLawUnitDismisal_SaveData(CaseLawUnitDismisal model, bool automatic_dismisal = false);
        IQueryable<CaseLawUnitDismisalVM> CaseLawUnitDismisal_Select(int caseId);
        List<SelectListItem> CaseLawUnitFreeDismisal_SelectForDropDownList(int caseId, int roleId);
        IQueryable<CaseLawUnitVM> CaseLawUnitByCaseFromSession_Select(int caseId);
        List<SelectListItem> GetJuryForSession_SelectForDropDownList(int caseSessionId);
        List<SelectListItem> CaseLawUnitForCase_SelectForDropDownList(int caseId);
        Task<bool> IsFullComposition(int CaseId);
        Task<bool> CaseLawUnit_SaveData(CaseLawUnit model);
        bool CaseLawUnit_RefreshData(int CaseId, int CaseSessionId);
        List<SelectListItem> GetDDL_GetJudgeFromCase(int caseId, int? caseSessionId = null);
        List<CaseLawUnit> GetJudgeFromCase(int caseId, int? caseSessionId = null);
        List<SelectListItem> GetDDL_GetListDepartmentFromRealDepartment(int caseId);
        CaseLawUnitChangeDepRolVM GetCaseLawUnitChangeDepRol(int caseId, int? caseSessionId = null);
        bool GetCaseLawUnitChangeDepRol_Save(CaseLawUnitChangeDepRolVM model);
        List<SelectListItem> CaseLawUnitForCaseObligation_SelectForDropDownList(int caseId);

        bool IsExistLawUnitByCase(int CaseId, DateTime DateFrom);
        bool IsExistJudgeReporterByCase(int CaseId, DateTime DateFrom);
        bool IsExistManualLawUnitByCase(int CaseId, int LawUnitId, DateTime DateFrom);

        /// <summary>
        /// Зарежда списък от замествания на съдии по заседание
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        IQueryable<CourtLawUnitSubstitutionVM> LawUnitSubstitution_SelectForSession(int caseSessionId);

        /// <summary>
        /// Прилагане на заместване по заседание
        /// </summary>
        /// <param name="substsitution_id"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        bool LawUnitSubstitution_Apply(int substsitution_id, int from, int to, int caseSessionId);

        /// <summary>
        /// Метод извличащ данни за заместване на съдия от случайно разпределение извън дело
        /// </summary>
        /// <param name="sessionId">Идентификатор на заседание</param>
        /// <returns></returns>
        Task<IQueryable<CourtLawUnitSubstitutionVM>> GetCaseSelectionProtokolSubstitution(int sessionId);

        /// <summary>
        /// Запис на заместване (случайно разпределение извън дело)
        /// </summary>
        /// <param name="protokolId">Идентификатор на протокола</param>
        /// <param name="caseSessionId">Идентификатор на заседанието</param>
        /// <returns></returns>
        Task<bool> CaseSelectionProtokolSubstitutionApply(int protokolId, int caseSessionId);

        IQueryable<CaseLawUnitManualJudgeVM> LawUnitManualJudge_Select(int? id, DateTime? dateFrom, DateTime? dateTo, string caseNumber, string lawunitName);
        SaveResultVM LawUnitManualJudge_SaveData(CaseLawUnitManualJudge model);
        CaseLawUnit GetJudgeReporter(int caseId);
        List<SelectListItem> GetDDL_LeftSide(int CaseId, bool addDefaultElement = true);
        List<CheckListVM> GetCheckListCaseLawUnitByCase(int caseId);
        List<CheckListVM> GetCheckListCaseLawUnitByCaseAll(int caseId);
        Task<List<CheckListVM>> GetCheckListCaseLawUnitByCaseAllAsync(int caseId);
        Task<bool> IsExistJudgeLawUnitInCase(int CaseId);
        bool IsExistIsExistManualLawUnitInConductedSession(int caseId, DateTime dateTo, int lawUnitId);
        SaveResultVM CreateAutomaticDismissalForJudgeReporter(int caseId, string description, DateTime dismisalDate);
        SaveResultVM CreateAutomaticDismissalForJudgeChlenSastav(int dismisalLawubitId, int caseId, string description, DateTime dismisalDate);
        int GetJudgeRoleChlenSastav(int dismisalLawubitId, int caseId);

    }
}
