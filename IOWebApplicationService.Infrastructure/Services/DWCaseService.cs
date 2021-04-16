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
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Models.Integrations.DW;

namespace IOWebApplicationService.Infrastructure.Services
{
  public class DWCaseService : IDWCaseService
  {
    private readonly IRepository repo;
    private readonly IDWRepository dwRepo;
    private readonly IDWErrorLogService serviceErrorLog;
    private readonly ICasePersonLinkService serviceCasePersonLink;
    public DWCaseService(IRepository _repo, IDWRepository _dwRepo, ICasePersonLinkService _serviceCasePersonLink, IDWErrorLogService _serviceErrorLog)
    {
      this.repo = _repo;
      this.dwRepo = _dwRepo;
      this.serviceCasePersonLink = _serviceCasePersonLink;
      this.serviceErrorLog = _serviceErrorLog;
    }
    public void CaseTransfer(DWCourt court)
    {
      serviceErrorLog.LogError((court.CourtId ?? 0), court.CourtName, "CaseTransfer", 0, "Стартирал");
      IEnumerable<DWCase> dwcases = SelectCasesForTransfer(DWConstants.DWTransfer.TransferRowCounts, court);

      bool insertRow = true;
      while (dwcases.Any()&&  insertRow )
      {
        //using (TransactionScope ts = new TransactionScope())


        foreach (var current_case in dwcases)
        {
          insertRow = CaseInsertUpdate(court, current_case);
          if (insertRow)
          {

            var updResult = repo.ExecuteProc<UpdateDateTransferedVM>($"{UpdateDateTransferedVM.ProcedureName}({current_case.CaseId},'{UpdateDateTransferedVM.Tables.Case}')");
            //var main_case = repo.GetById<Case>(current_case.CaseId);
            //main_case.DateTransferedDW = DateTime.Now;
            //repo.Update<Case>(main_case);
          }

        }
        dwRepo.SaveChanges();
        //repo.SaveChanges();
        //  ts.Complete();
        //}

        dwcases = SelectCasesForTransfer(DWConstants.DWTransfer.TransferRowCounts, court);



      }



    }

    public bool CaseInsertUpdate(DWCourt court, DWCase current)
    {
      bool result = false;

      try
      {
        DWCase saved = dwRepo.All<DWCase>().Where(x => x.CaseId == current.CaseId).FirstOrDefault();
        if (saved == null)

        {
          current.DateTransferedDW = DateTime.Now;
          dwRepo.Add<DWCase>(current);

          result = true;
        }
        else
        {
          saved.CaseId = current.CaseId;
          saved.CaseCharacterId = current.CaseCharacterId;
          saved.CaseCharacterName = current.CaseCharacterName;
          saved.CaseCodeId = current.CaseCodeId;
          saved.CaseCodeName = current.CaseCodeName;
          saved.CaseCodeLawbaseDescription = current.CaseCodeLawbaseDescription;
          saved.CaseCodeFullObject = current.CaseCodeFullObject;
          saved.CaseTypeCode = current.CaseTypeCode;
          saved.CaseDurationMonths = current.CaseDurationMonths;
          saved.CaseGroupId = current.CaseGroupId;
          saved.CaseGroupName = current.CaseGroupName;
          saved.CaseInforcedDate = current.CaseInforcedDate;
          saved.CaseReasonId = current.CaseReasonId;
          saved.CaseReasonName = current.CaseReasonName;
          saved.CaseStateId = current.CaseStateId;
          saved.CaseStateName = current.CaseStateName;
          saved.CaseTypeId = current.CaseTypeId;
          saved.CaseTypeName = current.CaseTypeName;
          saved.CaseTypeUnitId = current.CaseTypeUnitId;
          saved.CaseTypeUnitName = current.CaseTypeUnitName;
          saved.ComplexIndex = current.ComplexIndex;
          saved.CorrectionLoadIndex = current.CorrectionLoadIndex;
          saved.CourtGroupId = current.CourtGroupId;
          saved.CourtGroupName = current.CourtGroupName;
          saved.DateTransferedDW = current.DateTransferedDW;
          saved.DateWrt = current.DateWrt;
          saved.Description = current.Description;
          saved.DocumentId = current.DocumentId;
          saved.DocumentName = current.DocumentName;
          saved.DocumentTypeId = current.DocumentTypeId;
          saved.DocumentTypeName = current.DocumentTypeName;
          saved.EISSPNumber = current.EISSPNumber;
          saved.DateTransferedDW = DateTime.Now;
          saved.CourtId = current.CourtId;
          saved.DwCount = current.DwCount;
          saved.CourtName = current.CourtName;
          saved.CourtRegionId = current.CourtRegionId;
          saved.CourtRegionName = current.CourtRegionName;
          saved.CourtTypeId = current.CourtTypeId;
          saved.CourtTypeName = current.CourtTypeName;
          saved.ParentCourtId = current.ParentCourtId;
          saved.ParentCourtName = current.ParentCourtName;
          saved.RegDate = current.RegDate;
          saved.RegDateStr = current.RegDateStr;
          saved.RegNumber = current.RegNumber;
          saved.EcliCode = current.EcliCode;
          saved.EISPPCode = current.EISPPCode;
          saved.CityCode = current.CityCode;
          saved.CityName = current.CityName;


          dwRepo.Update<DWCase>(saved);
          result = true;
        }

        if (result)
        { CaseLawUnitTransfer(court, current.CaseId); }
      


      }
      catch (Exception ex)
      {

        serviceErrorLog.LogError((court.CourtId??0), court.CourtName, "case", current.CaseId, ex.Message);
      }

      return result;

    }


