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
    public interface ICaseSessionService: IBaseService
    {
        /// <summary>
        /// Изчитане на данни за заседания по дело
        /// </summary>
        /// <param name="CaseId"></param>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <param name="IsVisibleExpired"></param>
        /// <returns></returns>
        IQueryable<CaseSessionListVM> CaseSession_Select(int CaseId, DateTime? DateFrom, DateTime? DateTo, bool IsVisibleExpired = false);

        /// <summary>
        /// Изчитане на данни за заседания по дело
        /// </summary>
        /// <param name="CaseId"></param>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <param name="IsVisibleExpired"></param>
        /// <returns></returns>
        IQueryable<CaseSessionVM> CaseSession_OldSelect(int CaseId, DateTime? DateFrom, DateTime? DateTo, bool IsVisibleExpired = false);

        /// <summary>
        /// Запис на заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<SaveResultVM> CaseSession_SaveData(CaseSessionVM model);

        /// <summary>
        /// Копиране на заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> CaseSession_CopyData(CaseSessionVM model);

        /// <summary>
        /// Изчитане на заседание по ИД
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        CaseSession CaseSessionById(int caseSessionId);

        /// <summary>
        /// Изчитане на заседание по ИД
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        Task<CaseSession> CaseSessionByIdAsync(int caseSessionId);

        /// <summary>
        /// Изчитане на данни по заседание
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        Task<CaseSessionVM> CaseSessionVMById(int caseSessionId);

        /// <summary>
        /// Изчитане на данни по заседание
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        Task<CaseSessionVM> CaseSessionVMByIdAsync(int caseSessionId);

        /// <summary>
        /// Изчитане на данни за резултати по заседание
        /// </summary>
        /// <param name="CaseSessionId"></param>
        /// <param name="IsViewExpired"></param>
        /// <returns></returns>
        IQueryable<CaseSessionResultVM> CaseSessionResult_Select(int CaseSessionId, bool IsViewExpired = false);

        /// <summary>
        /// Запис на резултат по заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> CaseSessionResult_SaveData(CaseSessionResultEditVM model);

        /// <summary>
        /// Проверка дали съществува основен резултат
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <param name="SelectResultId"></param>
        /// <returns></returns>
        bool IsExistMainResult(int caseSessionId, int SelectResultId = 0);

        /// <summary>
        /// Справка за заетост на зали
        /// </summary>
        /// <param name="CourtId"></param>
        /// <param name="CourtHallId"></param>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <returns></returns>
        IQueryable<CaseSessionHallUseVM> CaseSessionHallUse_Select(int CourtId, int? CourtHallId, DateTime? DateFrom, DateTime? DateTo, int? JudgeReporterId);

        /// <summary>
        /// Изчитане на резултати от заседание
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        IQueryable<CaseSessionResultVM> CaseSessionResult_SelectByCaseId(int CaseId);


        Task<bool> IsExistCaseSessionResult(int CaseSessionId);

        /// <summary>
        /// Извличане на данни за резултати от заседание
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        List<SelectListItem> CaseSessionResultStringList_SelectByCaseId(int CaseId);

        /// <summary>
        /// Проверка за заетост на зала
        /// </summary>
        /// <param name="CourtHallId"></param>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo_Minutes"></param>
        /// <param name="ModelId"></param>
        /// <returns></returns>
        Task<bool> CourtHallBusy(int CourtHallId, DateTime DateFrom, int DateTo_Minutes, int ModelId);

        /// <summary>
        /// Справка за заседания
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        IQueryable<CaseSessionVM> CaseSessionSpr_Select(CaseSessionFilterVM model);


        IEnumerable<CalendarVM> CaseSessionSprCalendar_Select(CaseSessionFilterVM model);

        /// <summary>
        /// Извлича данни за срочна книга 
        /// </summary>
        /// <param name="CourtId"></param>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <param name="CaseGroupeId"></param>
        /// <param name="DepartmentId"></param>
        /// <returns></returns>
        IQueryable<CaseSessionTimeBookVM> CaseSessionTimeBook(int CourtId, DateTime DateFrom, DateTime DateTo, int CaseGroupeId, int DepartmentId);

        /// <summary>
        /// Срочна книга в ексел
        /// </summary>
        /// <param name="CourtId"></param>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <param name="CaseGroupeId"></param>
        /// <param name="DepartmentId"></param>
        /// <returns></returns>
        byte[] CaseSessionTimeBook_ToExcel(int CourtId, DateTime DateFrom, DateTime DateTo, int CaseGroupeId, int DepartmentId);

        /// <summary>
        /// Проверка за заетос на състав
        /// </summary>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo_Minutes"></param>
        /// <param name="ModelId"></param>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        string IsBusyLawUnit(DateTime DateFrom, int DateTo_Minutes, int ModelId, int CaseId);

        /// <summary>
        /// Справка за Заседания за период с участието на малолетни/непълнолетни лица
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        IQueryable<CaseSessionSprVM> CaseSessionReportMaturity_Select(int courtId, CaseFilterReport model);

        /// <summary>
        /// Връща списък на заседания по дело за комбо бокс
        /// </summary>
        /// <param name="CaseSessionId"></param>
        /// <returns></returns>
        List<SelectListItem> GetDDL_CaseSessionForCopy(int CaseSessionId);


        List<SelectListItem> GetDropDownList_CaseSessionByCase(int CaseId, DateTime? DateFrom);

        /// <summary>
        /// Справка за Заседания с не написани съдебни актове от всички съдии
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        IQueryable<CaseSprVM> CaseSessionWithActProject_Select(CaseFilterReport filter);

        /// <summary>
        /// Проверка дали заседанието може да бъде сторнирано
        /// </summary>
        /// <param name="CaseSessionId"></param>
        /// <returns></returns>
        bool IsCanExpired(int CaseSessionId);

        /// <summary>
        /// Проверка по ид дали заседанието е последното проведено
        /// </summary>
        /// <param name="CaseSessionId"></param>
        /// <returns></returns>
        bool IsLastConductedSession(int CaseSessionId);
        
        IEnumerable<CalendarVM> CaseSessionHallUseCalendar_Select(int CourtId, int? CourtHallId, DateTime? DateFrom, DateTime? DateТо);
        
        byte[] ListDataSprExportExcel(CaseSessionFilterVM model);
        
        bool CaseSessionResult_ExpiredInfo(ExpiredInfoVM model);
        
        bool CaseSession_ExpiredInfo(ExpiredInfoVM model);
        
        bool IsExistCaseSession(int CaseId);
        
        List<CheckListVM> GetCheckListCaseSession(int caseId);

        Task<List<SelectListItem>> GetDDL_CaseSessionAddAct(int caseId);
        
        CaseSessionResultEditVM GetSessionResultEditVMById(int Id);

        Task<CaseSessionResultEditVM> GetSessionResultEditVMByIdAsync(int Id);

        bool IsExistSessionWithoutAct(int CaseId, int SessionId, int SessionTypeId);
        
        Task<bool> IsExistLastSessionWithoutAct(int CaseId, int SessionId);
        Task<SaveResultVM> CaseSession_MnogoZasedania(int caseId, int courtId);
    }
}
