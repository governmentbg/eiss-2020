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
        /// <summary>
        /// Извличане на данни за дела
        /// </summary>
        /// <param name="model">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<CaseVM> Case_Select(CaseFilter model);

        IQueryable<CaseVM> Case_SelectForSelection(int courtId, CaseFilter model);
        Task<SaveResultVM> Case_SaveData(CaseEditVM model);

        /// <summary>
        /// Изчитане на данни за дело по ид
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<CaseVM> Case_GetById(int id);

        /// <summary>
        /// Изчитане на данни за дело по ид
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<CaseForNotificationVM> Case_GetByIdForNotification(int id);

        Task<CaseMigrationVM> Case_GetPriorCase(long documentId);
        CaseMigrationVM Case_GetPriorCaseEISPP(long documentId, string eisppNumber);
        bool TestHistory(int id);
        Task<CaseEditVM> Case_SelectForEdit(int id);
        bool CheckCaseOldNumber(int CaseGroupId, string oldNumber, DateTime oldDate);
        Task<IQueryable<CaseFolderItemVM>> Case_SelectFolder(int id);
        IEnumerable<LabelValueVM> GetCasesByCourt(int courtId, int? caseId, string query);
        IEnumerable<SelectListItem> GetDDL_SessionActsByCase(int caseId, bool addDefaultElement = false, bool notRegistered = false, bool declaredOnly = true);
        Task<CaseElectronicFolderVM> CaseElectronicFolder_Select(int caseId);
        Task<CaseProceedingsVM> CaseProceedings_Select(int CaseId);
        bool SaveDataDepersonalizationHistory(int CaseId, IEnumerable<DepersonalizationHistoryItem> model, int sourceType, int sourceId, bool saveDepersonalizeUser);
        IEnumerable<DepersonalizationHistoryItem> GetDepersonalizationHistory(int CaseId);
        IQueryable<CaseVM> CaseReport_Select(int courtId, CaseFilterReport model);
        Task<byte[]> CaseArchive(int CaseId);

        /// <summary>
        /// Справка за образувани дела с участието на малолетни/непълнолетни лица
        /// </summary>
        /// <param name="model">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<CaseSprVM> CaseReportMaturity_Select(CaseFilterReport model);

        IQueryable<CaseSprVM> CaseWithoutFinalAct_Select(int courtId, CaseFilterReport model);

        /// <summary>
        /// Справка за nесвършени дела към дата 
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<CaseSprVM> CaseWithoutFinal_Select(CaseFilterReport filter);

        /// <summary>
        /// Експорт на ексел справка несвършени дела към дата
        /// </summary>
        /// <param name="model">Филтър попълнен от потребител</param>
        /// <returns></returns>
        byte[] CaseWithoutFinalExportExcel(CaseFilterReport model);

        /// <summary>
        /// Експорт на ексел справка несвършени дела към дата със съд
        /// </summary>
        /// <param name="model">Филтър попълнен от потребител</param>
        /// <returns></returns>
        byte[] CaseWithoutFinalWithCourtExportExcel(CaseFilterReport model);

        /// <summary>
        /// Справка oбразувани и свършени дела за корупционни престъпления
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<CaseSprVM> CaseCorruptCrimes_Select(CaseFilterReport filter);

        /// <summary>
        /// Справка Свършили/Несвършили дела с участието на малолетни/непълнолетни лица
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<CaseSprVM> CaseFinalActMaturity_Select(CaseFilterReport filter);

        IQueryable<DocumentProvidedReturnedSprVM> DocumentProvidedReturned_Select(int courtId, CaseFilterReport model);

        /// <summary>
        /// Справка за срочност за насрочване на дела
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<CaseSprVM> CaseBeginReport_Select(CaseFilterReport filter);

        IQueryable<CaseSprVM> CaseActReport_Select(int courtId, CaseFilterReport model);

        /// <summary>
        /// Справка за Справка за времетраене на размяната на книжата (първи интервал)
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<CaseSprVM> CaseFirstLifecyclie_Select(CaseFilterReport filter);

        Task<bool> IsRegisterCompany(int caseId);

        bool IsCaseRestricted(int CaseId);
        IEnumerable<DepersonalizationHistoryItem> GetSimilarDepersonalizationHistory(int CaseId);
        bool IsDoneCase(int CaseId);

        /// <summary>
        /// Метод връщащ информация за дело
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        Task<CaseDataInfoVM> GetCaseInfo(int caseId);
        Task<bool> CheckCaseCodeForDebtorsCount(int caseCodeId);

        /// <summary>
        /// Зареждане на информация за обединяване на дела
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        Task<MergerCaseFastProcessVM> GetMergerCaseFastProcess(int caseId);

        /// <summary>
        /// Метод връщащ инфо за дело за обединяване на дела за бързо производство
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        Task<string> GetFromCaseInfoForMergerCaseFastProcess(int caseId);

        /// <summary>
        /// Обединяване на дела за бързо производство
        /// </summary>
        /// <param name="model">Model popylnen ot potrebitel</param>
        /// <returns></returns>
        Task<bool> MergerCaseFastProcess(MergerCaseFastProcessVM model);

        /// <summary>
        /// Извличане на данни за дела с еднакъв състав с текущо дело
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="regDate">Дата на регистрация на дело</param>
        /// <returns></returns>
        Task<IQueryable<CaseVM>> GetExsistCaseWithSamePeople(int caseId, DateTime regDate);

        /// <summary>
        /// Извличане на данни за еднаквост на дело
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        IQueryable<CaseVM> GetCaseSimiliarCase(int caseId);

        Task<string[]> GetSimilarCase(int caseId, DateTime regDate);
    }
}
