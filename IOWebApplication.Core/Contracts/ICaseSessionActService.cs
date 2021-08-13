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
    public interface ICaseSessionActService : IBaseService
    {
        IQueryable<CaseSessionActVM> CaseSessionAct_Select(int caseSessionId, int? caseId, DateTime? DateFrom,
             DateTime? DateTo, int? year, string caseRegNumber, bool showExpired = false);
        SaveResultVM CaseSessionAct_SaveData(CaseSessionAct model);
        bool CaseSessionAct_SaveDispositiv(int id, string dispositiv);
        bool CaseSessionAct_SaveMotiveCreator(int id);
        bool CheckActPrivateFileAccess(int id, CaseSessionAct model = null);
        (bool canAccess, string lawunitName) CheckActBlankAccess(int id, CaseSessionAct model = null);
        (bool canAccess, string lawunitName) CheckMotiveBlankAccess(int id);
        SaveResultVM CaseSessionAct_RegisterAct(int id);
        SaveResultVM CaseSessionAct_RegisterAct(CaseSessionAct model);
        CaseSessionAct CaseSessionAct_GetFullInfo(int id);
        CaseSessionActPrintVM CaseSessionAct_GetForPrint(int id);
        bool SendForCoordination_Init(int caseSessionActId, long taskId);
        SaveResultVM SendForSign_Init(int caseSessionActId, long taskId);
        bool SendForSignMotives_Init(int caseSessionActId, long taskId);
        ICollection<CaseLawUnit> GetCaseLawUnitsByAct(int caseSessionActId, int caseSessionId = 0, bool forMotives = false);
        /// <summary>
        /// Зареждане в комбо на актовете от заседания по ID на дело
        /// </summary>
        /// <param name="caseId">ID на акт</param>
        /// <param name="IsFinal">Финален документ. Ако е null не се взема предвид</param>
        /// <param name="IsDecreed">Постановен. Ако е null не се взема предвид</param>
        /// <param name="IsReadyForPublish">Готов за публикуване. Ако е null не се взема предвид</param>
        /// <param name="IsActInforced">Влязъл в сила. Ако е null не се взема предвид</param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        List<SelectListItem> GetDropDownList(int caseId, bool? IsFinal = null, bool? IsDecreed = null, bool? IsReadyForPublish = null, bool? IsActInforced = null, bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> GetDropDownListForDismisal(int caseId);
        List<SelectListItem> GetDDL_CaseSessionActFromMigration(int CaseId, int CourtId, bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> GetActTypesByCase(int caseSessionId, bool addDefaultElement = true);
        List<SelectListItem> GetActTypesFromCaseByCase(int caseId, int SessionTypeId, bool addDefaultElement = true);
        List<SelectListItem> GetActKindsByActType(int actTypeId);
        CaseSessionActCommandVM CaseSessionActCommand_GetForPrint(int id);
        List<SelectListItem> GetDropDownListForArchive(int caseId, bool addDefaultElement = true, bool addAllElement = false);
        IEnumerable<DepersonalizationHistoryItem> AutoDepersonalizeAct_GenerateRules(CaseSessionAct model);
        string AutoDepersonalizeAct(IEnumerable<DepersonalizationHistoryItem> rules, string html);
        CaseSessionActDivorce GetDivorceByActId(int actId);
        (bool result, string errorMessage) CaseSessionActDivorce_SaveData(CaseSessionActDivorce model);
        (bool result, string errorMessage) CaseSessionActDivorce_SaveExpired(ExpiredInfoVM model);
        List<SelectListItem> GetDropDownList_CaseSessionAct(int CaseId, bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> GetDropDownList_CaseSessionActEnforced(int CaseId, bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> GetDropDownListBySessionId(int caseSessionId, bool addDefaultElement = true, bool addAllElement = false);
        IQueryable<CaseSessionActELSprVM> CaseSessionActELSpr_Select(int courtId, CaseSessionActELSprFilterVM model);
        List<SelectListItem> GetDDL_FinalActToApeal(int caseId);
        List<SelectListItem> GetDDL_CanAppealAct(int caseId);
        IQueryable<CaseSessionActReportVM> CaseSessionActReport_Select(int courtId, CaseSessionActReportFilterVM model);
        DateTime GetLastSignCaseDate(int caseid, int? actId);
        CaseSessionActCompany GetCompanyByActId(int actId);
        (bool result, string errorMessage) CaseSessionActCompany_SaveData(CaseSessionActCompany model);
        CaseSessionAct GetByIdWithOtherData(int actId);
        List<CaseSessionActVM> GetSessionActsFinal(int CaseId);
        IQueryable<CaseSessionActVM> CaseSessionActSpr_Select(int courtId, CaseSessionActFilterVM model, bool forLawUnitCurrent = false);
        CaseSessionAct GetByRelatedActId(int actId);
        bool IsExistCaseSessionActByCase(int CaseId);
        List<SelectListItem> GetDropDownList_CaseSessionActByCaseBySession(int? CaseId, int? CaseSessionId, bool addDefAllOne = false, bool addDefaultElement = true, bool addAllElement = false);
    }
}
