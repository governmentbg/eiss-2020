using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplicationService.Infrastructure.Contracts;
using IOWebApplicationService.Infrastructure.Data.Common;
using IOWebApplicationService.Infrastructure.Data.DW;
using IOWebApplicationService.Infrastructure.Data.DW.Models;
using IOWebApplicationService.Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Transactions;
using IOWebApplicationService.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Constants;
using MongoDB.Bson;
using IOWebApplicationService.Infrastructure.Transactions;
using IOWebApplication.Infrastructure.Models.Integrations.DW;

namespace IOWebApplicationService.Infrastructure.Services
{
  public class DWSessionService : IDWSessionService
  {
    private readonly IRepository repo;
    private readonly IDWRepository dwRepo;
    private readonly IDWErrorLogService serviceErrorLog;
    private readonly IDWCaseService caseService;
    public DWSessionService(IRepository _repo, IDWRepository _dwRepo, IDWCaseService _caseService, IDWErrorLogService _serviceErrorLog)
    {
      this.repo = _repo;
      this.dwRepo = _dwRepo;
      caseService = _caseService;
      this.serviceErrorLog = _serviceErrorLog;
    }


    public void SessionTransfer(DWCourt court)
    {
      serviceErrorLog.LogError((court.CourtId ?? 0), court.CourtName, "SessionTransfer", 0, "Стартирал");

      IEnumerable<DWCaseSession> dwcasesSessions = SelectCasesSessionForTransfer(DWConstants.DWTransfer.TransferRowCounts, court);
      bool insertRow = true;
      while (dwcasesSessions.Any() && insertRow)
      {
        List<int> updateList = new List<int>();
        foreach (var current_session in dwcasesSessions)
        {
          insertRow = SessionInsertUpdate(current_session, court);
          if (insertRow)
          {

            updateList.Add(current_session.Id);
          }

        }
        dwRepo.SaveChanges();

        UpdateCaseSession(updateList, dwcasesSessions);

        dwcasesSessions = SelectCasesSessionForTransfer(DWConstants.DWTransfer.TransferRowCounts, court);
      }
    }
    private bool UpdateCaseSession(List<int> updateList, IEnumerable<DWCaseSession> dwcasesSessions)
    {
      bool result = false;
      var idd = dwcasesSessions;
      DateTime datewrt = DateTime.Now.Date;
      try
      {
        DateTime dt = DateTime.Now;
        foreach (var item in updateList)
        {
          //  using (TransactionScope tran = DWTransactions.GetTransactionScope())
          {
            var updResult = repo.ExecuteProc<UpdateDateTransferedVM>($"{UpdateDateTransferedVM.ProcedureName}({item},'{UpdateDateTransferedVM.Tables.CaseSession}')");

            //var update = repo.GetById<CaseSession>(item);
            //datewrt = update.DateWrt;
            //update.DateTransferedDW = dt;
            //repo.Update(update);
            //repo.SaveChanges();
            //repo.Detach(update);
            //   tran.Complete();
          }
        }

        //using (TransactionScope tran = DWTransactions.GetTransactionScope())
        //{
        //  DateTime dt = DateTime.Now;
        //  var update = repo.All<CaseSession>().Where(x => updateList.Contains(x.Id)).ToList();
        //  foreach (var up in update)
        //  { //up.DateWrt = up.DateWrt.AddDays(-1);
        //    up.DateTransferedDW = dt;
        //  }

        //  repo.UpdateRange(update);
        //  repo.SaveChanges();
        //  tran.Complete();


        //}

      }
      catch (Exception ex)
      {
        var i = datewrt;
        //throw ex;
      }



      return result;
    }

