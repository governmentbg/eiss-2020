// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

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

namespace IOWebApplicationService.Infrastructure.Services
{
  public class DWSessionActService : IDWSessionActService
  {
    private readonly IRepository repo;
    private readonly IDWRepository dwRepo;
    private readonly IDWCaseService caseService;
    public DWSessionActService(IRepository _repo, IDWRepository _dwRepo, IDWCaseService _caseService)
    {
      this.repo = _repo;
      this.dwRepo = _dwRepo;
      caseService = _caseService;
    }

    #region SessionAct
    public void SessionActTransfer(DWCourt court)
    {
      IEnumerable<DWCaseSessionAct> dwcasesSessionsAct = SelectCasesSessionActForTransfer(DWConstants.DWTransfer.TransferRowCounts, court);


      while (dwcasesSessionsAct.Any())
      {


        foreach (var current_session_act in dwcasesSessionsAct)
        {
          bool insertRow = SessionActInsertUpdate(current_session_act);
          if (insertRow)
          {
            var main_session_act = repo.GetById<CaseSessionAct>(current_session_act.Id);
            main_session_act.DateTransferedDW = DateTime.Now;
            repo.Update<CaseSessionAct>(main_session_act);
            repo.SaveChanges();
            repo.Detach(main_session_act);
          }

        }
        dwRepo.SaveChanges();
      
        //  ts.Complete();
        //}

        dwcasesSessionsAct = SelectCasesSessionActForTransfer(DWConstants.DWTransfer.TransferRowCounts, court);



      }



    }

    public bool SessionActInsertUpdate(DWCaseSessionAct current)
    {
      bool result = false;

      try
      {
        DWCaseSessionAct saved = dwRepo.All<DWCaseSessionAct>().Where(x => x.Id == current.Id).FirstOrDefault();
        if (saved == null)

        {
          current.DateTransferedDW = DateTime.Now;

          dwRepo.Add<DWCaseSessionAct>(current);

          result = true;
        }
        else
        {
          saved.Id = current.Id;
          saved.ActComplainResultId = current.ActComplainResultId;
          saved.ActComplainResultStateId = current.ActComplainResultStateId;
          saved.ActComplainResultStateName = current.ActComplainResultStateName;
          saved.ActDate = current.ActDate;
          saved.ActDateStr = current.ActDateStr;
          saved.ActDeclaredDate = current.ActDeclaredDate;
          saved.ActDeclaredDateStr = current.ActDeclaredDateStr;
          saved.ActInforcedDate = current.ActInforcedDate;
          saved.ActInforcedDateStr = current.ActInforcedDateStr;
          saved.ActKindId = current.ActKindId;
          saved.ActKindName = current.ActKindName;
          saved.ActMotivesDeclaredDate = current.ActMotivesDeclaredDate;
          saved.ActMotivesDeclaredDateStr = current.ActMotivesDeclaredDateStr;
          saved.ActResultId = current.ActResultId;
          saved.ActResultName = current.ActResultName;
          saved.ActStateId = current.ActStateId;
          saved.ActStateName = current.ActStateName;
          saved.ActTypeId = current.ActTypeId;
          saved.ActTypeName = current.ActTypeName;
          saved.CanAppeal = current.CanAppeal;
          saved.CaseSessionId = current.CaseSessionId;
          saved.DepersonalizeUserId = current.DepersonalizeUserId;
          saved.DepersonalizeUserName = current.DepersonalizeUserName;
          saved.DateTransferedDW = current.DateTransferedDW;
          saved.DateWrt = current.DateWrt;
          saved.DateExpired= current.DateExpired ;
          saved.DateExpiredStr = current.DateExpiredStr;
          saved.Description = current.Description;
          saved.EcliCode = current.EcliCode;
          saved.IsFinalDoc = current.IsFinalDoc;
          saved.IsReadyForPublish = current.IsReadyForPublish;
          saved.RegDate = current.RegDate;
          saved.RegDateStr = current.RegDateStr;
          saved.RegNumber = current.RegNumber;
          saved.RegNumberFull = current.RegNumberFull;
          saved.SecretaryUserId = current.SecretaryUserId;
          saved.SecretaryUserName = current.SecretaryUserName;
          saved.UserId = current.UserId;
          saved.UserName = current.UserName;
          saved.CaseId = current.CaseId;
          saved.ActReturnDate = current.ActReturnDate;
          saved.ActReturnDateStr = current.ActReturnDateStr;

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

          dwRepo.Update<DWCaseSessionAct>(saved);
          result = true;
        }





      }
      catch (Exception ex)
      {

        throw;
      }

      return result;

    }

