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
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Models.Integrations.DW;

namespace IOWebApplicationService.Infrastructure.Services
{
  public class DWDocumentService : IDWDocumentService
  {
    private readonly IRepository repo;
    private readonly IDWRepository dwRepo;
    private readonly IDWErrorLogService serviceErrorLog;
    public DWDocumentService(IRepository _repo, IDWRepository _dwRepo, IDWErrorLogService _serviceErrorLog)
    {
      this.repo = _repo;
      this.dwRepo = _dwRepo;
      this.serviceErrorLog = _serviceErrorLog;
    }
    #region Document
    public bool DocumentInsertUpdate(DWDocument current, DWCourt court)
    {
      bool result = false;

      try
      {
        DWDocument saved = dwRepo.All<DWDocument>().Where(x => x.Id == current.Id).FirstOrDefault();
        if (saved == null)

        {
          current.DateTransferedDW = DateTime.Now;

          dwRepo.Add<DWDocument>(current);

          result = true;
        }
        else
        {
          saved.Id = current.Id;
          saved.ActualDocumentDate = current.ActualDocumentDate;
          saved.DeliveryGroupId = current.DeliveryGroupId;
          saved.DeliveryGroupName = current.DeliveryGroupName;
          saved.DeliveryTypeId = current.DeliveryTypeId;
          saved.DeliveryTypeName = current.DeliveryTypeName;
          saved.Description = current.Description;
          saved.DocumentDate = current.DocumentDate;
          saved.DocumentDirectionId = current.DocumentDirectionId;
          saved.DocumentDirectionName = current.DocumentDirectionName;
          saved.DocumentGroupId = current.DocumentGroupId;
          saved.DocumentGroupName = current.DocumentGroupName;
          saved.DocumentNumber = current.DocumentNumber;
          saved.DocumentNumberValue = current.DocumentNumberValue;
          saved.DocumentTypeId = current.DocumentTypeId;
          saved.DocumentTypeName = current.DocumentTypeName;
          saved.IsOldNumber = current.IsOldNumber;
          saved.IsRestictedAccess = current.IsRestictedAccess;
          saved.IsSecret = current.IsSecret;


          saved.DateTransferedDW = DateTime.Now;
          saved.DateWrt = current.DateWrt;
          saved.UserId = current.UserId;
          saved.UserName = current.UserName;


          saved.CourtId = current.CourtId;
          saved.DwCount = current.DwCount;
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


          dwRepo.Update<DWDocument>(saved);
          result = true;
        }



        if (result)
        {
          DocumentCaseInfoTransfer(court, current.Id);
          DocumentPersonTransfer(court, current.Id);
          DocumentLinkTransfer(court, current.Id);
          DocumentInstitutionCaseInfoTransfer(court, current.Id);
        }

      }
      catch (Exception ex)
      {

        serviceErrorLog.LogError((court.CourtId ?? 0), court.CourtName, "document", current.Id, ex.Message);
      }



      return result;

    }