    public bool SessionInsertUpdate(DWCaseSession current, DWCourt court)
    {
      bool result = false;

      try
      {
        DWCaseSession saved = dwRepo.All<DWCaseSession>().Where(x => x.Id == current.Id).FirstOrDefault();
        if (saved == null)

        {
          current.DateTransferedDW = DateTime.Now;
          // caseService.MergeTransferedlawUnits(current.CaseId, current.Id);
          var lu_session = CaseSessionLawUnitTransfer(court, current);
          current.JudgeReporterId = lu_session.JudgeReporterId;
          current.JudgeReporterName = lu_session.JudgeReporterName;
          current.SessionJudgeStaff = lu_session.SessionJudgeStaff;
          current.SessionJudgeStaff = lu_session.SessionJudgeStaff;
          current.SessionFullJudgeStaff = lu_session.SessionFullJudgeStaff;
          current.SessionJuriStaff = lu_session.SessionJuriStaff;
          current.SessionFullStaff = lu_session.SessionFullStaff;

          dwRepo.Add<DWCaseSession>(current);

          result = true;
        }
        else
        {
          saved.Id = current.Id;
          saved.CompartmentId = current.CompartmentId;
          saved.CompartmentName = current.CompartmentName;
          saved.CourtHallId = current.CourtHallId;
          saved.CourtHallName = current.CourtHallName;
          saved.DateExpired = current.DateExpired;
          saved.DateExpiredStr = current.DateExpiredStr;
          saved.DescriptionExpired = current.DescriptionExpired;
          saved.DateFrom = current.DateFrom;
          saved.DateFromStr = current.DateFromStr;
          saved.DateTo = current.DateTo;
          saved.DateToStr = current.DateToStr;


          saved.SessionStateId = current.SessionStateId;
          saved.SessionStateName = current.SessionStateName;
          saved.SessionTypeId = current.SessionTypeId;
          saved.SessionTypeName = current.SessionTypeName;
          saved.UserExpiredId = current.UserExpiredId;
          saved.UserExpiredName = current.UserExpiredName;
          saved.CaseId = current.CaseId;
          saved.DateTransferedDW = current.DateTransferedDW;
          saved.DateWrt = current.DateWrt;
          saved.Description = current.Description;
          saved.UserId = current.UserId;
          saved.DateReturned = current.DateReturned;

          saved.CourtId = current.CourtId; saved.DwCount = current.DwCount;
          saved.CourtName = current.CourtName;
          saved.CourtRegionId = current.CourtRegionId;
          saved.CourtRegionName = current.CourtRegionName;
          saved.CourtTypeId = current.CourtTypeId;
          saved.CourtTypeName = current.CourtTypeName;
          saved.ParentCourtId = current.ParentCourtId;
          saved.ParentCourtName = current.ParentCourtName;
          saved.EcliCode = current.EcliCode;
          saved.EISPPCode = current.EISPPCode;
          saved.CityCode = current.CityCode;
          saved.CityName = current.CityName;


          //caseService.MergeTransferedlawUnits(current.CaseId, current.Id);

          var lu_session = CaseSessionLawUnitTransfer(court, current);
          saved.JudgeReporterId = lu_session.JudgeReporterId;
          saved.JudgeReporterName = lu_session.JudgeReporterName;
          saved.SessionJudgeStaff = lu_session.SessionJudgeStaff;
          saved.SessionJudgeStaff = lu_session.SessionJudgeStaff;
          saved.SessionFullJudgeStaff = lu_session.SessionFullJudgeStaff;
          saved.SessionJuriStaff = lu_session.SessionJuriStaff;
          saved.SessionFullStaff = lu_session.SessionFullStaff;
          dwRepo.Update<DWCaseSession>(saved);
          result = true;
        }

        if (result)
        {
          CaseSessionResultTransfer(court, current.Id);

        }



      }
      catch (Exception ex)
      {

        serviceErrorLog.LogError((court.CourtId ?? 0), court.CourtName, "case_session", current.Id, ex.Message);
      }

      return result;

    }

