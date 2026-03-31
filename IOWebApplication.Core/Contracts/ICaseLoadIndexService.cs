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
        CaseLoadIndexVM CaseLoadIndexVM_ByID(int id);

        /// <summary>
        /// Натовареност по дела: основни и допълнителни дейности
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<CaseLoadIndexSprVM> CaseLoadIndexSpr_Select(CaseLoadIndexFilterVM filter);

        bool CaseLoadIndex_SaveData(CaseLoadIndex model);
        bool CaseLoadIndexAutomationElementGroupeND_SaveData(int CaseSessionId);
        /// <summary>
        /// Автоматичен запис на натоварване свързано със заседание/резултат от заседание/акт
        /// </summary>
        /// <param name="CaseSessionId"></param>
        /// <returns></returns>
        bool CaseLoadIndexAutomationElementGroupe_SRA_SaveData(int CaseSessionId, List<CaseLoadElementGroup> loadElementGroups = null);
        /// <summary>
        /// Автоматичен запис на натоварване при образуване на дело
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        bool CaseLoadIndexAutomationElementGroupe_CC_SaveData(int CaseId);
        bool IsExistCaseLoadActivity(int ModelId, int CaseId, bool isMainActivity, int JudgeRepLawUnitId, int? caseLoadElementTypeId, int? caseLoadAddActivityId);
        List<SelectListItem> GetDDL_CaseLoadElementGroup(int CaseId, bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> GetDDL_CaseLoadElementType(int CaseLoadElementGroupeId, bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> GetDDL_CaseLoadElementType_Replace(int CurrentId, bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> GetDDL_CaseLoadAddActivity(int CaseId, bool addDefaultElement = true, bool addAllElement = false);
        IQueryable<CaseLoadElementGroupVM> CaseLoadElementGroup_Select();
        CaseLoadElementGroupVM CaseLoadElementGroupVM_ById(int id);
        bool CaseLoadElementGroup_SaveData(CaseLoadElementGroup model);
        IQueryable<CaseLoadElementTypeVM> CaseLoadElementType_Select(int CaseLoadElementGroupId);
        bool CaseLoadElementType_SaveData(CaseLoadElementType model);
        IQueryable<CaseLoadElementTypeRuleVM> CaseLoadElementTypeRule_Select(int CaseLoadElementTypeId);
        bool CaseLoadElementTypeRule_SaveData(CaseLoadElementTypeRule model);
        IQueryable<CaseLoadElementTypeStopVM> CaseLoadElementTypeStop_Select(int CaseLoadElementTypeId);
        bool CaseLoadElementTypeStop_SaveData(CaseLoadElementTypeStop model);
        IQueryable<CaseLoadAddActivityVM> CaseLoadAddActivity_Select();
        bool CaseLoadAddActivity_SaveData(CaseLoadAddActivity model);
        IQueryable<CaseLoadAddActivityIndexVM> CaseLoadAddActivityIndex_Select(int CaseLoadAddActivityId);
        bool CaseLoadAddActivityIndex_SaveData(CaseLoadAddActivityIndex model);
        IEnumerable<LabelValueVM> Get_CaseLoadAddActivity(string term, int? id);
        IQueryable<JudgeLoadActivityVM> JudgeLoadActivity_Select();
        bool JudgeLoadActivity_SaveData(JudgeLoadActivity model);
        IQueryable<JudgeLoadActivityIndexVM> JudgeLoadActivityIndex_Select(int JudgeLoadActivityId);
        bool JudgeLoadActivityIndex_SaveData(JudgeLoadActivityIndex model);
        IQueryable<CourtLawUnitActivityVM> CourtLawUnitActivity_Select(int CourtId, CaseLoadIndexFilterVM model);
        bool CourtLawUnitActivity_SaveData(CourtLawUnitActivity model);
        bool IsExistCourtLawUnitActivity(int LawUnitId, int JudgeLoadActivityId, int ModelId, DateTime ActivityDate);
        bool IsExistCourtLawUnitActivityNew(int LawUnitId, int JudgeLoadActivityId, int ModelId, DateTime ActivityDate, DateTime DateTo);

        /// <summary>
        /// Натоварване на съдии извън дело
        /// </summary>
        /// <param name="dateFrom">От дата</param>
        /// <param name="dateTo">До дата</param>
        /// <param name="lawUnitId">Идентификатор на лице</param>
        /// <param name="judgeLoadActivityId">Идентификатор на judgeLoadActivity</param>
        /// <param name="courtId">Идентификатор на съд</param>
        /// <returns></returns>
        IQueryable<LawUnitLoadSprVM> CourtLawUnitActivitySpr_Select(DateTime dateFrom, DateTime dateTo, int? lawUnitId, int judgeLoadActivityId, int? courtId);

        /// <summary>
        /// Натовареност - извън и в дело
        /// </summary>
        /// <param name="dateFrom">От дата</param>
        /// <param name="dateTo">До дате</param>
        /// <param name="lawUnitId">Идентификатор на лице</param>
        /// <param name="courtId">Идентификатор на съд</param>
        /// <returns></returns>
        IQueryable<LawUnitLoadSprVM> LawUnitActivitySpr_Select(DateTime dateFrom, DateTime dateTo, int? lawUnitId, int? courtId);

        bool ElementTypeRule_Expired(ExpiredInfoVM model);
        bool ElementTypeStop_Expired(ExpiredInfoVM model);
        bool CaseLoadIndexRecalcBaseIndex(int CaseId);
        string RecalcAllCase();
        string RecalcCaseLoadIndexByCase(int CaseId);
        string RecalcAllCourtLawUnitActivity();
        bool IsCaseExistSessionAct(int CaseId);
        bool EditSessionAndRecalcCase(int CaseId, int SessionId);
        bool EditActAndRecalcCase(int CaseId, int ActId);
        bool EditSessionResultAndRecalcCase(int CaseId, int ResultId);
        bool CaseLoadIndex_ExpiredInfo(ExpiredInfoVM model);
        bool CaseLoadIndexAutomationElementGroupeAdditional_SRA_SaveData(int CaseSessionId, List<CaseLoadElementGroup> loadElementGroups = null);
        IQueryable<CaseLoadIndexNewVM> CaseLoadIndexNew_Select(int CaseId, int? CaseSessionId);
        IQueryable<CaseLoadIndexCourtGroupSprVM> CaseLoadIndexCourtGroupSpr_Select(CaseLoadIndexFilterVM model);
    }
}