    public IEnumerable<DWCaseSessionAct> SelectCasesSessionActForTransfer(int selectedRowCount, DWCourt court)
    {

      var act_complain_result = repo.AllReadonly<CaseSessionActComplainResult>();

      Expression<Func<CaseSessionAct, bool>> selectedCourt = x => true;
      if (court.CourtId != null)
        selectedCourt = x => x.CaseSession.Case.CourtId == court.CourtId;

      IEnumerable<DWCaseSessionAct> result = null;
      DateTime oldDate = new DateTime(1900, 01, 01);
      result = repo.AllReadonly<CaseSessionAct>()


                             .Where(selectedCourt)
                             .Where(x => x.DateExpired == null)
                             .Where(x=>x.RegDate!=null)
                             .Where(x => x.DateWrt > (x.DateTransferedDW ?? oldDate))


                              .Select(x => new DWCaseSessionAct()
                              {
                                Id = x.Id,
                                ActComplainResultId = x.ActComplainResultId,
                                ActComplainResultStateId = act_complain_result.Where(a => a.CaseSessionActId == x.Id).Select(a => a.ActResultId).FirstOrDefault(),
                                ActComplainResultStateName = act_complain_result.Where(a => a.CaseSessionActId == x.Id).Select(a => a.ActResult.Label).FirstOrDefault(),
                                ActDate = x.ActDate,
                                ActResultId = x.ActResultId,
                                ActResultName = x.ActResult.Label,
                                ActDeclaredDate = x.ActDeclaredDate,
                                ActInforcedDate = x.ActInforcedDate,
                                ActDateStr = x.ActDate.HasValue ? x.ActDate.Value.ToString("dd.MM.yyyy HH:mm") : "",
                                ActDeclaredDateStr= x.ActDeclaredDate.HasValue ? x.ActDeclaredDate.Value.ToString("dd.MM.yyyy HH:mm") : "",
                                ActMotivesDeclaredDateStr = x.ActMotivesDeclaredDate.HasValue ? x.ActMotivesDeclaredDate.Value.ToString("dd.MM.yyyy HH:mm") : "",
                                ActInforcedDateStr = x.ActInforcedDate.HasValue ? x.ActInforcedDate.Value.ToString("dd.MM.yyyy HH:mm") : "",
                                ActKindId = x.ActKindId,
                                ActKindName = x.ActKind.Label,
                                ActMotivesDeclaredDate = x.ActMotivesDeclaredDate,
                                ActStateId = x.ActStateId,
                                ActStateName = x.ActState.Label,
                                ActTypeId = x.ActTypeId,
                                ActTypeName = x.ActType.Label,
                                CanAppeal = x.CanAppeal,
                                CaseSessionId = x.CaseSessionId,
                                CaseId=x.CaseSession.CaseId,
                                DepersonalizeUserId = x.DepersonalizeUserId,
                                DepersonalizeUserName = x.DepersonalizeUser.LawUnit.FullName,
                                ActReturnDate=x.ActReturnDate,
                                ActReturnDateStr= (x.ActReturnDate).HasValue ? (x.ActReturnDate).Value.ToString("dd.MM.yyyy HH:mm") : "",

                                IsFinalDoc = x.IsFinalDoc,
                                IsReadyForPublish = x.IsReadyForPublish,
                                RegDate = x.RegDate,
                                RegDateStr= x.RegDate.HasValue ? x.RegDate.Value.ToString("dd.MM.yyyy") : " ",
                                RegNumber = x.RegNumber,
                                RegNumberFull = $"{x.RegNumber}/{x.RegDate:dd.MM.yyyy}",
                                SecretaryUserId = x.SecretaryUserId,
                                SecretaryUserName = x.SecretaryUser.LawUnit.FullName,
                                DateExpired=x.DateExpired,
                                DateExpiredStr = (x.DateExpired??x.CaseSession.DateExpired).HasValue ? (x.DateExpired ?? x.CaseSession.DateExpired).Value.ToString("dd.MM.yyyy HH:mm") : "",
                                DateTransferedDW = DateTime.Now,
                                DateWrt = x.DateWrt,
                                Description = x.Description,
                                UserId = x.UserId,
                                UserName = x.User.LawUnit.FullName,

                                CourtId = court.CourtId,
                                CourtName = court.CourtName,
                                CourtTypeId = court.CourtTypeId,
                                CourtTypeName = court.CourtTypeName,
                                ParentCourtId = court.ParentCourtId,
                                ParentCourtName = court.ParentCourtName,
                                CourtRegionId = court.CourtRegionId,
                                CourtRegionName = court.CourtRegionName,
                                EcliCode = x.EcliCode,
                                EISPPCode = court.EISPPCode,
                                CityCode = court.CityCode,
                                CityName = court.CityName

                              }).OrderBy(x => x.CourtId).Take(selectedRowCount);




      return result;
    }
    #endregion

    #region SessionActComplain