    public IEnumerable<DWCaseSession> SelectCasesSessionForTransfer(int selectedRowCount, DWCourt court)
    {
      var act = repo.AllReadonly<CaseSessionAct>();

      Expression<Func<CaseSession, bool>> selectedCourt = x => true;
      if (court.CourtId != null)
        selectedCourt = x => x.Case.CourtId == court.CourtId;

      IEnumerable<DWCaseSession> result = null;
      DateTime oldDate = new DateTime(1900, 01, 01);
      result = repo.AllReadonly<CaseSession>()


                             .Where(selectedCourt)
                             // .Where(x => x.DateExpired == null)
                             .Where(x => x.DateWrt > (x.DateTransferedDW ?? oldDate))
                             .OrderBy(x => x.Case.CourtId).ThenBy(x => x.CaseId)

                              .Select(x => new DWCaseSession()
                              {
                                Id = x.Id,
                                      //CompartmentName = x.Compartment.Label,
                                      CourtHallId = x.CourtHallId,
                                CourtHallName = x.CourtHall.Name,
                                DateExpired = x.DateExpired,
                                DateExpiredStr = x.DateExpired.HasValue ? x.DateExpired.Value.ToString("dd.MM.yyyy HH:mm") : "",
                                DateFrom = x.DateFrom,
                                DateFromStr = x.DateFrom.ToString("dd.MM.yyyy HH:mm"),
                                DateTo = x.DateTo,
                                      // DateToStr = x.DateTo.Value.ToString("dd.MM.yyyy HH:mm"),
                                      DateToStr = x.DateTo.HasValue ? x.DateTo.Value.ToString("dd.MM.yyyy HH:mm") : "",
                                DescriptionExpired = x.DescriptionExpired,
                                SessionStateId = x.SessionStateId,
                                SessionStateName = x.SessionState.Label,
                                SessionTypeId = x.SessionTypeId,
                                SessionTypeName = x.SessionType.Label,
                                UserExpiredId = x.UserExpiredId,
                                UserExpiredName = x.User.LawUnit.FullName,
                                CaseId = x.Case.Id,
                                DateReturned = (act.Any()) ? act.Where(a => a.CaseSessionId == x.Id).OrderByDescending(a => a.ActDeclaredDate).FirstOrDefault().ActDeclaredDate : null,
                                DateTransferedDW = DateTime.Now,
                                DateWrt = x.DateWrt,
                                Description = x.Description,
                                UserId = x.UserId,

                                CourtId = court.CourtId,
                                CourtName = court.CourtName,
                                CourtTypeId = court.CourtTypeId,
                                CourtTypeName = court.CourtTypeName,
                                ParentCourtId = court.ParentCourtId,
                                ParentCourtName = court.ParentCourtName,
                                CourtRegionId = court.CourtRegionId,
                                CourtRegionName = court.CourtRegionName,
                                EcliCode = court.EcliCode,
                                EISPPCode = court.EISPPCode,
                                CityCode = court.CityCode,
                                CityName = court.CityName



                              }).OrderBy(x => x.CourtId).Take(selectedRowCount);


      //foreach (var ccase in result)
      //{ ccase.DWCaseLawUnits = SelectCaseLawUnitsTransfer(ccase.CaseId,null); }


      return result;
    }


    #region Session Result
    public bool CaseSessionResultInsertUpdate(DWCaseSessionResult current)
    {
      bool result = false;

      try
      {
        DWCaseSessionResult saved = dwRepo.All<DWCaseSessionResult>().Where(x => x.Id == current.Id).FirstOrDefault();
        if (saved == null)

        {
          current.DateTransferedDW = DateTime.Now;

          dwRepo.Add<DWCaseSessionResult>(current);

          result = true;
        }
        else
        {
          saved.Id = current.Id;
          saved.CaseId = current.CaseId;

          saved.Description = current.Description;

          saved.IsActive = current.IsActive;
          saved.IsMain = current.IsMain;
          saved.SessionResultBaseId = current.SessionResultBaseId;
          saved.SessionResultBaseName = current.SessionResultBaseName;
          saved.SessionResultId = current.SessionResultId;
          saved.SessionResultName = current.SessionResultName;
          saved.UserExpiredId = current.UserExpiredId;
          saved.UserExpiredName = current.UserExpiredName;




          saved.DateTransferedDW = DateTime.Now;
          saved.DateWrt = current.DateWrt;
          saved.UserId = current.UserId;
          saved.UserName = current.UserName;

          saved.CourtId = current.CourtId; saved.DwCount = current.DwCount;
          saved.CourtName = current.CourtName;
          saved.CourtRegionId = current.CourtRegionId;
          saved.CourtRegionName = current.CourtRegionName;
          saved.CourtTypeId = current.CourtTypeId;
          saved.CourtTypeName = current.CourtTypeName;
          saved.ParentCourtId = current.ParentCourtId;
          saved.ParentCourtName = current.ParentCourtName;
          saved.EcliCode = current.EcliCode;
          saved.EISPPCode = current.EISPPCode;
          saved.CityCode = current.CityCode;
          saved.CityName = current.CityName;
          saved.DateExpired = current.DateExpired;
          saved.DateExpiredStr = current.DateExpiredStr;

          dwRepo.Update<DWCaseSessionResult>(saved);
          result = true;
        }





      }
      catch (Exception ex)
      {

        serviceErrorLog.LogError((current.CourtId ?? 0), current.CourtName, "case_session_result", current.Id, ex.Message);
      }

      return result;

    }