    //public bool DWCaseLowUnitInsertUpdate(List<DWCaseLawUnit> savedList, List<DWCaseLawUnit> currentList)
    //{
    //  bool result = false;
    //  try
    //  {

    //    foreach (var current in currentList)
    //    {
    //      var saved = savedList.Where(x => x.Id == current.Id)
    //                           .Where(x => x.CaseSessionId == current.CaseSessionId).FirstOrDefault();

    //      if (saved == null)
    //      {
    //        savedList.Add(current);
    //        dwRepo.Add<DWCaseLawUnit>(current);
    //      }
    //      else
    //      {

    //        saved.CaseId = current.CaseId;
    //        saved.CaseSessionId = current.CaseSessionId;
    //        saved.CourtDepartmentId = current.CourtDepartmentId;
    //        saved.CourtDepartmentId = current.CourtDepartmentId;
    //        saved.CourtDutyId = current.CourtDutyId;
    //        saved.CourtDutyName = current.CourtDutyName;
    //        saved.CourtGroupId = current.CourtGroupId;
    //        saved.CourtGroupName = current.CourtGroupName;
    //        saved.DateFrom = current.DateFrom;
    //        saved.DateTo = current.DateTo;
    //        saved.DateFromStr = current.DateFromStr;
    //        saved.DateToStr = current.DateToStr;
    //        saved.Description = current.Description;
    //        saved.Id = current.Id;
    //        saved.JudgeDepartmentRoleId = current.JudgeDepartmentRoleId;
    //        saved.JudgeDepartmentRoleName = current.JudgeDepartmentRoleName;
    //        saved.JudgeRoleId = current.JudgeRoleId;
    //        saved.JudgeRoleName = current.JudgeRoleName;
    //        saved.LawUnitFullName = current.LawUnitFullName;
    //        saved.LawUnitId = current.LawUnitId;
    //        saved.DwCount = current.DwCount;


    //      }
    //    }

    //  }
    //  catch (Exception ex)
    //  {

    //    throw;
    //  }

    //  return result;
    //}

