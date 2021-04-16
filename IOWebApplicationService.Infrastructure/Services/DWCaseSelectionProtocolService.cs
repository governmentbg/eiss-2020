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
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.Integrations.DW;

namespace IOWebApplicationService.Infrastructure.Services
{
  public class DWCaseSelectionProtocolService : IDWCaseSelectionProtocolService
  {
    private readonly IRepository repo;
    private readonly IDWRepository dwRepo;
    private readonly IDWErrorLogService serviceErrorLog;
    public DWCaseSelectionProtocolService(IRepository _repo, IDWRepository _dwRepo, IDWErrorLogService _serviceErrorLog)
    {
      this.repo = _repo;
      this.dwRepo = _dwRepo;
      this.serviceErrorLog = _serviceErrorLog;
    }
    #region CaseSelectionProtocol
    public bool CaseSelectionProtocolInsertUpdate(DWCaseSelectionProtocol current, DWCourt court)
    {
      bool result = false;

      try
      {
        DWCaseSelectionProtocol saved = dwRepo.All<DWCaseSelectionProtocol>().Where(x => x.Id == current.Id).FirstOrDefault();
        if (saved == null)

        {
          current.DateTransferedDW = DateTime.Now;

          dwRepo.Add<DWCaseSelectionProtocol>(current);

          result = true;
        }
        else
        {
          saved.Id = current.Id;

          saved.CaseId = current.CaseId;
          saved.CompartmentID = current.CompartmentID;
          saved.CompartmentName = current.CompartmentName;
          saved.CaseLawUnitDismisalId = current.CaseLawUnitDismisalId;
          saved.CourtDutyId = current.CourtDutyId;
          saved.CourtDutyName = current.CourtDutyName;
          saved.CourtGroupName = current.CourtGroupName;
          saved.SelectionDate = current.SelectionDate;
          saved.JudgeRoleId = current.JudgeRoleId;
          saved.JudgeRoleName = current.JudgeRoleName;
          saved.SelectionModeId = current.SelectionModeId;
          saved.SelectionModeName = current.SelectionModeName;
          saved.SpecialityId = current.SpecialityId;
          saved.SpecialityName = current.SpecialityName;
          saved.IncludeCompartmentJudges = current.IncludeCompartmentJudges;
          saved.Description = current.Description;
          saved.SelectedLawUnitId = current.SelectedLawUnitId;
          saved.SelectedLawUnitName = current.SelectedLawUnitName;
          saved.SelectionProtokolStateId = current.SelectionProtokolStateId;
          saved.SelectionProtokolStateName = current.SelectionProtokolStateName;

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


          dwRepo.Update<DWCaseSelectionProtocol>(saved);
          result = true;
        }





      }
      catch (Exception ex)
      {

        serviceErrorLog.LogError((current.CourtId ?? 0), current.CourtName, "case+selection_protocol", current.Id, ex.Message);


      }



      return result;

    }

