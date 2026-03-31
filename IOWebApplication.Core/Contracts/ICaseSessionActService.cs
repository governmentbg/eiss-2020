using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.Cdn;
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
    public interface ICaseSessionActService : IBaseService
    {
        IQueryable<CaseSessionActVM> CaseSessionAct_Select(int caseSessionId, int? caseId, DateTime? DateFrom,
             DateTime? DateTo, int? year, string caseRegNumber, bool showExpired = false);
        Task<SaveResultVM> CaseSessionAct_SaveData(CaseSessionActEditVM viewModel);
        Task<bool> CaseSessionAct_SaveDispositiv(int id, string dispositiv, string actBlank);
        bool CaseSessionAct_SaveMotiveCreator(int id);
        Task<bool> CheckActPrivateFileAccess(int id, CaseSessionAct model = null);
        /// <summary>
        /// Проверка за достъп до акт
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<(bool canAccess, string lawunitName)> CheckActAccess(int id, CaseSessionAct model = null);
        Task<SaveResultVM> CaseSessionAct_RegisterAct(CaseSessionAct model, string historyType = null);
        Task<CaseSessionActPrintVM> CaseSessionAct_GetForPrint(int id);
        Task<bool> SendForCoordination_Init(int caseSessionActId, long taskId, int coordinationType);
        Task<SaveResultVM> SendForSign_Init(int caseSessionActId, long taskId);
        Task<bool> SendForSignMotives_Init(int caseSessionActId, long taskId);
        Task<ICollection<CaseLawUnit>> GetCaseLawUnitsByAct(int caseSessionActId, int caseSessionId = 0, bool forMotives = false, bool forActPrint = false);
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
        Task<List<SelectListItem>> GetDropDownListAsync(int caseId, bool? IsFinal = null, bool? IsDecreed = null, bool? IsReadyForPublish = null, bool? IsActInforced = null, bool addDefaultElement = true, bool addAllElement = false);

        List<SelectListItem> GetDropDownListForDismisal(int caseId);

        /// <summary>
        /// Зареждане в комбо на актовете от заседания по постановени с определение за отвод/С разпореждане за отвод
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        Task<List<SelectListItem>> GetDropDownListForDismisalAsync(int caseId);

        List<SelectListItem> GetDropDownListForDismisalRequest(int caseId);
        Task<List<SelectListItem>> GetDropDownListForDismisalRequestAsync(int caseId);
        List<SelectListItem> GetDDL_CaseSessionActFromMigration(int CaseId, int CourtId, bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> GetActTypesByCase(int caseSessionId, bool addDefaultElement = true);
        List<SelectListItem> GetActTypesFromCaseByCase(int caseId, int SessionTypeId, bool addDefaultElement = true);

        /// <summary>
        /// Извличане на вид акт по тип
        /// </summary>
        /// <param name="actTypeId"></param>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        public List<SelectListItem> GetActKindsByActType(int actTypeId, int? caseId);

        Task<CaseSessionActCommandVM> CaseSessionActCommand_GetForPrint(int id);
        List<SelectListItem> GetDropDownListForArchive(int caseId, bool addDefaultElement = true, bool addAllElement = false);
        List<DepersonalizationHistoryItem> AutoDepersonalizeAct_GenerateRules(int caseId);
        string AutoDepersonalizeAct(IEnumerable<DepersonalizationHistoryItem> rules, string html);
        CaseSessionActDivorce GetDivorceByActId(int actId);
        (bool result, string errorMessage) CaseSessionActDivorce_SaveData(CaseSessionActDivorce model);
        (bool result, string errorMessage) CaseSessionActDivorce_SaveExpired(ExpiredInfoVM model);
        List<SelectListItem> GetDropDownList_CaseSessionAct(int CaseId, bool finalOnly = true, bool addDefaultElement = true, bool addAllElement = false);
        Task<List<SelectListItem>> GetDropDownList_CaseSessionActEnforced(int CaseId, bool addDefaultElement = true, bool addAllElement = false);
        List<SelectListItem> GetDropDownListBySessionId(int caseSessionId, bool addDefaultElement = true, bool addAllElement = false);
        IQueryable<CaseSessionActELSprVM> CaseSessionActELSpr_Select(int courtId, CaseSessionActELSprFilterVM model);
        Task<List<SelectListItem>> GetDDL_CanAppealAct(int caseId);

        /// <summary>
        /// Извличане на данни за справка за съдебни актове
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<CaseSessionActReportVM> CaseSessionActReport_Select(CaseSessionActReportFilterVM filter);

        DateTime GetLastSignCaseDate(int caseid, int? actId);
        CaseSessionActCompany GetCompanyByActId(int actId);
        (bool result, string errorMessage) CaseSessionActCompany_SaveData(CaseSessionActCompany model);
        CaseSessionAct GetByIdWithOtherData(int actId);
        List<CaseSessionActVM> GetSessionActsFinal(int CaseId);
        IQueryable<CaseSessionActVM> CaseSessionActSpr_Select(int courtId, CaseSessionActFilterVM model, bool forLawUnitCurrent = false);
        CaseSessionAct GetByRelatedActId(int actId);
        bool IsExistCaseSessionActByCase(int CaseId);
        List<SelectListItem> GetDropDownList_CaseSessionActByCaseBySession(int? CaseId, int? CaseSessionId, bool addDefAllOne = false, bool addDefaultElement = true, bool addAllElement = false);
        bool RemoveDepersonalizationInfo(int id);
        bool RemoveMotiveDepersonalizationInfo(int id);
        List<SelectListItem> GetActDirectionItems();
        Task<(bool canAccess, string lawunitName)> CheckActBlankAccess(CaseSessionAct act, NomenclatureConstants.ActAccessMode mode);
        Task<(bool canAccess, string lawunitName)> CheckActBlankAccess(int actId, NomenclatureConstants.ActAccessMode mode);
        Task<SaveResultVM> FixActDeclaration(int actId);
        SaveResultVM testtracking();
        Task<SaveResultVM> LawUnit_SaveOrderBy(int caseSessionActId, int caseSessionId = 0);
        Task<CaseSessionActEditVM> ReadActById(int id);
        List<SelectListItem> GetDDLSelect2_SecretaryList(int caseSessionId, int actId = 0);
        Task<List<CdnItemVM>> SelectPdfFilesBySourceType(int caseId, int? sourceType);
        Task<SaveResultVM> CheckBeforeSignRNFLAct(int actId);
    }
}