    public void SessionActComplainTransfer(DWCourt court)
    {
      IEnumerable<DWCaseSessionActComplain> dwcasesSessionsActComplain = SelectCasesSessionActComplainForTransfer(DWConstants.DWTransfer.TransferRowCounts, court);


      while (dwcasesSessionsActComplain.Any())
      {


        foreach (var current in dwcasesSessionsActComplain)
        {
          bool insertRow = SessionActComplainInsertUpdate(current);
          if (insertRow)
          {
            var main = repo.GetById<CaseSessionActComplain>(current.Id);
            main.DateTransferedDW = DateTime.Now;
            repo.Update<CaseSessionActComplain>(main);
            repo.SaveChanges();
            repo.Detach(main);
          }

        }
        dwRepo.SaveChanges();
        //repo.SaveChanges();
        //  ts.Complete();
        //}

        dwcasesSessionsActComplain = SelectCasesSessionActComplainForTransfer(DWConstants.DWTransfer.TransferRowCounts, court);



      }



    }
    public bool SessionActComplainInsertUpdate(DWCaseSessionActComplain current)
    {
      bool result = false;

      DWCaseSessionActComplain outParams = ReadOutDocumentsProperties(current.CaseSessionActId);
      if (outParams != null)
      {
        current.OutComplainDocumentId = outParams.OutComplainDocumentId;
        current.OutComplainDocumentName = outParams.OutComplainDocumentName;
        current.OutComplainDocumentTypeId = outParams.OutComplainDocumentTypeId;
        current.OutComplainDocumentTypeName = outParams.OutComplainDocumentTypeName;
        current.OutDocumentNumber = outParams.OutDocumentNumber;
        current.OutCourtId = outParams.OutCourtId;
        current.OutCourtName = outParams.OutCourtName;
        current.OutDocumentDate = outParams.OutDocumentDate;
        current.OutDocumentDateStr = outParams.OutDocumentDateStr;
      }
      dwRepo.DeleteRange<DWCaseSessionActComplainPerson>(x => x.CaseSessionActComplainId == current.Id);
      try
      {
        DWCaseSessionActComplain saved = dwRepo.All<DWCaseSessionActComplain>().Where(x => x.Id == current.Id).FirstOrDefault();
        if (saved == null)

        {
          current.DateTransferedDW = DateTime.Now;

          dwRepo.Add<DWCaseSessionActComplain>(current);

          result = true;
        }
        else
        {
          saved.Id = current.Id;
          saved.CaseSessionActId = current.CaseSessionActId;
          saved.CaseId = current.CaseId;
          saved.CaseSessionId = current.CaseSessionId;
          saved.ComplainDocumentId = current.ComplainDocumentId;
          saved.ComplainDocumentName = current.ComplainDocumentName;
          saved.ComplainDocumentTypeId = current.ComplainDocumentTypeId;
          saved.ComplainDocumentTypeName = current.ComplainDocumentTypeName;
          saved.ComplainStateId = current.ComplainStateId;
          saved.ComplainStateName = current.ComplainStateName;
          saved.DateTransferedDW = current.DateTransferedDW;
          saved.DateWrt = current.DateWrt;
          saved.RejectDescription = current.RejectDescription;
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
          saved.DocumentDate = current.DocumentDate;
          saved.DocumentDateStr = current.DocumentDateStr;
          saved.DocumentNumber = current.DocumentNumber;

          saved.OutComplainDocumentId = current.OutComplainDocumentId;
          saved.OutComplainDocumentName = current.OutComplainDocumentName;
          saved.OutComplainDocumentTypeId = current.OutComplainDocumentTypeId;
          saved.OutComplainDocumentTypeName = current.OutComplainDocumentTypeName;
          saved.OutDocumentNumber = current.OutDocumentNumber;
          saved.OutCourtId = current.OutCourtId;
          saved.OutCourtName = current.OutCourtName;
          saved.OutDocumentDate = current.OutDocumentDate;
          saved.OutDocumentDateStr = current.OutDocumentDateStr;

          dwRepo.Update<DWCaseSessionActComplain>(saved);
          result = true;
        }





      }
      catch (Exception ex)
      {

        throw;
      }

      return result;

    }
    public IEnumerable<DWCaseSessionActComplain> SelectCasesSessionActComplainForTransfer(int selectedRowCount, DWCourt court)
    {

      //var act_complain_result = repo.AllReadonly<CaseSessionActComplainResult>();

      Expression<Func<CaseSessionActComplain, bool>> selectedCourt = x => true;
      if (court.CourtId != null)
        selectedCourt = x => x.CaseSessionAct.CaseSession.Case.CourtId == court.CourtId;

      IEnumerable<DWCaseSessionActComplain> result = null;
      DateTime oldDate = new DateTime(1900, 01, 01);
      result = repo.AllReadonly<CaseSessionActComplain>()


                             .Where(selectedCourt)

                             .Where(x => x.DateWrt > (x.DateTransferedDW ?? oldDate))


                              .Select(x => new DWCaseSessionActComplain()
                              {
                                Id = x.Id,
                                CaseSessionActId = x.CaseSessionActId,
                                CaseId = x.CaseSessionAct.CaseSession.CaseId,
                                CaseSessionId = x.CaseSessionAct.CaseSession.Id,
                                ComplainDocumentId = x.ComplainDocumentId,
                                ComplainDocumentName = $"{x.ComplainDocument.DocumentNumber}/{x.ComplainDocument.DocumentDate:dd.MM.yyyy}",
                                ComplainDocumentTypeId = x.ComplainDocument.DocumentTypeId,
                                ComplainDocumentTypeName = x.ComplainDocument.DocumentType.Label,
                                DocumentNumber=x.ComplainDocument.DocumentNumber,
                               
                                ComplainStateId = x.ComplainStateId,
                                ComplainStateName = x.ComplainState.Label,
                                RejectDescription = x.RejectDescription,
                                DateExpired = (x.CaseSessionAct.DateExpired??x.CaseSessionAct.CaseSession.DateExpired),
                                DateExpiredStr = (x.CaseSessionAct.DateExpired ?? x.CaseSessionAct.CaseSession.DateExpired).HasValue ? (x.CaseSessionAct.DateExpired ?? x.CaseSessionAct.CaseSession.DateExpired).Value.ToString("dd.MM.yyyy HH:mm") : "",
                                DocumentDate = x.ComplainDocument.DocumentDate,
                                DocumentDateStr = $"{x.ComplainDocument.DocumentDate:dd.MM.yyyy}",

                                DateTransferedDW = DateTime.Now,
                                DateWrt = x.DateWrt,
                                UserId = x.UserId,
                                UserName = x.User.LawUnit.FullName,

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
                                
                                

                              }).OrderBy(x => x.CourtId).Take(selectedRowCount);

      //foreach (var current in result)
      //{
      //  DWCaseSessionActComplain outParams = ReadOutDocumentsProperties(current.CaseSessionActId);
      //  if (outParams != null)
      //  {
      //    current.OutComplainDocumentId = outParams.OutComplainDocumentId;
      //    current.OutComplainDocumentName = outParams.OutComplainDocumentName;
      //    current.OutComplainDocumentTypeId = outParams.OutComplainDocumentTypeId;
      //    current.OutComplainDocumentTypeName = outParams.OutComplainDocumentTypeName;
      //    current.OutDocumentNumber = outParams.OutDocumentNumber;
      //    current.OutCourtId = outParams.OutCourtId;
      //    current.OutCourtName = outParams.OutCourtName;
      //    current.OutDocumentDate = outParams.OutDocumentDate;
      //    current.OutDocumentDateStr = outParams.OutDocumentDateStr;
      //  }
      //}


      return result;
    }

