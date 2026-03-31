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
    public interface ICaseSessionMeetingService: IBaseService
    {
        IQueryable<CaseSessionMeetingVM> CaseSessionMeeting_Select(int caseSessionId, bool IsVisibleExpired = false);
        IQueryable<CaseSessionMeetingVM> CaseSessionMeeting_SelectByCaseId(int caseId);
        CaseSessionMeeting CaseSessionMeeting_ById(int Id);
        Task<CaseSessionMeetingEditVM> CaseSessionMeetingEdit_ById(int Id);
        Task<bool> CaseSessionMeeting_SaveData(CaseSessionMeetingEditVM model);
        Task<List<CheckListVM>> GetCheckListCaseSessionMeetingUser(int caseSessionId, int CaseSessionMeetingId = 0);
        IQueryable<CaseSessionMeetingUserVM> CaseSessionMeetingUser_Select(int CaseSessionMeetingId);
        bool CaseSessionMeetingUser_SaveData(CaseSessionMeetingUser model);
        List<SelectListItem> GetDDL_MeetingUserBySessionId(int caseSessionId, bool addDefaultElement = true, bool addAllElement = false);
        bool IsExistMeetengInSession(DateTime DateFrom, DateTime DateTo, int CaseSessionId, int MeetingId = 0);
        CaseSessionMeeting CaseSessionMeetingAutoCreateGetBySessionId(int CaseSessionId);
        Task<bool> CourtHallBusy(int CourtHallId, DateTime DateFrom, DateTime DateTo, int CaseSessionId);
        Task<bool> CourtHallBusyFromSession(int CourtHallId, DateTime DateFrom, int DateTo_Minutes, int CaseSessionId);
        Task<string> IsCaseLawUnitFromCaseBusy(int caseId, int caseSessionId, DateTime dateTimeFrom, DateTime dateTimeTo);
        bool IsExistMeetengInSessionAfterDate(DateTime DateTo, int CaseSessionId, int? CaseSessionMeetingId);
        Task<bool> CheckExistSecretaryOfAllMeeting(int caseSessionId);
        bool CaseSessionMeeting_ExpiredInfo(ExpiredInfoVM model);
    }
}