    public IEnumerable<DWCase> SelectCasesForTransfer(int selectedRowCount, DWCourt court)
    {


      Expression<Func<Case, bool>> selectedCourt = x => true;
      if (court.CourtId != null)
        selectedCourt = x => x.CourtId == court.CourtId;

      IEnumerable<DWCase> result = null;
      DateTime oldDate = new DateTime(1900, 01, 01);
      result = repo.AllReadonly<Case>()
                             .Include(x => x.CaseType)
                             .Include(x => x.CaseCode)
                             .Include(x => x.ProcessPriority)
                             .Include(x => x.CaseState)
                             .Include(x => x.CaseCharacter)
                             .Include(x => x.CaseCode)
                             .Include(x => x.CourtGroup)
                             .Include(x => x.CaseTypeUnit)
                             .Include(x => x.Document)
                             .Include(x => x.Document.DocumentType)
                             .Include(x => x.LoadGroupLink.LoadGroup)

                             .Where(selectedCourt)
                             .Where(x => x.RegNumber != null)
                             .Where(x => x.DateWrt > (x.DateTransferedDW ?? oldDate))

                              .Select(x => new DWCase()
                              {
                                CaseId = x.Id,
                                CaseGroupId = x.CaseGroupId,
                                CaseGroupName = x.CaseGroup.Label,
                                CaseCharacterId = x.CaseCharacterId,
                                CaseCharacterName = x.CaseCharacter.Label,
                                CaseTypeId = x.CaseTypeId,
                                CaseTypeName = x.CaseType.Label,
                                CaseTypeCode = x.CaseType.Code,
                                CaseCodeId = x.CaseCodeId,
                                CaseCodeName = x.CaseCode.Label,
                                CaseCodeLawbaseDescription = x.CaseCode.LawBaseDescription,
                                CaseCodeFullObject = $"{x.CaseCode.Code} {x.CaseCode.Label} {x.CaseCode.LawBaseDescription}",
                                CourtGroupId = x.CourtGroupId,
                                CourtGroupName = x.CourtGroup.Label,
                                CaseInforcedDate = x.CaseInforcedDate,
                                CaseReasonId = x.CaseReasonId,
                                CaseReasonName = x.CaseReason.Label,
                                CaseDurationMonths = repo.AllReadonly<CaseLifecycle>().Where(a => a.CaseId == x.Id).Sum(a => a.DurationMonths),
                                CaseStateId = x.CaseStateId,
                                CaseStateName = x.CaseState.Label,
                                CaseTypeUnitId = x.CaseTypeUnitId,
                                CaseTypeUnitName = x.CaseTypeUnit.Label,

                                ComplexIndex = x.ComplexIndex,
                                CorrectionLoadIndex = x.CorrectionLoadIndex,
                                DateTransferedDW = DateTime.Now,
                                DateWrt = x.DateWrt,
                                Description = x.Description,
                                DocumentId = x.DocumentId,
                                DocumentName = $"{x.Document.DocumentNumber}/{x.Document.DocumentDate:dd.MM.yyyy}",
                                DocumentTypeId = x.Document.DocumentTypeId,
                                DocumentTypeName = x.Document.DocumentType.Label,
                                EISSPNumber = x.EISSPNumber,
                                IsOldNumber = x.IsOldNumber,
                                IsRestictedAccess = x.IsRestictedAccess,
                                LoadGroupLinkId = x.LoadGroupLinkId,
                                LoadGrouId = x.LoadGroupLink.LoadGroupId,
                                LoadGrouName = x.LoadGroupLink.LoadGroup.Label,
                                LoadIndex = x.LoadIndex,
                                ProcessPriorityId = x.ProcessPriorityId,
                                ProcessPriorityName = x.ProcessPriority.Label,
                                RegNumber = x.RegNumber,
                                RegDate = x.RegDate,
                                RegDateStr = x.RegDate.ToString("dd.MM.yyyy HH:mm"),
                                ShortNumber = x.ShortNumber,
                                ShortNumberValue = x.ShortNumberValue,
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





                              }).OrderBy(x => x.CourtId).ThenBy(x => x.CaseId).Take(selectedRowCount);



      return result;
    }
    #region CaseLawunit

    public bool CaseLawUnitInsertUpdate(DWCaseLawUnit current)
    {
      bool result = false;

      try
      {
        DWCaseLawUnit saved = dwRepo.All<DWCaseLawUnit>().Where(x => x.Id == current.Id).FirstOrDefault();
        if (saved == null)

        {

          current.DateTransferedDW = DateTime.Now;
          dwRepo.Add<DWCaseLawUnit>(current);

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



          dwRepo.Update<DWCaseLawUnit>(saved);
          result = true;
        }





      }
      catch (Exception ex)
      {

        serviceErrorLog.LogError((current.CourtId ?? 0), current.CourtName, "case_lawunit", current.Id, ex.Message);
      }

      return result;

    }

    public IEnumerable<DWCaseLawUnit> SelectCaseLawUnitTransfer(DWCourt court, long caseId)
    {




      IEnumerable<DWCaseLawUnit> result = null;





      result = repo.AllReadonly<CaseLawUnit>()

                              .Where(x => x.CaseSessionId == null)
                              .Where(x => x.DateTo == null)
                              .Where(x => x.CaseId == caseId)

                              .Select(x => new DWCaseLawUnit()
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
                                CityName = court.CityName



                              }
                                );



