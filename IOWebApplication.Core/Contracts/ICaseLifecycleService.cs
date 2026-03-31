using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaseLifecycleService : IBaseService
    {
        IQueryable<CaseLifecycleVM> CaseLifecycle_Select(int CaseId);

        /// <summary>
        /// Запис на интервал
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> CaseLifecycle_SaveData(CaseLifecycle model);

        bool CaseLifecycle_SaveFirst_ForCaseType(int CaseSessionId, DateTime? lifecycleDateClosed = null);
        Task<bool> CaseLifecycle_CloseInterval(int CaseId, int CaseSessionActId, DateTime DateToLifeCycle);
        bool CaseLifecycle_UndoCloseInterval(int CaseId, int CaseSessionActId);
        bool CaseLifecycle_IsExistLifcycleAfter(int CaseId, int CaseSessionActId);
        bool CaseLifecycle_NewIntervalSave(int CaseId, DateTime DateFromLifeCycle, int? migrationId);
        bool CaseLifecycle_IsAllLifcycleClose(int CaseId);
        DateTime? GetDateTimeLastCaseLifecycle(int CaseId);

        /// <summary>
        /// Стартиране на интервал за медиация
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        Task<bool> StartLifecycleMediation(int caseId);

        /// <summary>
        /// Стартиране на интервал за медиация
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        Task<bool> StopLifecycleMediation(int caseId);
    }
}