    private DWCaseSessionActComplain ReadOutDocumentsProperties(int actId)
    {
      DWCaseSessionActComplain dw = new DWCaseSessionActComplain();
      dw = repo.AllReadonly<CaseMigration>()
        .Where(x => x.CaseSessionActId == actId)
        .Where(x => x.CaseMigrationType.MigrationDirection == 1)
        .Where(x=>(x.OutDocumentId??0)>0)
        .Where(x=>x.SendToCourtId>0)
         .Select(x => new DWCaseSessionActComplain()
         {
           OutComplainDocumentId = x.OutDocumentId,
           OutComplainDocumentName = $"{x.OutDocument.DocumentNumber}/{x.OutDocument.DocumentDate:dd.MM.yyyy}",
           OutComplainDocumentTypeId = x.OutDocument.DocumentTypeId,
           OutComplainDocumentTypeName = x.OutDocument.DocumentType.Label,
           OutDocumentNumber = x.OutDocument.DocumentNumber,
           OutCourtId = x.SendToCourtId,
           OutCourtName=x.SendToCourt.Label,
           OutDocumentDate = x.OutDocument.DocumentDate,
           OutDocumentDateStr = $"{x.OutDocument.DocumentDate:dd.MM.yyyy}",


         })
      .FirstOrDefault();
      return dw;
    }


    #endregion

    #region SessionActComplainResult

    public void SessionActComplainResultTransfer(DWCourt court)
    {
      IEnumerable<DWCaseSessionActComplainResult> dwcasesSessionsActComplainResult = SelectCasesSessionActComplainResultForTransfer(DWConstants.DWTransfer.TransferRowCounts, court);


      while (dwcasesSessionsActComplainResult.Any())
      {


        foreach (var current in dwcasesSessionsActComplainResult)
        {
          bool insertRow = SessionActComplainResultInsertUpdate(current);
          if (insertRow)
          {
            var main = repo.GetById<CaseSessionActComplainResult>(current.Id);
            main.DateTransferedDW = DateTime.Now;
            repo.Update<CaseSessionActComplainResult>(main);
            repo.SaveChanges();
            repo.Detach(main);
          }

        }
        dwRepo.SaveChanges();
        //repo.SaveChanges();
        //  ts.Complete();
        //}

        dwcasesSessionsActComplainResult = SelectCasesSessionActComplainResultForTransfer(DWConstants.DWTransfer.TransferRowCounts, court);



      }



    }