      return result;
    }
    public void CaseLawUnitTransfer(DWCourt court, long CaseId)
    {
      IEnumerable<DWCaseLawUnit> dw = SelectCaseLawUnitTransfer(court, CaseId);




      foreach (var current in dw)
      {
        bool insertRow = CaseLawUnitInsertUpdate(current);

      }






    }
    #endregion
    #region CasePerson

    public bool CasePersonInsertUpdate(DWCourt court, DWCasePerson current)
    {
      bool result = false;
      current.LinkRelationsString = LinkRelationsString_Select(current.Id);
      try
      {
        DWCasePerson saved = dwRepo.All<DWCasePerson>().Where(x => x.Id == current.Id).FirstOrDefault();
        if (saved == null)

        {

          current.DateTransferedDW = DateTime.Now;
          dwRepo.Add<DWCasePerson>(current);

          result = true;
        }
        else
        {
          saved.Id = current.Id;
          saved.DepartmentName = current.DepartmentName;
          saved.Family2Name = current.Family2Name;
          saved.FamilyName = current.FamilyName;
          saved.FirstName = current.FirstName;
          saved.FullName = current.FullName;
          saved.LatinName = current.LatinName;
          saved.MiddleName = current.MiddleName;
          saved.PersonMaturityId = current.PersonMaturityId;
          saved.PersonMaturityName = current.PersonMaturityName;
          saved.PersonRoleId = current.PersonRoleId;
          saved.PersonRoleName = current.PersonRoleName;
          saved.Person_SourceId = current.Person_SourceId;

          saved.Person_SourceType = current.Person_SourceType;

          saved.Uic = current.Uic;
          saved.UicTypeId = current.UicTypeId;
          saved.UicTypeName = current.UicTypeName;

          saved.CaseId = current.CaseId;
          saved.CaseSessionId = current.CaseSessionId;
          saved.PersonRoleId = current.PersonRoleId;
          saved.PersonRoleName = current.PersonRoleName;
          saved.MilitaryRangId = current.MilitaryRangId;
          saved.MilitaryRangName = current.MilitaryRangName;
          saved.IsInitialPerson = current.IsInitialPerson;
          saved.CasePersonIdentificator = current.CasePersonIdentificator;
          saved.DateFrom = current.DateFrom;
          saved.DateTo = current.DateTo;
          saved.DateFromStr = current.DateFromStr;
          saved.DateToStr = current.DateToStr;
          saved.RowNumber = current.RowNumber;
          saved.ForNotification = current.ForNotification;
          saved.NotificationNumber = current.NotificationNumber;
          saved.UserId = current.UserId;
          saved.UserName = current.UserName;
          saved.DateWrt = current.DateWrt;
          saved.DateTransferedDW = current.DateTransferedDW;
          saved.IsArrested = current.IsArrested;
          saved.BirthCountryCode = current.BirthCountryCode;
          saved.BirthCountryName = current.BirthCountryName;
          saved.BirthCityCode = current.BirthCityCode;
          saved.BirthCityName = current.BirthCityName;
          saved.BirthForeignPlace = current.BirthForeignPlace;

          saved.DateExpired = current.DateExpired;
          saved.DateExpiredStr = current.DateExpiredStr;
          saved.DescriptionExpired = current.DescriptionExpired;
          saved.UserExpiredId = current.UserExpiredId;
          saved.UserExpiredName = current.UserExpiredName;
          saved.LinkRelationsString = current.LinkRelationsString;
          saved.DateTransferedDW = DateTime.Now;
          saved.DwCount = current.DwCount;

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
          saved.LinkRelationsString = current.LinkRelationsString;



          dwRepo.Update<DWCasePerson>(saved);
          result = true;
        }





      }
      catch (Exception ex)
      {

        serviceErrorLog.LogError((court.CourtId ?? 0), court.CourtName, "case_person", current.Id, ex.Message);

      }

      return result;

    }