    public IEnumerable<DWDocument> SelectDocumentTransfer(int selectedRowCount, DWCourt court)
    {

      //var act_complain_result = repo.AllReadonly<CaseSessionActComplainResult>();

      Expression<Func<Document, bool>> selectedCourt = x => true;
      if (court.CourtId != null)

        selectedCourt = x => x.CourtId == court.CourtId;

      IEnumerable<DWDocument> result = null;
      DateTime oldDate = new DateTime(1900, 01, 01);
      result = repo.AllReadonly<Document>()


                             .Where(selectedCourt)

                             .Where(x => x.DateWrt > (x.DateTransferedDW ?? oldDate))


                              .Select(x => new DWDocument()
                              {
                                Id = x.Id,
                                ActualDocumentDate = x.ActualDocumentDate,
                                DeliveryGroupId = x.DeliveryGroupId,
                                DeliveryGroupName = x.DeliveryGroup.Label,
                                DeliveryTypeId = x.DeliveryTypeId,
                                DeliveryTypeName = x.DeliveryType.Label,
                                Description = x.Description,
                                DocumentDate = x.DocumentDate,
                                DocumentDirectionId = x.DocumentDirectionId,
                                DocumentDirectionName = x.DocumentDirection.Label,
                                DocumentGroupId = x.DocumentGroupId,
                                DocumentGroupName = x.DocumentGroup.Label,
                                DocumentNumber = x.DocumentNumber,
                                DocumentNumberValue = x.DocumentNumberValue,
                                DocumentTypeId = x.DocumentTypeId,
                                DocumentTypeName = x.DocumentType.Label,
                                IsOldNumber = x.IsOldNumber,
                                IsRestictedAccess = x.IsRestictedAccess,
                                IsSecret = x.IsSecret,
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
    public void DocumentTransfer(DWCourt court)
    {
      serviceErrorLog.LogError((court.CourtId ?? 0), court.CourtName, "DocumentTransfer", 0, "Стартирал");
      IEnumerable<DWDocument> dwDocuments = SelectDocumentTransfer(DWConstants.DWTransfer.TransferRowCounts, court);

      bool insertRow = true;
      while (dwDocuments.Any() && insertRow)
      {
      

        foreach (var current in dwDocuments)
        {
          insertRow = DocumentInsertUpdate(current, court);
          if (insertRow)
          {
            var updResult = repo.ExecuteProc<UpdateDateTransferedVM>($"{UpdateDateTransferedVM.ProcedureName}({current.Id},'{UpdateDateTransferedVM.Tables.Document}')");

            //var main = repo.GetById<Document>(current.Id);
            //main.DateTransferedDW = DateTime.Now;
            //repo.Update<Document>(main);
          }

        }
        dwRepo.SaveChanges();
       // repo.SaveChanges();
        //  ts.Complete();
        //}

        dwDocuments = SelectDocumentTransfer(DWConstants.DWTransfer.TransferRowCounts, court);



      }



    }

    #endregion
    #region DocumentCaseInfo
    public bool DocumentCaseInfoInsertUpdate(DWDocumentCaseInfo current)
    {
      bool result = false;

      try
      {
        DWDocumentCaseInfo saved = dwRepo.All<DWDocumentCaseInfo>().Where(x => x.Id == current.Id).FirstOrDefault();
        if (saved == null)

        {
          current.DateTransferedDW = DateTime.Now;

          dwRepo.Add<DWDocumentCaseInfo>(current);

          result = true;
        }
        else
        {
          saved.Id = current.Id;
          saved.ActDate = current.ActDate;
          saved.ActNumber = current.ActNumber;
          saved.ActTypeId = current.ActTypeId;
          saved.ActTypeName = current.ActTypeName;
          saved.CaseGroupId = current.CaseGroupId;
          saved.CaseGroupName = current.CaseGroupName;
          saved.CaseId = current.CaseId;
          saved.CaseRegNumber = current.CaseRegNumber;
          saved.CaseShortNumber = current.CaseShortNumber;
          saved.CaseYear = current.CaseYear;
          saved.Description = current.Description;
          saved.SessionActId = current.SessionActId;


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


          dwRepo.Update<DWDocumentCaseInfo>(saved);
          result = true;
        }





      }
      catch (Exception ex)
      {

        serviceErrorLog.LogError((current.CourtId ?? 0), current.CourtName, "document_case_info", current.Id, ex.Message);
      }

      return result;

    }

    public IEnumerable<DWDocumentCaseInfo> SelectDocumentCaseInfoTransfer(long documentId, DWCourt court)
    {


      var session_act = repo.AllReadonly<CaseSessionAct>();

      IEnumerable<DWDocumentCaseInfo> result = null;

      result = repo.AllReadonly<DocumentCaseInfo>()


                             .Where(x => x.DocumentId == documentId)


                              .Select(x => new DWDocumentCaseInfo()
                              {
                                Id = x.Id,
                                DocumentId = x.DocumentId,
                                ActDate = session_act.Where(a => a.Id == x.SessionActId).Select(a => a.ActDate).FirstOrDefault(),
                                ActNumber = session_act.Where(a => a.Id == x.SessionActId).Select(a => a.RegNumber).FirstOrDefault(),
                                ActTypeId = session_act.Where(a => a.Id == x.SessionActId).Select(a => a.ActTypeId).FirstOrDefault(),
                                ActTypeName = session_act.Where(a => a.Id == x.SessionActId).Select(a => a.ActType.Label).FirstOrDefault(),
                                CaseGroupId = x.Case.CaseGroupId,
                                CaseGroupName = x.Case.CaseGroup.Label,
                                CaseId = x.CaseId,
                                CaseRegNumber = x.Case.RegNumber,
                                CaseShortNumber = x.Case.ShortNumber,
                                CaseYear = x.Case.RegDate.Year,

                                SessionActId = x.SessionActId,


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
    public void DocumentCaseInfoTransfer(DWCourt court, long documenId)
    {
      IEnumerable<DWDocumentCaseInfo> dwDocumentsCseInfo = SelectDocumentCaseInfoTransfer(documenId, court);




      foreach (var current in dwDocumentsCseInfo)
      {
        bool insertRow = DocumentCaseInfoInsertUpdate(current);

      }
      //dwRepo.SaveChanges();





    }

    #endregion

    #region DocumentPerson
    public bool DocumentPersonInsertUpdate(DWDocumentPerson current)
    {
      bool result = false;

      try
      {
        DWDocumentPerson saved = dwRepo.All<DWDocumentPerson>().Where(x => x.Id == current.Id).FirstOrDefault();
        if (saved == null)

        {
          current.DateTransferedDW = DateTime.Now;

          dwRepo.Add<DWDocumentPerson>(current);

          result = true;
        }
        else
        {
          saved.Id = current.Id;
          saved.DepartmentName = current.DepartmentName;
          saved.DocumentId = current.DocumentId;
          saved.Family2Name = current.Family2Name;
          saved.FamilyName = current.FamilyName;
          saved.FirstName = current.FirstName;
          saved.FullName = current.FullName;
          saved.LatinName = current.LatinName;
          saved.MiddleName = current.MiddleName;
          saved.MilitaryRangId = current.MilitaryRangId;
          saved.MilitaryRangName = current.MilitaryRangName;
          saved.PersonMaturityId = current.PersonMaturityId;
          saved.PersonMaturityName = current.PersonMaturityName;
          saved.PersonRoleId = current.PersonRoleId;
          saved.PersonRoleName = current.PersonRoleName;
          saved.Person_SourceId = current.Person_SourceId;
          //saved.Person_SourceName = current.Person_SourceName;
          saved.Person_SourceType = current.Person_SourceType;
         // saved.Person_SourceTypeName = current.Person_SourceTypeName;
          saved.Uic = current.Uic;
          saved.UicTypeId = current.UicTypeId;
          saved.UicTypeName = current.UicTypeName;




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


          dwRepo.Update<DWDocumentPerson>(saved);
          result = true;
        }





      }
      catch (Exception ex)
      {

        serviceErrorLog.LogError((current.CourtId ?? 0), current.CourtName, "document_person", current.Id, ex.Message);
      }

      return result;

    }

    public IEnumerable<DWDocumentPerson> SelectDocumentPersonTransfer(long documentId, DWCourt court)
    {


      var session_act = repo.AllReadonly<CaseSessionAct>();

      IEnumerable<DWDocumentPerson> result = null;

      result = repo.AllReadonly<DocumentPerson>()


                             .Where(x => x.DocumentId == documentId)


                              .Select(x => new DWDocumentPerson()
                              {
                                Id = x.Id,
                                DateWrt = x.Document.DateWrt,
                                Family2Name = x.Family2Name,
                                FamilyName = x.FamilyName,
                                FirstName = x.FirstName,
                                FullName = x.FullName,
                                LatinName = x.LatinName,
                                MiddleName = x.MiddleName,
                                MilitaryRangId = x.MilitaryRangId,
                                MilitaryRangName = x.MilitaryRang.Label,
                                PersonMaturityId = x.PersonMaturityId,
                                PersonMaturityName = x.PersonMaturity.Label,
                                PersonId = x.PersonId,
                                PersonRoleId = x.PersonRoleId,
                                PersonRoleName = x.PersonRole.Label,
                                Person_SourceId = x.Person_SourceId,
                                //Person_SourceName= to do

                                Person_SourceType = x.Person_SourceType,
                                //Person_SourceTypeName =  to do
                                Uic = x.Uic,
                                UicTypeId = x.UicTypeId,
                                UicTypeName = x.UicType.Label,
                                DocumentId = x.DocumentId,


                                UserId = x.Document.UserId,
                                UserName = x.Document.User.UserName,




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
    public void DocumentPersonTransfer(DWCourt court, long documenId)
    {
      IEnumerable<DWDocumentPerson> dwDocumentsCseInfo = SelectDocumentPersonTransfer(documenId, court);




      foreach (var current in dwDocumentsCseInfo)
      {
        bool insertRow = DocumentPersonInsertUpdate(current);

      }
      //  dwRepo.SaveChanges();





    }

    #endregion
    #region DocumentInstitutionCaseInfo
    public bool DocumentInstitutionCaseInfoInsertUpdate(DWDocumentInstitutionCaseInfo current)
    {
      bool result = false;

      try
      {
        DWDocumentInstitutionCaseInfo saved = dwRepo.All<DWDocumentInstitutionCaseInfo>().Where(x => x.Id == current.Id).FirstOrDefault();
        if (saved == null)

        {
          current.DateTransferedDW = DateTime.Now;

          dwRepo.Add<DWDocumentInstitutionCaseInfo>(current);

          result = true;
        }
        else
        {
          saved.CaseNumber = current.CaseNumber;
          saved.CaseYear = current.CaseYear;
          saved.Description = current.Description;
          saved.DocumentId = current.DocumentId;
          saved.InstitutionCaseTypeId = current.InstitutionCaseTypeId;
          saved.InstitutionCaseTypeName = current.InstitutionCaseTypeName;
          saved.InstitutionId = current.InstitutionId;
          saved.InstitutionName = current.InstitutionName;
          saved.InstitutionCaseTypeId = current.InstitutionCaseTypeId;
          saved.InstitutionCaseTypeName = current.InstitutionCaseTypeName;




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


          dwRepo.Update<DWDocumentInstitutionCaseInfo>(saved);
          result = true;
        }





      }
      catch (Exception ex)
      {

        serviceErrorLog.LogError((current.CourtId ?? 0), current.CourtName, "document_institution_case_info", current.Id, ex.Message);
      }

      return result;

    }

    public IEnumerable<DWDocumentInstitutionCaseInfo> SelectDocumentInstitutionCaseInfoTransfer(long documentId, DWCourt court)
    {




      IEnumerable<DWDocumentInstitutionCaseInfo> result = null;

      result = repo.AllReadonly<DocumentInstitutionCaseInfo>()


                             .Where(x => x.DocumentId == documentId)


                              .Select(x => new DWDocumentInstitutionCaseInfo()
                              {
                                Id = x.Id,
                                InstitutionId = x.InstitutionId,
                                InstitutionName = x.Institution.FullName,
                                InstitutionCaseTypeId = x.InstitutionCaseTypeId,
                                InstitutionCaseTypeName = x.InstitutionCaseType.Label,
                                CaseNumber = x.CaseNumber,
                                CaseYear = x.CaseYear,
                                Description = x.Description,
                                DateWrt = x.Document.DateWrt,
                                DocumentId = x.DocumentId,


                                UserId = x.Document.UserId,
                                UserName = x.Document.User.UserName,
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
    public void DocumentInstitutionCaseInfoTransfer(DWCourt court, long documenId)
    {
      IEnumerable<DWDocumentInstitutionCaseInfo> dwDocumentsInstitutionCseInfo = SelectDocumentInstitutionCaseInfoTransfer(documenId, court);




      foreach (var current in dwDocumentsInstitutionCseInfo)
      {
        bool insertRow = DocumentInstitutionCaseInfoInsertUpdate(current);

      }
      //dwRepo.SaveChanges();





    }

    #endregion

    #region DocumentLink
    public bool DocumentLinkInsertUpdate(DWDocumentLink current)
    {
      bool result = false;

      try
      {
        DWDocumentLink saved = dwRepo.All<DWDocumentLink>().Where(x => x.Id == current.Id).FirstOrDefault();
        if (saved == null)

        {
          current.DateTransferedDW = DateTime.Now;

          dwRepo.Add<DWDocumentLink>(current);

          result = true;
        }
        else
        {
          saved.Id = current.Id;
          saved.PrevDocumentId = current.PrevDocumentId;
          saved.PrevDocumentNumber = current.PrevDocumentNumber;
          saved.PrevDocumentDate = current.PrevDocumentDate;
          saved.Description = current.Description;
          saved.DocumentDirectionId = current.DocumentDirectionId;
          saved.DocumentDirectionName = current.DocumentDirectionName;
          saved.DocumentId = current.DocumentId;




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


          dwRepo.Update<DWDocumentLink>(saved);
          result = true;
        }





      }
      catch (Exception ex)
      {

        serviceErrorLog.LogError((current.CourtId ?? 0), current.CourtName, "document_link", current.Id, ex.Message);
      }

      return result;

    }

    public IEnumerable<DWDocumentLink> SelectDocumentLinkTransfer(long documentId, DWCourt court)
    {



      IEnumerable<DWDocumentLink> result = null;

      result = repo.AllReadonly<DocumentLink>()


                             .Where(x => x.DocumentId == documentId)


                              .Select(x => new DWDocumentLink()
                              {
                                Id = x.Id,
                                Description = x.Description,
                                DocumentDirectionId = x.DocumentDirectionId,
                                DocumentDirectionName = x.DocumentDirection.Label,
                                PrevDocumentDate = x.PrevDocumentDate,
                                PrevDocumentNumber = x.PrevDocumentNumber,
                                PrevDocumentId = x.PrevDocumentId,
                                DocumentId = x.DocumentId,


                                UserId = x.Document.UserId,
                                UserName = x.Document.User.UserName,




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
    public void DocumentLinkTransfer(DWCourt court, long documenId)
    {
      IEnumerable<DWDocumentLink> dwDocumentLink = SelectDocumentLinkTransfer(documenId, court);




      foreach (var current in dwDocumentLink)
      {
        bool insertRow = DocumentLinkInsertUpdate(current);

      }
      // dwRepo.SaveChanges();





    }

    #endregion

    #region DocumentDecision
    public bool DocumentDecisionInsertUpdate(DWDocumentDecision current, DWCourt court)
    {
      bool result = false;

      try
      {
        DWDocumentDecision saved = dwRepo.All<DWDocumentDecision>().Where(x => x.Id == current.Id).FirstOrDefault();
        if (saved == null)

        {
          current.DateTransferedDW = DateTime.Now;

          dwRepo.Add<DWDocumentDecision>(current);

          result = true;
        }
        else
        {
          saved.Id = current.Id;
          saved.DocumentId = current.DocumentId;
          saved.DateWrt = current.DateWrt;
          saved.DecisionTypeId = current.DecisionTypeId;
          saved.DecisionTypeName = current.DecisionTypeName;
          saved.Description = current.Description;
          saved.DocumentDecisionStateId = current.DocumentDecisionStateId;
          saved.DocumentDecisionStateName = current.DocumentDecisionStateName;
          saved.OutDocumentId = current.OutDocumentId;
          saved.UserDecisionId = saved.UserDecisionId;
          saved.UserDecisionName = saved.UserDecisionName;
          saved.RegNumber = current.RegNumber;
          saved.RegDate = current.RegDate;



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


          dwRepo.Update<DWDocumentDecision>(saved);
          result = true;
        }



        if (result)
        {
          DocumentDecisionCaseTransfer(court, current.Id);

        }

      }
      catch (Exception ex)
      {

        serviceErrorLog.LogError((current.CourtId ?? 0), current.CourtName, "document_decision", current.Id, ex.Message);
      }



      return result;

    }

    public IEnumerable<DWDocumentDecision> SelectDocumentDecisionTransfer(int selectedRowCount, DWCourt court)
    {



      Expression<Func<DocumentDecision, bool>> selectedCourt = x => true;
      if (court.CourtId != null)

        selectedCourt = x => x.CourtId == court.CourtId;

      IEnumerable<DWDocumentDecision> result = null;
      DateTime oldDate = new DateTime(1900, 01, 01);
      result = repo.AllReadonly<DocumentDecision>()


                             .Where(selectedCourt)

                             .Where(x => x.DateWrt > (x.DateTransferedDW ?? oldDate))


                              .Select(x => new DWDocumentDecision()
                              {
                                Id = x.Id,
                                DecisionTypeId = x.DecisionTypeId,
                                DecisionTypeName = x.DecisionType.Label,
                                DocumentId = x.DocumentId,
                                Description = x.Description,
                                DocumentDecisionStateId = x.DocumentDecisionStateId,
                                DocumentDecisionStateName = x.DocumentDecisionState.Label,
                                OutDocumentId = x.OutDocumentId,
                                RegDate = x.RegDate,
                                RegNumber = x.RegNumber,
                                UserDecisionId = x.UserDecisionId,
                                UserDecisionName = x.UserDecision.LawUnit.FullName,



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
    public void DocumentDecisionTransfer(DWCourt court)
    {
      serviceErrorLog.LogError((court.CourtId ?? 0), court.CourtName, "DocumentDecisionTransfer", 0, "Стартирал");
      IEnumerable<DWDocumentDecision> dwDocumentsDecisions = SelectDocumentDecisionTransfer(DWConstants.DWTransfer.TransferRowCounts, court);

      bool insertRow = true;
      while (dwDocumentsDecisions.Any() && insertRow)
      {


        foreach (var current in dwDocumentsDecisions)
        {
          insertRow = DocumentDecisionInsertUpdate(current, court);
          if (insertRow)
          {
            var updResult = repo.ExecuteProc<UpdateDateTransferedVM>($"{UpdateDateTransferedVM.ProcedureName}({current.Id},'{UpdateDateTransferedVM.Tables.DocumentDecision}')");

            //var main = repo.GetById<DocumentDecision>(current.Id);
            //main.DateTransferedDW = DateTime.Now;
            //repo.Update<DocumentDecision>(main);
          }

        }
        dwRepo.SaveChanges();
        //repo.SaveChanges();
        //  ts.Complete();
        //}

        dwDocumentsDecisions = SelectDocumentDecisionTransfer(DWConstants.DWTransfer.TransferRowCounts, court);



      }



    }
    #endregion

    #region DocumentDecisionCase
    public bool DocumentDecisionCaseInsertUpdate(DWDocumentDecisionCase current)
    {
      bool result = false;

      try
      {
        DWDocumentDecisionCase saved = dwRepo.All<DWDocumentDecisionCase>().Where(x => x.Id == current.Id).FirstOrDefault();
        if (saved == null)

        {
          current.DateTransferedDW = DateTime.Now;

          dwRepo.Add<DWDocumentDecisionCase>(current);

          result = true;
        }
        else
        {
          saved.Id = current.Id;
          saved.DecisionTypeId = current.DecisionTypeId;
          saved.DecisionTypeName = current.DecisionTypeName;
          saved.DocumentDecisionId = current.DocumentDecisionId;
          saved.Description = current.Description;
          saved.CaseId = current.CaseId;



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


          dwRepo.Update<DWDocumentDecisionCase>(saved);
          result = true;
        }





      }
      catch (Exception ex)
      {

        serviceErrorLog.LogError((current.CourtId ?? 0), current.CourtName, "document_decision_case", current.Id, ex.Message);

      }

      return result;

    }

    public IEnumerable<DWDocumentDecisionCase> SelectDocumentDecisionCaseTransfer(long decisionId, DWCourt court)
    {



      IEnumerable<DWDocumentDecisionCase> result = null;

      result = repo.AllReadonly<DocumentDecisionCase>()


                             .Where(x => x.DocumentDecisionId == decisionId)


                              .Select(x => new DWDocumentDecisionCase()
                              {
                                Id = x.Id,
                                Description = x.Description,
                                DecisionTypeId = x.DecisionTypeId,
                                DecisionTypeName = x.DecisionType.Label,
                                DocumentDecisionId = x.DocumentDecisionId,




                                UserId = x.DocumentDecision.UserId,
                                UserName = x.DocumentDecision.User.LawUnit.FullName,
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
    public void DocumentDecisionCaseTransfer(DWCourt court, long decisionId)
    {
      IEnumerable<DWDocumentDecisionCase> dwDocumentsDecisionCase = SelectDocumentDecisionCaseTransfer(decisionId, court);




      foreach (var current in dwDocumentsDecisionCase)
      {
        bool insertRow = DocumentDecisionCaseInsertUpdate(current);

      }
      // dwRepo.SaveChanges();





    }
    #endregion
  }
}