    public bool SessionActComplainResultInsertUpdate(DWCaseSessionActComplainResult current)
    {
      bool result = false;

      try
      {
        DWCaseSessionActComplainResult saved = dwRepo.All<DWCaseSessionActComplainResult>().Where(x => x.Id == current.Id).FirstOrDefault();
        if (saved == null)

        {
          current.DateTransferedDW = DateTime.Now;

          dwRepo.Add<DWCaseSessionActComplainResult>(current);

          result = true;
        }
        else
        {
          saved.Id = current.Id;
          saved.ActResultId = current.ActResultId;
          saved.ActResultName = current.ActResultName;
          saved.CaseId = current.CaseId;
          saved.CaseSessionId = current.CaseSessionId;
          saved.CaseRegDate = current.CaseRegDate;
          saved.CaseRegNumber = current.CaseRegNumber;
          saved.CaseSessionActComplainId = current.CaseSessionActComplainId;
          saved.CaseSessionActId = current.CaseSessionActId;
          saved.CaseShortNumberValue = current.CaseShortNumberValue;
             saved.CourtId = current.CourtId; saved.DwCount = current.DwCount;
          saved.CourtName = current.CourtName;
          saved.DateResult = current.DateResult;
          saved.DateTransferedDW = current.DateTransferedDW;

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
          
          dwRepo.Update<DWCaseSessionActComplainResult>(saved);
          result = true;
        }





      }
      catch (Exception ex)
      {

        throw;
      }

      return result;

    }