    public IEnumerable<DWCasePerson> SelectCasePersonTransfer(int selectedRowCount, DWCourt court)
    {




      IEnumerable<DWCasePerson> result = null;

      DateTime oldDate = new DateTime(1900, 01, 01);


      Expression<Func<CasePerson, bool>> selectedCourt = x => true;
      if (court.CourtId != null)
        selectedCourt = x => x.CourtId == court.CourtId;

      result = repo.AllReadonly<CasePerson>()

                              .Where(x => x.CaseSessionId == null)

                             .Where(x => x.DateWrt > (x.DateTransferedDW ?? oldDate))
                             .Where(selectedCourt)

                              .Select(x => new DWCasePerson()
                              {
                                Id = x.Id,
                                Family2Name = x.Family2Name,
                                FamilyName = x.FamilyName,
                                FirstName = x.FirstName,
                                FullName = x.FullName,
                                LatinName = x.LatinName,
                                MiddleName = x.MiddleName,
                                PersonMaturityId = x.PersonMaturityId,
                                PersonMaturityName = x.PersonMaturity.Label,
                                Person_SourceId = x.Person_SourceId,

                                Person_SourceType = x.Person_SourceType,

                                Uic = x.Uic,
                                UicTypeId = x.UicTypeId,
                                UicTypeName = x.UicType.Label,
                                CaseId = x.CaseId,
                                CaseSessionId = x.CaseSessionId,
                                PersonRoleId = x.PersonRoleId,
                                PersonRoleName = x.PersonRole.Label,
                                MilitaryRangId = x.MilitaryRangId,
                                MilitaryRangName = x.MilitaryRang.Label,
                                IsInitialPerson = x.IsInitialPerson,
                                CasePersonIdentificator = x.CasePersonIdentificator,
                                DateFrom = x.DateFrom,
                                DateTo = x.DateTo,
                                DateFromStr = x.DateFrom.ToString("dd.MM.yyyy"),
                                DateToStr = x.DateTo.HasValue ? x.DateTo.Value.ToString("dd.MM.yyyy") : "",
                                      //////////////////
                                      RowNumber = x.RowNumber,
                                ForNotification = x.ForNotification,
                                NotificationNumber = x.NotificationNumber,
                                UserId = x.UserId,
                                UserName = x.User.LawUnit.FullName,
                                DateWrt = x.DateWrt,
                                DateTransferedDW = DateTime.Now,
                                IsArrested = x.IsArrested,
                                BirthCountryCode = x.BirthCountryCode,

                                BirthCityCode = x.BirthCityCode,

                                BirthForeignPlace = x.BirthForeignPlace,

                                DateExpired = x.DateExpired,
                                DateExpiredStr = x.DateExpired.HasValue ? x.DateExpired.Value.ToString("dd.MM.yyyy") : "",
                                DescriptionExpired = x.DescriptionExpired,
                                UserExpiredId = x.UserExpiredId,
                                UserExpiredName = x.User.LawUnit.FullName,
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



                              }
                                )
                                           .OrderBy(x => x.CourtId).ThenBy(x => x.CaseId).Take(selectedRowCount);


      //foreach (var item in result)
      //{
      //  item.LinkRelationsString = LinkRelationsString_Select(item.CaseId, item.Id);
      //}

      return result;
    }
    public void CasePersonTransfer(DWCourt court)
    {
      serviceErrorLog.LogError((court.CourtId ?? 0), court.CourtName, "CasePersonTransfer", 0, "Стартирал");
      IEnumerable<DWCasePerson> dw = SelectCasePersonTransfer(DWConstants.DWTransfer.TransferRowCounts, court);

      bool insertRow = true;
      while (dw.Any() && insertRow)
      {



        foreach (var current in dw)
        {
          insertRow = CasePersonInsertUpdate(court, current);
          if (insertRow)
          {
                        var updResult = repo.ExecuteProc<UpdateDateTransferedVM>($"{UpdateDateTransferedVM.ProcedureName}({current.Id},'{UpdateDateTransferedVM.Tables.CasePerson}')");

            //var main = repo.GetById<CasePerson>(current.Id);
            //main.DateTransferedDW = DateTime.Now;
            //repo.Update<CasePerson>(main);
          }

        }
        dwRepo.SaveChanges();
       // repo.SaveChanges();


        dw = SelectCasePersonTransfer(DWConstants.DWTransfer.TransferRowCounts, court);







      }
    }

    public string LinkRelationsString_Select(int personId)
    {
      string linkRelationsString = "";

      var personLinks = serviceCasePersonLink.GetLinkForPerson(personId, false, 0, null).ToList();

      foreach (var pl in personLinks)
      {



        linkRelationsString = linkRelationsString + pl.Label + "; ";
      }

      return linkRelationsString;

    }

