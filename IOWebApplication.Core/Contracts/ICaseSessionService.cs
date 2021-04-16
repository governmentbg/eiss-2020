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
    public interface ICaseSessionService: IBaseService
    {
        IQueryable<CaseSessionListVM> CaseSession_Select(int CaseId, DateTime? DateFrom, DateTime? DateTo, bool IsVisibleExpired = false);
        IQueryable<CaseSessionVM> CaseSession_OldSelect(int CaseId, DateTime? DateFrom, DateTime? DateTo, bool IsVisibleExpired = false);
        SaveResultVM CaseSession_SaveData(CaseSessionVM model);
        bool CaseSession_CopyData(CaseSessionVM model);
        CaseSession CaseSessionById(int caseSessionId);
        CaseSessionVM CaseSessionVMById(int caseSessionId);
        IQueryable<CaseSessionResultVM> CaseSessionResult_Select(int CaseSessionId, bool IsViewExpired = false);
        bool CaseSessionResult_SaveData(CaseSessionResultEditVM model);
        bool IsExistMainResult(int caseSessionId, int SelectResultId = 0);
        IQueryable<CaseSessionHallUseVM> CaseSessionHallUse_Select(int CourtId, int? CourtHallId, DateTime? DateFrom, DateTime? DateТо, int? JudgeReporterId);
        IQueryable<CaseSessionResultVM> CaseSessionResult_SelectByCaseId(int CaseId);
        bool IsExistCaseSessionResult(int CaseSessionId);
        List<SelectListItem> CaseSessionResultStringList_SelectByCaseId(int CaseId);
        bool CourtHallBusy(int CourtHallId, DateTime DateFrom, int DateTo_Minutes, int ModelId);
        IQueryable<CaseSessionVM> CaseSessionSpr_Select(CaseSessionFilterVM model);
        IQueryable<CaseSessionTimeBookVM> CaseSessionTimeBook(int CourtId, DateTime DateFrom, DateTime DateTo, int CaseGroupeId, int DepartmentId);
        byte[] CaseSessionTimeBook_ToExcel(int CourtId, DateTime DateFrom, DateTime DateTo, int CaseGroupeId, int DepartmentId);
        string IsBusyLawUnit(DateTime DateFrom, int DateTo_Minutes, int ModelId, int CaseId);
        IQueryable<CaseSessionSprVM> CaseSessionReportMaturity_Select(int courtId, CaseFilterReport model);
        List<SelectListItem> GetDDL_CaseSessionForCopy(int CaseSessionId);
        IQueryable<CaseSprVM> CaseSessionWithActProject_Select(int courtId, CaseFilterReport model);
        bool IsCanExpired(int CaseSessionId);
        bool IsLastConductedSession(int CaseSessionId);
        IEnumerable<CalendarVM> CaseSessionHallUseCalendar_Select(int CourtId, int? CourtHallId, DateTime? DateFrom, DateTime? DateТо);
        byte[] ListDataSprExportExcel(CaseSessionFilterVM model);
        bool CaseSessionResult_ExpiredInfo(ExpiredInfoVM model);
        bool CaseSession_ExpiredInfo(ExpiredInfoVM model);
        bool IsExistCaseSession(int CaseId);
        List<CheckListVM> GetCheckListCaseSession(int caseId);
        CaseSessionResultEditVM GetSessionResultEditVMById(int Id);
    }
}