    public IEnumerable<DWCaseSessionResult> SelectCaseSessionResultTransfer(long sessionId, DWCourt court)
    {


      var session_act = repo.AllReadonly<CaseSessionAct>();

      IEnumerable<DWCaseSessionResult> result = null;

      result = repo.AllReadonly<CaseSessionResult>()


                             .Where(x => x.CaseSessionId == sessionId)


                              .Select(x => new DWCaseSessionResult()
                              {
                                Id = x.Id,
                                CaseSessionId = x.CaseSessionId,
                                CaseId = x.CaseId,

                                Description = x.Description,

                                IsActive = x.IsActive,
                                SessionResultBaseId = x.SessionResultBaseId,
                                SessionResultBaseName = x.SessionResultBase.Label,
                                SessionResultId = x.SessionResultId,
                                SessionResultName = x.SessionResult.Label,
                                IsMain = x.IsMain,
                                UserExpiredId = x.UserExpiredId,
                                DateExpired = x.CaseSession.DateExpired,
                                DateExpiredStr = x.CaseSession.DateExpired.HasValue ? x.CaseSession.DateExpired.Value.ToString("dd.MM.yyyy") : "",


                                UserExpiredName = x.UserExpired.LawUnit.FullName,


                                DateTransferedDW = DateTime.Now,


                                CourtId = court.CourtId,
                                CourtName = court.CourtName,
                                CourtTypeId = court.CourtTypeId,
                                CourtTypeName = court.CourtTypeName,
                                ParentCourtId = court.ParentCourtId,
                                ParentCourtName = court.ParentCourtName,
                                CourtRegionId = court.CourtRegionId,
                                CourtRegionName = court.CourtRegionName,
                                EcliCode = court.EcliCode,
                                EISPPCode = court.EISPPCode,
                                CityCode = court.CityCode,
                                CityName = court.CityName

                              });




      return result;
    }
    public void CaseSessionResultTransfer(DWCourt court, long SessionId)
    {
      IEnumerable<DWCaseSessionResult> dwcaseSessionResults = SelectCaseSessionResultTransfer(SessionId, court);




      foreach (var current in dwcaseSessionResults)
      {
        bool insertRow = CaseSessionResultInsertUpdate(current);

      }
      // dwRepo.SaveChanges();





    }

    #endregion
    #region Session Law Unit
    public bool CaseSessionLawUnitInsertUpdate(DWCaseSessionLawUnit current)
    {
      bool result = false;

      try
      {
        DWCaseSessionLawUnit saved = dwRepo.All<DWCaseSessionLawUnit>().Where(x => x.Id == current.Id).FirstOrDefault();
        if (saved == null)

        {

          current.DateTransferedDW = DateTime.Now;
          dwRepo.Add<DWCaseSessionLawUnit>(current);

          result = true;
        }
        else
        {
          saved.Id = current.Id;
          saved.CaseId = current.CaseId;
          saved.CaseId = current.CaseId;
          saved.CaseSessionId = current.CaseSessionId;
          saved.CourtDepartmentId = current.CourtDepartmentId;
          saved.CourtDepartmentId = current.CourtDepartmentId;
          saved.CourtDutyId = current.CourtDutyId;
          saved.CourtDutyName = current.CourtDutyName;
          saved.CourtGroupId = current.CourtGroupId;
          saved.CourtGroupName = current.CourtGroupName;
          saved.DateFrom = current.DateFrom;
          saved.DateTo = current.DateTo;
          saved.DateFromStr = current.DateFromStr;
          saved.DateToStr = current.DateToStr;

          saved.Description = current.Description;
          saved.Id = current.Id;
          saved.JudgeDepartmentRoleId = current.JudgeDepartmentRoleId;
          saved.JudgeDepartmentRoleName = current.JudgeDepartmentRoleName;
          saved.JudgeRoleId = current.JudgeRoleId;
          saved.JudgeRoleName = current.JudgeRoleName;
          saved.LawUnitFullName = current.LawUnitFullName;
          saved.LawUnitId = current.LawUnitId;
          saved.DwCount = current.DwCount;
          saved.DateTransferedDW = DateTime.Now;
          saved.CourtId = current.CourtId; saved.DwCount = current.DwCount;
          saved.CourtName = current.CourtName;
          saved.CourtRegionId = current.CourtRegionId;
          saved.CourtRegionName = current.CourtRegionName;
          saved.CourtTypeId = current.CourtTypeId;
          saved.CourtTypeName = current.CourtTypeName;
          saved.ParentCourtId = current.ParentCourtId;
          saved.ParentCourtName = current.ParentCourtName;
          saved.EcliCode = current.EcliCode;
          saved.EISPPCode = current.EISPPCode;
          saved.CityCode = current.CityCode;
          saved.CityName = current.CityName;
          saved.DateExpired = current.DateExpired;
          saved.DateExpiredStr = current.DateExpiredStr;


          dwRepo.Update<DWCaseSessionLawUnit>(saved);
          result = true;
        }





      }
      catch (Exception ex)
      {

        serviceErrorLog.LogError((current.CourtId ?? 0), current.CourtName, "case_session_lawunit", current.Id, ex.Message);
      }

      return result;

    }