    #endregion
    #region DWCaseLifecycle
    public void CaseLifecycleTransfer(DWCourt court)
    {
      try
      {

        serviceErrorLog.LogError((court.CourtId ?? 0), court.CourtName, "CaseLifecycleTransfer", 0, "Стартирал");
        IEnumerable<DWCaseLifecycle> dw = SelectCaseLifecycleTransfer(DWConstants.DWTransfer.TransferRowCounts, court);

        bool insertRow = true;
        while (dw.Any() && insertRow)
        {


          foreach (var current in dw)
          {
            insertRow = CaseLifecycleInsertUpdate(current);
            if (insertRow)
            {
              var updResult = repo.ExecuteProc<UpdateDateTransferedVM>($"{UpdateDateTransferedVM.ProcedureName}({current.Id},'{UpdateDateTransferedVM.Tables.CaseLifecycle}')");

              //var main = repo.GetById<CaseLifecycle>(current.Id);
              //main.DateTransferedDW = DateTime.Now;
              //repo.Update<CaseLifecycle>(main);
            }

          }
          dwRepo.SaveChanges();
          //repo.SaveChanges();
          //  ts.Complete();
          //}

          dw = SelectCaseLifecycleTransfer(DWConstants.DWTransfer.TransferRowCounts, court);



        }

      }
      catch (Exception ex)
      {
        //serviceErrorLog.LogError((current.CourtId ?? 0), current.CourtName, "case_lifecycle", current.CaseId, ex.Message);
      }

    }

    public bool CaseLifecycleInsertUpdate(DWCaseLifecycle current)
    {
      bool result = false;

      try
      {
        DWCaseLifecycle saved = dwRepo.All<DWCaseLifecycle>().Where(x => x.Id == current.Id).FirstOrDefault();
        if (saved == null)

        {
          current.DateTransferedDW = DateTime.Now;

          dwRepo.Add<DWCaseLifecycle>(current);

          result = true;
        }
        else
        {
          saved.Id = current.Id;
          saved.CaseId = current.CaseId;
          saved.LifecycleTypeId = current.LifecycleTypeId;
          saved.LifecycleTypeIdName = current.LifecycleTypeIdName;
          saved.Iteration = saved.Iteration;
          saved.DateFrom = current.DateFrom;
          saved.DateFromStr = current.DateFromStr;
          saved.DateTo = current.DateTo;
          saved.DateToStr = current.DateToStr;
          saved.DurationMonths = current.DurationMonths;
          saved.Description = current.Description;
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

          dwRepo.Update<DWCaseLifecycle>(saved);
          result = true;
        }





      }
      catch (Exception ex)
      {
        serviceErrorLog.LogError((current.CourtId ?? 0), current.CourtName, "case_lifecycle", current.CaseId, ex.Message);
      }

      return result;

    }

    public IEnumerable<DWCaseLifecycle> SelectCaseLifecycleTransfer(int selectedRowCount, DWCourt court)
    {



      Expression<Func<CaseLifecycle, bool>> selectedCourt = x => true;
      if (court.CourtId != null)
        selectedCourt = x => x.Case.CourtId == court.CourtId;

      IEnumerable<DWCaseLifecycle> result = null;
      DateTime oldDate = new DateTime(1900, 01, 01);
      result = repo.AllReadonly<CaseLifecycle>()


                             .Where(selectedCourt)
                             .Where(x => x.DateWrt > (x.DateTransferedDW ?? oldDate))


                              .Select(x => new DWCaseLifecycle()
                              {
                                Id = x.Id,

                                CaseId = x.CaseId,

                                LifecycleTypeId = x.LifecycleTypeId,
                                LifecycleTypeIdName = x.LifecycleType.Label,
                                Iteration = x.Iteration,
                                DateFrom = x.DateFrom,
                                DateFromStr = x.DateFrom.ToString("dd.MM.yyyy"),
                                DateTo = x.DateTo,
                                DateToStr = x.DateTo.HasValue ? x.DateTo.Value.ToString("dd.MM.yyyy") : "",
                                DurationMonths = x.DurationMonths,
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

                              }).OrderBy(x => x.Id).Take(selectedRowCount);




      return result;
    }
    #endregion

  }
}