    public IEnumerable<DWCaseSessionActComplainResult> SelectCasesSessionActComplainResultForTransfer(int selectedRowCount, DWCourt court)
    {

      //var act_complain_result = repo.AllReadonly<CaseSessionActComplainResult>();

      Expression<Func<CaseSessionActComplainResult, bool>> selectedCourt = x => true;
      if (court.CourtId != null)

        selectedCourt = x => x.CaseSessionActComplain.CaseSessionAct.CaseSession.Case.CourtId == court.CourtId;

      IEnumerable<DWCaseSessionActComplainResult> result = null;
      DateTime oldDate = new DateTime(1900, 01, 01);
      result = repo.AllReadonly<CaseSessionActComplainResult>()


                             .Where(selectedCourt)

                             .Where(x => x.DateWrt > (x.DateTransferedDW ?? oldDate))


                              .Select(x => new DWCaseSessionActComplainResult()
                              {
                                Id = x.Id,
                                CaseSessionActId = x.CaseSessionActId,
                                ActResultId = x.ActResultId,
                                ActResultName = x.ActResult.Label,
                                DateResult = x.DateResult,
                                CaseId = x.CaseId,
                                CaseSessionId=x.CaseSessionAct.CaseSessionId,
                                CaseRegDate = x.Case.RegDate,
                                CaseRegNumber = x.Case.RegNumber,
                                CaseShortNumberValue = x.Case.ShortNumberValue,
                                CaseSessionActComplainId = x.CaseSessionActComplainId,
                                DateExpired = (x.CaseSessionAct.DateExpired?? x.CaseSessionAct.CaseSession.DateExpired),
                                DateExpiredStr = (x.CaseSessionAct.DateExpired ?? x.CaseSessionAct.CaseSession.DateExpired).HasValue ? (x.CaseSessionAct.DateExpired ?? x.CaseSessionAct.CaseSession.DateExpired).Value.ToString("dd.MM.yyyy HH:mm") : "",

                                Description = x.Description,
                                DateTransferedDW = DateTime.Now,
                                DateWrt = x.DateWrt,
                                UserId = x.UserId,
                                UserName = x.User.LawUnit.FullName,

                                CourtId = x.CourtId,
                                CourtName = x.Court.Label,
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




      return result;
    }
    #endregion

    #region SesssionActComplainPerson
    public bool SessionActComplainPersonInsertUpdate(DWCaseSessionActComplainPerson current)
    {
      bool result = false;

      try
      {
        DWCaseSessionActComplainPerson saved = dwRepo.All<DWCaseSessionActComplainPerson>().Where(x => x.Id == current.Id).FirstOrDefault();
        if (saved == null)

        {
          current.DateTransferedDW = DateTime.Now;

          dwRepo.Add<DWCaseSessionActComplainPerson>(current);

          result = true;
        }
        else
        {
          saved.Id = current.Id;
          saved.CasePersonId = current.CasePersonId;
            saved.CaseId = current.CaseId;
          saved.CaseSessionId = current.CaseSessionId;

          saved.CasePersonName = current.CasePersonName;
          saved.CaseSessionActComplainId = current.CaseSessionActComplainId;

          saved.DateTransferedDW = current.DateTransferedDW;

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

          dwRepo.Update<DWCaseSessionActComplainPerson>(saved);
          result = true;
        }





      }
      catch (Exception ex)
      {

        throw;
      }

      return result;

    }

    public IEnumerable<DWCaseSessionActComplainPerson> SelectCasesSessionActComplainPersonForTransfer(int selectedRowCount, DWCourt court)
    {

      //var act_complain_result = repo.AllReadonly<CaseSessionActComplainResult>();

      Expression<Func<CaseSessionActComplainPerson, bool>> selectedCourt = x => true;
      if (court.CourtId != null)

        selectedCourt = x => x.CaseSessionActComplain.CaseSessionAct.CaseSession.Case.CourtId == court.CourtId;

      IEnumerable<DWCaseSessionActComplainPerson> result = null;
      DateTime oldDate = new DateTime(1900, 01, 01);
      result = repo.AllReadonly<CaseSessionActComplainPerson>()


                             .Where(selectedCourt)

                             .Where(x => x.DateWrt > (x.DateTransferedDW ?? oldDate))


                              .Select(x => new DWCaseSessionActComplainPerson()
                              {
                                Id = x.Id,
                                CaseId=x.CaseSessionActComplain.CaseSessionAct.CaseId,
                                CaseSessionId=x.CaseSessionActComplain.CaseSessionAct.CaseSessionId,
                                CaseSessionActComplainId = x.CaseSessionActComplainId,
                                CasePersonId = x.CasePersonId,
                                CasePersonName = x.CasePerson.FullName,
                                DateExpired = (x.CaseSessionActComplain.CaseSessionAct.DateExpired?? x.CaseSessionActComplain.CaseSessionAct.CaseSession.DateExpired),
                                DateExpiredStr = (x.CaseSessionActComplain.CaseSessionAct.DateExpired ?? x.CaseSessionActComplain.CaseSessionAct.CaseSession.DateExpired).HasValue ? (x.CaseSessionActComplain.CaseSessionAct.DateExpired ?? x.CaseSessionActComplain.CaseSessionAct.CaseSession.DateExpired).Value.ToString("dd.MM.yyyy HH:mm") : "",


                                DateTransferedDW = DateTime.Now,
                                DateWrt = x.DateWrt,
                                UserId = x.UserId,
                                UserName = x.User.LawUnit.FullName,

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




      return result;
    }
    public void SessionActComplainPersonTransfer(DWCourt court)
    {
      IEnumerable<DWCaseSessionActComplainPerson> dwcasesSessionsActComplainPerson = SelectCasesSessionActComplainPersonForTransfer(DWConstants.DWTransfer.TransferRowCounts, court);


      while (dwcasesSessionsActComplainPerson.Any())
      {


        foreach (var current in dwcasesSessionsActComplainPerson)
        {
          bool insertRow = SessionActComplainPersonInsertUpdate(current);
          if (insertRow)
          {
            var main = repo.GetById<CaseSessionActComplainPerson>(current.Id);
            main.DateTransferedDW = DateTime.Now;
            repo.Update<CaseSessionActComplainPerson>(main);
            repo.SaveChanges();
            repo.Detach(main);
          }

        }
        dwRepo.SaveChanges();
        //repo.SaveChanges();
        //  ts.Complete();
        //}

        dwcasesSessionsActComplainPerson = SelectCasesSessionActComplainPersonForTransfer(DWConstants.DWTransfer.TransferRowCounts, court);



      }



    }

    #endregion

    #region SesssionActCoordination
    public bool SessionActCoordinationInsertUpdate(DWCaseSessionActCoordination current)
    {
      bool result = false;

      try
      {
        DWCaseSessionActCoordination saved = dwRepo.All<DWCaseSessionActCoordination>().Where(x => x.Id == current.Id).FirstOrDefault();
        if (saved == null)

        {
          current.DateTransferedDW = DateTime.Now;

          dwRepo.Add<DWCaseSessionActCoordination>(current);

          result = true;
        }
        else
        {
          saved.Id = current.Id;
          saved.ActCoordinationTypeId = current.ActCoordinationTypeId;
          saved.ActCoordinationTypeName = current.ActCoordinationTypeName;
          saved.CaseLawUnitId = current.CaseLawUnitId;
          saved.CaseLawUnitName = current.CaseLawUnitName;
          saved.CaseSessionActId = current.CaseSessionActId;
          saved.Content = current.Content;

          saved.DateTransferedDW = current.DateTransferedDW;
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

          dwRepo.Update<DWCaseSessionActCoordination>(saved);
          result = true;
        }





      }
      catch (Exception ex)
      {

        throw;
      }

      return result;

    }

    public IEnumerable<DWCaseSessionActCoordination> SelectCasesSessionActCoordinationTransfer(int selectedRowCount, DWCourt court)
    {

      //var act_complain_result = repo.AllReadonly<CaseSessionActComplainResult>();

      Expression<Func<CaseSessionActCoordination, bool>> selectedCourt = x => true;
      if (court.CourtId != null)

        selectedCourt = x => x.CaseSessionAct.CaseSession.Case.CourtId == court.CourtId;

      IEnumerable<DWCaseSessionActCoordination> result = null;
      DateTime oldDate = new DateTime(1900, 01, 01);
      result = repo.AllReadonly<CaseSessionActCoordination>()


                             .Where(selectedCourt)

                             .Where(x => x.DateWrt > (x.DateTransferedDW ?? oldDate))


                              .Select(x => new DWCaseSessionActCoordination()
                              {
                                Id = x.Id,
                                ActCoordinationTypeId = x.ActCoordinationTypeId,
                                ActCoordinationTypeName = x.ActCoordinationType.Label,
                                CaseLawUnitId = x.CaseLawUnitId,
                                CaseLawUnitName = x.CaseLawUnit.LawUnit.FullName,
                                CaseSessionActId = x.CaseSessionActId,
                                DateExpired = (x.CaseSessionAct.DateExpired??x.CaseSessionAct.CaseSession.DateExpired),
                                DateExpiredStr = (x.CaseSessionAct.DateExpired ?? x.CaseSessionAct.CaseSession.DateExpired).HasValue ? (x.CaseSessionAct.DateExpired ?? x.CaseSessionAct.CaseSession.DateExpired).Value.ToString("dd.MM.yyyy HH:mm") : "",

                                Content = x.Content,
                                DateTransferedDW = DateTime.Now,
                                DateWrt = x.DateWrt,
                                UserId = x.UserId,
                                UserName = x.User.LawUnit.FullName,

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




      return result;
    }
    public void SessionActCoordinationTransfer(DWCourt court)
    {
      IEnumerable<DWCaseSessionActCoordination> dwcasesSessionsActCoordination = SelectCasesSessionActCoordinationTransfer(DWConstants.DWTransfer.TransferRowCounts, court);


      while (dwcasesSessionsActCoordination.Any())
      {


        foreach (var current in dwcasesSessionsActCoordination)
        {
          bool insertRow = SessionActCoordinationInsertUpdate(current);
          if (insertRow)
          {
            var main = repo.GetById<CaseSessionActCoordination>(current.Id);
            main.DateTransferedDW = DateTime.Now;
            repo.Update<CaseSessionActCoordination>(main);
            repo.SaveChanges();
            repo.Detach(main);
          }

        }
        dwRepo.SaveChanges();
        //repo.SaveChanges();
        //  ts.Complete();
        //}

        dwcasesSessionsActCoordination = SelectCasesSessionActCoordinationTransfer(DWConstants.DWTransfer.TransferRowCounts, court);



      }



    }

    #endregion

    #region SessionActDivorce
    public void SessionActDivorceTransfer(DWCourt court)
    {
      IEnumerable<DWCaseSessionActDivorce> dw = SelectCasesSessionActDivorceForTransfer(DWConstants.DWTransfer.TransferRowCounts, court);


      while (dw.Any())
      {


        foreach (var current in dw)
        {
          bool insertRow = SessionActDivorceInsertUpdate(current);
          if (insertRow)
          {
            var main = repo.GetById<CaseSessionActDivorce>(current.Id);
            main.DateTransferedDW = DateTime.Now;
            repo.Update<CaseSessionActDivorce>(main);
            repo.SaveChanges();
            repo.Detach(main);
          }

        }
        dwRepo.SaveChanges();
       // repo.SaveChanges();
        //  ts.Complete();
        //}

        dw = SelectCasesSessionActDivorceForTransfer(DWConstants.DWTransfer.TransferRowCounts, court);



      }



    }

    public bool SessionActDivorceInsertUpdate(DWCaseSessionActDivorce current)
    {
      bool result = false;

      try
      {
        DWCaseSessionActDivorce saved = dwRepo.All<DWCaseSessionActDivorce>().Where(x => x.Id == current.Id).FirstOrDefault();
        if (saved == null)

        {
          current.DateTransferedDW = DateTime.Now;

          dwRepo.Add<DWCaseSessionActDivorce>(current);

          result = true;
        }
        else
        {
          saved.Id = current.Id;
          saved.BirthDayMan = current.BirthDayMan;
          saved.BirthDayWoman = current.BirthDayWoman;
          saved.CaseId = current.CaseId;
          saved.CasePersonManId = current.CasePersonManId;
          saved.CasePersonManName = current.CasePersonManName;
          saved.CasePersonWomanId = current.CasePersonWomanId;
          saved.CasePersonWomanName = current.CasePersonWomanName;
          saved.CaseSessionActId = current.CaseSessionActId;
          saved.ChildrenOver18 = current.ChildrenOver18;
          saved.ChildrenUnder18 = current.ChildrenUnder18;
          saved.CountryCode = current.CountryCode;
          saved.CountryCodeDate = current.CountryCodeDate;
          saved.DivorceCountMan = current.DivorceCountMan;
          saved.DivorceCountWoman = current.DivorceCountWoman;
          saved.EducationMan = current.EducationMan;
          saved.EducationWoman = current.EducationWoman;
          saved.MarriageCountMan = current.MarriageCountMan;
          saved.MarriageCountWoman = current.MarriageCountWoman;
          saved.MarriageDate = current.MarriageDate;
          saved.MarriageFault = current.MarriageFault;
          saved.MarriageFaultDescription = current.MarriageFaultDescription;
          saved.MarriageNumber = current.MarriageNumber;
          saved.MarriagePlace = current.MarriagePlace;
          saved.MarriedStatusBeforeMan = current.MarriedStatusBeforeMan;
          saved.MarriedStatusBeforeWoman = current.MarriedStatusBeforeWoman;
          saved.NameAfterMarriageMan = current.NameAfterMarriageMan;
          saved.NameAfterMarriageWoman = current.NameAfterMarriageWoman;
          saved.NationalityMan = current.NationalityMan;
          saved.NationalityWoman = current.NationalityWoman;
          saved.OutDocumentId = current.OutDocumentId;
          saved.RegDate = current.RegDate;
          saved.RegNumber = current.RegNumber;


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
          dwRepo.Update<DWCaseSessionActDivorce>(saved);
          result = true;
        }





      }
      catch (Exception ex)
      {

        throw;
      }

      return result;

    }

    public IEnumerable<DWCaseSessionActDivorce> SelectCasesSessionActDivorceForTransfer(int selectedRowCount, DWCourt court)
    {



      Expression<Func<CaseSessionActDivorce, bool>> selectedCourt = x => true;
      if (court.CourtId != null)
        selectedCourt = x => x.CaseSessionAct.CaseSession.Case.CourtId == court.CourtId;

      IEnumerable<DWCaseSessionActDivorce> result = null;
      DateTime oldDate = new DateTime(1900, 01, 01);
      result = repo.AllReadonly<CaseSessionActDivorce>()


                             .Where(selectedCourt)
                             .Where(x => x.DateWrt > (x.DateTransferedDW ?? oldDate))


                              .Select(x => new DWCaseSessionActDivorce()
                              {
                                Id = x.Id,
                                BirthDayMan = x.BirthDayMan,
                                BirthDayWoman = x.BirthDayWoman,
                                CaseId = x.CaseId,
                                CasePersonManId = x.CasePersonManId,
                                CasePersonManName = x.CasePersonMan.FullName,
                                CasePersonWomanId = x.CasePersonWomanId,
                                CasePersonWomanName = x.CasePersonWoman.FullName,
                                CaseSessionActId = x.CaseSessionActId,
                                ChildrenOver18 = x.ChildrenOver18,
                                ChildrenUnder18 = x.ChildrenUnder18,
                                CountryCode = x.CountryCode,
                                CountryCodeDate = x.CountryCodeDate,
                                DivorceCountMan = x.DivorceCountMan,
                                DivorceCountWoman = x.DivorceCountWoman,
                                EducationMan = x.EducationMan,
                                EducationWoman = x.EducationWoman,
                                MarriageCountMan = x.MarriageCountMan,
                                MarriageCountWoman = x.MarriageCountWoman,
                                MarriageDate = x.MarriageDate,
                                MarriageFault = x.MarriageFault,
                                MarriageFaultDescription = x.MarriageFaultDescription,
                                MarriageNumber = x.MarriageNumber,
                                MarriagePlace = x.MarriagePlace,
                                MarriedStatusBeforeMan = x.MarriedStatusBeforeMan,
                                MarriedStatusBeforeWoman = x.MarriedStatusBeforeWoman,
                                NameAfterMarriageMan = x.NameAfterMarriageMan,
                                NameAfterMarriageWoman = x.NameAfterMarriageWoman,
                                NationalityMan = x.NationalityMan,
                                NationalityWoman = x.NationalityWoman,
                                OutDocumentId = x.OutDocumentId,
                                RegDate = x.RegDate,
                                RegNumber = x.RegNumber,
                                DateExpired = (x.CaseSessionAct.DateExpired ?? x.CaseSessionAct.CaseSession.DateExpired),
                                DateExpiredStr = (x.CaseSessionAct.DateExpired ?? x.CaseSessionAct.CaseSession.DateExpired).HasValue ? (x.CaseSessionAct.DateExpired ?? x.CaseSessionAct.CaseSession.DateExpired).Value.ToString("dd.MM.yyyy HH:mm") : "",




                                DateTransferedDW = DateTime.Now,
                                DateWrt = x.DateWrt,

                                UserId = x.UserId,
                                UserName = x.User.LawUnit.FullName,

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




      return result;
    }
    #endregion
  }
}