    public IEnumerable<DWCaseSessionLawUnit> SelectCaseSessionLawUnitTransfer(DWCourt court, long sessionId)
    {




      IEnumerable<DWCaseSessionLawUnit> result = null;





      result = repo.AllReadonly<CaseLawUnit>()

                              .Where(x => x.CaseSessionId == sessionId)
                                .Where(x => x.DateTo == null)
                              .Select(x => new DWCaseSessionLawUnit()
                              {
                                Id = x.Id,
                                CaseId = x.CaseId,
                                CaseSessionId = x.CaseSessionId,
                                LawUnitId = x.LawUnitId,
                                LawUnitFullName = x.LawUnit.FullName,
                                JudgeRoleId = x.JudgeRoleId,
                                JudgeRoleName = x.JudgeRole.Label,
                                CourtDepartmentId = x.CourtDepartmentId,
                                CourtDepartmentName = x.CourtDepartment.Label,
                                CourtDutyId = x.CourtDutyId,
                                CourtDutyName = x.CourtDuty.Label,
                                CourtGroupId = x.CourtGroupId,
                                CourtGroupName = x.CourtGroup.Label,
                                JudgeDepartmentRoleId = x.JudgeDepartmentRoleId,
                                JudgeDepartmentRoleName = x.JudgeDepartmentRole.Label,
                                DateFrom = x.DateFrom,
                                DateTo = x.DateTo,
                                DateFromStr = x.DateFrom.ToString("dd.MM.yyyy"),
                                      //DateToStr = x.DateTo.Value.ToString("dd.MM.yyyy"),
                                      DateToStr = x.DateTo.HasValue ? x.DateTo.Value.ToString("dd.MM.yyyy") : "",
                                Description = x.Description,


                                CourtId = court.CourtId,
                                CourtName = court.CourtName,
                                CourtTypeId = court.CourtTypeId,
                                CourtTypeName = court.CourtTypeName,
                                ParentCourtId = court.ParentCourtId,
                                ParentCourtName = court.ParentCourtName,
                                CourtRegionId = court.CourtRegionId,
                                CourtRegionName = court.CourtRegionName,
                                EcliCode = court.EcliCode,
                                EISPPCode = court.EISPPCode,
                                CityCode = court.CityCode,
                                CityName = court.CityName,
                                DateExpired = x.CaseSession.DateExpired,
                                DateExpiredStr = x.CaseSession.DateExpired.HasValue ? x.CaseSession.DateExpired.Value.ToString("dd.MM.yyyy") : "",



                              }
                                );



      return result;
    }
    public DWCaseSession CaseSessionLawUnitTransfer(DWCourt court, DWCaseSession session)
    {
      IEnumerable<DWCaseSessionLawUnit> dw = SelectCaseSessionLawUnitTransfer(court, session.Id);

      DWCaseSession lu_session = new DWCaseSession();

      int JudgeReporterId = 0;
      string JudgeReporterName = "";
      string JudgeReporterStaff = "";
      string SessionFullJudgeStaff = "";
      string SessionJudgeStaff = "";
      string SessionJuriStaff = "";


      foreach (var current in dw)
      {
        bool insertRow = CaseSessionLawUnitInsertUpdate(current);




        if (current.DateTo == null)
        {
          if (current.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
          {
            JudgeReporterId = current.LawUnitId;
            JudgeReporterName = current.LawUnitFullName;
            SessionFullJudgeStaff = SessionFullJudgeStaff + $"{current.LawUnitFullName}({current.JudgeRoleName}); ";
          }
          if (current.JudgeRoleId != NomenclatureConstants.JudgeRole.JudgeReporter && NomenclatureConstants.JudgeRole.JudgeRolesList.Contains(current.JudgeRoleId))
          {
            SessionJudgeStaff = SessionJudgeStaff + $"{current.LawUnitFullName}({current.JudgeRoleName}); ";

          }
          if (NomenclatureConstants.JudgeRole.JuriRolesList.Contains(current.JudgeRoleId))
          {
            SessionJuriStaff = SessionJuriStaff + $"{current.LawUnitFullName}({current.JudgeRoleName}); ";

          }
        }



      }
      lu_session.JudgeReporterId = JudgeReporterId;
      lu_session.JudgeReporterName = JudgeReporterName;
      lu_session.SessionJudgeStaff = SessionJudgeStaff;
      SessionFullJudgeStaff = SessionFullJudgeStaff + SessionJudgeStaff;
      lu_session.SessionFullJudgeStaff = SessionFullJudgeStaff;
      lu_session.SessionJuriStaff = SessionJuriStaff;
      lu_session.SessionFullStaff = SessionFullJudgeStaff + SessionJuriStaff;

      return lu_session;



    }

    #endregion
  }
}