    public IEnumerable<DWCaseSelectionProtocol> SelectCaseSelectionProtokol(int selectedRowCount, DWCourt court)
    {



      Expression<Func<CaseSelectionProtokol, bool>> selectedCourt = x => true;
      if (court.CourtId != null)

        selectedCourt = x => x.CourtId == court.CourtId;

      IEnumerable<DWCaseSelectionProtocol> result = null;
      DateTime oldDate = new DateTime(1900, 01, 01);
      result = repo.AllReadonly<CaseSelectionProtokol>()


                             .Where(selectedCourt)

                             .Where(x => x.DateWrt > (x.DateTransferedDW ?? oldDate))


                              .Select(x => new DWCaseSelectionProtocol()
                              {
                                Id = x.Id,
                                CaseId = x.CaseId,
                                CompartmentID = x.CompartmentID,
                                CompartmentName = x.CompartmentName,
                                CaseLawUnitDismisalId = x.CaseLawUnitDismisalId,
                                CourtDutyId = x.CourtDutyId,
                                CourtDutyName = x.CourtDuty.Description,
                                CourtGroupId = x.Case.CourtGroupId,
                                CourtGroupName = x.Case.CourtGroup.Label,
                                SelectionDate = x.SelectionDate,
                                JudgeRoleId = x.JudgeRoleId,
                                JudgeRoleName = x.JudgeRole.Label,
                                SelectionModeId = x.SelectionModeId,
                                SelectionModeName = x.SelectionMode.Label,
                                SpecialityId = x.SpecialityId,
                                SpecialityName = x.Speciality.Label,
                                IncludeCompartmentJudges = x.IncludeCompartmentJudges,
                                Description = x.Description,
                                SelectedLawUnitId = x.SelectedLawUnitId,
                                SelectedLawUnitName = x.SelectedLawUnit.FullName,
                                SelectionProtokolStateId = x.SelectionProtokolStateId,
                                SelectionProtokolStateName = x.SelectionProtokolState.Label,



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
    public void CaseSelectionProtokolTransfer(DWCourt court)
    {
      serviceErrorLog.LogError((court.CourtId ?? 0), court.CourtName, "CaseSelectionProtokolTransfer", 0, "Стартирал");
      IEnumerable<DWCaseSelectionProtocol> dwSelectCaseSelectionProtokol = SelectCaseSelectionProtokol(DWConstants.DWTransfer.TransferRowCounts, court);

      bool insertRow = true;
      while (dwSelectCaseSelectionProtokol.Any() && insertRow)
      {


        foreach (var current in dwSelectCaseSelectionProtokol)
        {
           insertRow = CaseSelectionProtocolInsertUpdate(current, court);
          if (insertRow)
          {
            var updResult = repo.ExecuteProc<UpdateDateTransferedVM>($"{UpdateDateTransferedVM.ProcedureName}({current.Id},'{UpdateDateTransferedVM.Tables.CaseSelectionProtocol}')");

            //var main = repo.GetById<CaseSelectionProtokol>(current.Id);
            //main.DateTransferedDW = DateTime.Now;
            //repo.Update<CaseSelectionProtokol>(main);
          }

        }
        dwRepo.SaveChanges();
       // repo.SaveChanges();
        //  ts.Complete();
        //}

        dwSelectCaseSelectionProtokol = SelectCaseSelectionProtokol(DWConstants.DWTransfer.TransferRowCounts, court);



      }



    }

    #endregion
    #region CaseSelectionProtocolCompartment
    public bool CaseSelectionProtocolCompartmentInsertUpdate(DWCaseSelectionProtocolCompartment current, DWCourt court)
    {
      bool result = false;

      try
      {
        DWCaseSelectionProtocolCompartment saved = dwRepo.All<DWCaseSelectionProtocolCompartment>().Where(x => x.Id == current.Id).FirstOrDefault();
        if (saved == null)

        {
          current.DateTransferedDW = DateTime.Now;

          dwRepo.Add<DWCaseSelectionProtocolCompartment>(current);

          result = true;
        }
        else
        {
          saved.Id = current.Id;
          saved.LawUnitId = current.LawUnitId;
          saved.LawUnitName = current.LawUnitName;
          saved.CaseId = current.CaseId;
          saved.CaseSelectionProtokolId = current.CaseSelectionProtokolId;



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


          dwRepo.Update<DWCaseSelectionProtocolCompartment>(saved);
          result = true;
        }





      }
      catch (Exception ex)
      {

        serviceErrorLog.LogError((current.CourtId ?? 0), current.CourtName, "case_Selection_Protocol_Compartment", current.Id, ex.Message);


      }



      return result;

    }

    public IEnumerable<DWCaseSelectionProtocolCompartment> SelectCaseSelectionProtokolCompartment(int selectedRowCount, DWCourt court)
    {



      Expression<Func<CaseSelectionProtokolCompartment, bool>> selectedCourt = x => true;
      if (court.CourtId != null)

        selectedCourt = x => x.CourtId == court.CourtId;

      IEnumerable<DWCaseSelectionProtocolCompartment> result = null;
      DateTime oldDate = new DateTime(1900, 01, 01);
      result = repo.AllReadonly<CaseSelectionProtokolCompartment>()


                             .Where(selectedCourt)

                             .Where(x => x.DateWrt > (x.DateTransferedDW ?? oldDate))


                              .Select(x => new DWCaseSelectionProtocolCompartment()
                              {
                                Id = x.Id,
                                CaseId = x.CaseId,
                                LawUnitId = x.LawUnitId,
                                LawUnitName = x.LawUnit.FullName,
                                CaseSelectionProtokolId = x.CaseSelectionProtokolId,




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
    public void CaseSelectionProtocolCompartmentTransfer(DWCourt court)
    {
      serviceErrorLog.LogError((court.CourtId ?? 0), court.CourtName, "CaseSelectionProtocolCompartmentTransfer", 0, "Стартирал");
      IEnumerable<DWCaseSelectionProtocolCompartment> dw = SelectCaseSelectionProtokolCompartment(DWConstants.DWTransfer.TransferRowCounts, court);
      bool insertRow = true;

      while (dw.Any() && insertRow)
      {


        foreach (var current in dw)
        {
          insertRow = CaseSelectionProtocolCompartmentInsertUpdate(current, court);
          if (insertRow)
          {
            var updResult = repo.ExecuteProc<UpdateDateTransferedVM>($"{UpdateDateTransferedVM.ProcedureName}({current.Id},'{UpdateDateTransferedVM.Tables.CaseSelectionProtocolCompartment}')");

            //var main = repo.GetById<CaseSelectionProtokolCompartment>(current.Id);
            //main.DateTransferedDW = DateTime.Now;
            //repo.Update<CaseSelectionProtokolCompartment>(main);
          }

        }
        dwRepo.SaveChanges();
        //repo.SaveChanges();
        //  ts.Complete();
        //}

        dw= SelectCaseSelectionProtokolCompartment(DWConstants.DWTransfer.TransferRowCounts, court);



      }



    }

    #endregion

    #region CaseSelectionProtocolCompartment
    public bool CaseSelectionProtocolLawUnitInsertUpdate(DWCaseSelectionProtocolLawunit current, DWCourt court)
    {
      bool result = false;

      try
      {
        DWCaseSelectionProtocolLawunit saved = dwRepo.All<DWCaseSelectionProtocolLawunit>().Where(x => x.Id == current.Id).FirstOrDefault();
        if (saved == null)

        {
          current.DateTransferedDW = DateTime.Now;

          dwRepo.Add<DWCaseSelectionProtocolLawunit>(current);

          result = true;
        }
        else
        {
          saved.Id = current.Id;
          saved.LawUnitId = current.LawUnitId;
          saved.LawUnitName = current.LawUnitName;
          saved.CaseId = current.CaseId;
          saved.CaseSelectionProtokolId = current.CaseSelectionProtokolId;
          saved.SelectedFromCaseGroup = current.SelectedFromCaseGroup;
          saved.CaseCount = current.CaseCount;
          saved.LoadIndex = current.LoadIndex;
          saved.CaseGroupId = current.CaseGroupId;
          saved.CaseGroupName = current.CaseGroupName;
          saved.StateId = current.StateId;
          saved.StateName= current.StateName;



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


          dwRepo.Update<DWCaseSelectionProtocolLawunit>(saved);
          result = true;
        }





      }
      catch (Exception ex)
      {
        serviceErrorLog.LogError((current.CourtId ?? 0), current.CourtName, "case_selection_protocol_lawunit", current.Id, ex.Message);

      }



      return result;

    }

    public IEnumerable<DWCaseSelectionProtocolLawunit> SelectCaseSelectionProtokolLawUnit(int selectedRowCount, DWCourt court)
    {



      Expression<Func<CaseSelectionProtokolLawUnit, bool>> selectedCourt = x => true;
      if (court.CourtId != null)

        selectedCourt = x => x.CourtId == court.CourtId;

      IEnumerable<DWCaseSelectionProtocolLawunit> result = null;
      DateTime oldDate = new DateTime(1900, 01, 01);
      result = repo.AllReadonly<CaseSelectionProtokolLawUnit>()


                             .Where(selectedCourt)

                             .Where(x => x.DateWrt > (x.DateTransferedDW ?? oldDate))


                              .Select(x => new DWCaseSelectionProtocolLawunit()
                              {
                                Id = x.Id,
                                CaseId = x.CaseId,
                                LawUnitId = x.LawUnitId,
                                LawUnitName = x.LawUnit.FullName,
                                LoadIndex = x.LoadIndex,
                                CaseCount=x.CaseCount,
                                SelectedFromCaseGroup=x.SelectedFromCaseGroup,
                                CaseGroupId=x.CaseGroupId,
                                CaseGroupName=x.CaseGroup.Label,
                                StateId=x.StateId,
                                StateName=x.State.Label,
                               CaseSelectionProtokolId = x.CaseSelectionProtokolId,
                                Description = x.Description,
                                




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
    public void CaseSelectionProtocolLawunitTransfer(DWCourt court)
    {
      serviceErrorLog.LogError((court.CourtId ?? 0), court.CourtName, "CaseSelectionProtocolLawunitTransfer", 0, "Стартирал");
      IEnumerable<DWCaseSelectionProtocolLawunit> dw = SelectCaseSelectionProtokolLawUnit(DWConstants.DWTransfer.TransferRowCounts, court);
      bool insertRow = true;

      while (dw.Any() && insertRow)
      {


        foreach (var current in dw)
        {
           insertRow = CaseSelectionProtocolLawUnitInsertUpdate(current, court);
          if (insertRow)
          {
            var updResult = repo.ExecuteProc<UpdateDateTransferedVM>($"{UpdateDateTransferedVM.ProcedureName}({current.Id},'{UpdateDateTransferedVM.Tables.CaseSelectionProtocolLawunit}')");

            //var main = repo.GetById<CaseSelectionProtokolLawUnit>(current.Id);
            //main.DateTransferedDW = DateTime.Now;
            //repo.Update<CaseSelectionProtokolLawUnit>(main);
          }

        }
        dwRepo.SaveChanges();
        //repo.SaveChanges();
        //  ts.Complete();
        //}

        dw = SelectCaseSelectionProtokolLawUnit(DWConstants.DWTransfer.TransferRowCounts, court);



      }



    }

    #endregion
  }
}
