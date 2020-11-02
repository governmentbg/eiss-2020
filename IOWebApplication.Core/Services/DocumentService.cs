// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Extensions;
using IOWebApplication.Core.Helper;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Models.Documents;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Documents;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace IOWebApplication.Core.Services
{
    public class DocumentService : BaseService, IDocumentService
    {
        private readonly ICounterService counterService;
        private readonly INomenclatureService nomenclatureService;
        private readonly ICommonService commonService;
        private readonly ICaseClassificationService caseClassificationService;
        private readonly IWorkTaskService workTaskService;
        private readonly IMQEpepService epepService;
        private readonly ICaseDeadlineService deadlineService;
        private readonly ICaseSessionActComplainService caseSessionActComplainService;
        private readonly ICaseSessionDocService caseSessionDocService;
        private readonly ICaseMigrationService caseMigrationService;

        public DocumentService(
            ILogger<DocumentService> _logger,
            ICounterService _counterService,
            INomenclatureService _nomenclatureService,
            ICommonService _commonService,
            ICaseClassificationService _caseClassificationService,
            IWorkTaskService _workTaskService,
            IMQEpepService _epepService,
            ICaseDeadlineService _deadlineService,
            ICaseSessionActComplainService _caseSessionActComplainService,
            ICaseSessionDocService _caseSessionDocService,
            ICaseMigrationService _caseMigrationService,
            IRepository _repo,
            IUserContext _userContext)
        {
            logger = _logger;
            counterService = _counterService;
            nomenclatureService = _nomenclatureService;
            commonService = _commonService;
            caseClassificationService = _caseClassificationService;
            workTaskService = _workTaskService;
            epepService = _epepService;
            deadlineService = _deadlineService;
            caseSessionActComplainService = _caseSessionActComplainService;
            repo = _repo;
            userContext = _userContext;
            caseSessionDocService = _caseSessionDocService;
            caseMigrationService = _caseMigrationService;
        }

        public IQueryable<DocumentListVM> Document_Select(DocumentFilterVM model)
        {
            model.NormalizeValues();
            Expression<Func<Document, bool>> yearSearch = x => true;
            if (model.DocumentYear > 0)
            {
                yearSearch = x => x.DocumentDate.Year == model.DocumentYear;
            }
            Expression<Func<Document, bool>> numberSearch = x => true;
            if (!string.IsNullOrWhiteSpace(model.DocumentNumber))
            {
                model.DocumentNumber = model.DocumentNumber.Trim();
                numberSearch = x => x.DocumentNumber == model.DocumentNumber;
            }
            Expression<Func<Document, bool>> personSearch = x => true;
            if (!string.IsNullOrEmpty(model.PersonName) || model.PersonRoleId > 0)
            {
                //NpgsqlDbFunctionsExtensions.ILike()

                personSearch = x => x.DocumentPersons.Any(p =>
                                                            //EF.Functions.ILike(p.FullName,$"%{model.PersonName}%")
                                                            EF.Functions.ILike(p.FullName, model.PersonName.ToPaternSearch())
                                                            //&& p.FullName.Contains(model.PersonName ?? p.FullName, StringComparison.InvariantCultureIgnoreCase)
                                                            && (p.PersonRoleId == (model.PersonRoleId ?? p.PersonRoleId))
                                                        );
            }

            Expression<Func<Document, bool>> personUicSearch = x => true;
            if (!string.IsNullOrEmpty(model.PersonUIC))
            {
                personUicSearch = x => x.DocumentPersons.Any(p => p.Uic == model.PersonUIC);
            }

            Expression<Func<Document, bool>> courtOrgSearch = x => true;
            if (model.CourtOrganizationId > 0)
            {
                courtOrgSearch = x => x.CourtOrganizationId == model.CourtOrganizationId;
            }

            //СВЪРЗАНИ ДЕЛА
            Expression<Func<Document, bool>> linkCaseCourtWhere = x => true;
            if (model.LinkDelo_CourtId > 0)
                linkCaseCourtWhere = x => x.DocumentCaseInfo.Where(a => a.Case.CourtId == model.LinkDelo_CourtId).Any();

            Expression<Func<Document, bool>> linkCaseIdWhere = x => true;
            if (model.LinkDelo_CaseId > 0)
                linkCaseIdWhere = x => x.DocumentCaseInfo.Where(a => a.CaseId == model.LinkDelo_CaseId).Any();

            Expression<Func<Document, bool>> regNumOtherSystem = x => true;
            if (!string.IsNullOrEmpty(model.RegNumberOtherSystem))
                regNumOtherSystem = x => x.DocumentCaseInfo.Where(a => (a.CaseRegNumber.EndsWith(model.RegNumberOtherSystem.ToShortCaseNumber())) && (a.IsLegacyCase ?? false)).Any();

            List<long> documentIds = new List<long>();
            Expression<Func<Document, bool>> caseRegNumberWhere = x => true;
            if (string.IsNullOrEmpty(model.CaseRegNumber) == false)
            {
                documentIds.AddRange(repo.AllReadonly<Case>()
                     .Where(x => x.RegNumber != null)
                     .Where(x => x.RegNumber.EndsWith(model.CaseRegNumber.ToShortCaseNumber(),
                                                     StringComparison.InvariantCultureIgnoreCase))
                     .Select(x => x.DocumentId));

                documentIds.AddRange(repo.AllReadonly<DocumentCaseInfo>()
                     .Where(x => x.CaseId != null)
                     .Where(x => x.Case.RegNumber.EndsWith(model.CaseRegNumber.ToShortCaseNumber(),
                                                     StringComparison.InvariantCultureIgnoreCase))
                     .Select(x => x.DocumentId));

                caseRegNumberWhere = x => documentIds.Contains(x.Id);
            }
            //caseRegNumberWhere = x => x.Cases.Any() ? x.Cases.Where(a => a.RegNumber != null && 
            //                                     a.RegNumber.EndsWith(model.CaseRegNumber.ToShortCaseNumber(),
            //                                     StringComparison.InvariantCultureIgnoreCase)).Any() :
            //                                     x.DocumentCaseInfo.Where(a => a.CaseId != null &&
            //                                     a.Case.RegNumber.EndsWith(model.CaseRegNumber.ToShortCaseNumber(),
            //                                     StringComparison.InvariantCultureIgnoreCase)).Any();

            int _court = userContext.CourtId;
            var result = repo.AllReadonly<Document>()
                             .Include(x => x.DocumentPersons)
                             .ThenInclude(x => x.PersonRole)
                             .Include(x => x.DocumentType)
                             .ThenInclude(x => x.DocumentGroup)
                             .Include(x => x.DocumentDirection)
                             .Include(x => x.User)
                             .ThenInclude(x => x.LawUnit)
                             .Include(x => x.Cases)
                             .Include(x => x.DocumentCaseInfo)
                             .ThenInclude(x => x.Case)
                             .Where(x => x.CourtId == _court)
                             .Where(courtOrgSearch)
                             .Where(x => x.DocumentDirectionId == (model.DocumentDirectionId ?? x.DocumentDirectionId))
                             .Where(x => x.DocumentType.DocumentGroup.DocumentKindId == (model.DocumentKindId ?? x.DocumentType.DocumentGroup.DocumentKindId))
                             .Where(x => x.DocumentGroupId == (model.DocumentGroupId ?? x.DocumentGroupId))
                             .Where(x => x.DocumentTypeId == (model.DocumentTypeId ?? x.DocumentTypeId))
                             .Where(yearSearch)
                             .Where(numberSearch)
                             .Where(personSearch)
                             .Where(personUicSearch)
                             .Where(FilterExpireInfo<Document>(false))
                             .Where(x => x.DocumentDate >= (model.DateFrom ?? x.DocumentDate) && x.DocumentDate <= (model.DateTo ?? x.DocumentDate))
                             .Where(linkCaseCourtWhere)
                             .Where(linkCaseIdWhere)
                             .Where(regNumOtherSystem)
                             .Where(caseRegNumberWhere)
                             .Select(x => new DocumentListVM
                             {
                                 Id = x.Id,
                                 DocumentTypeName = x.DocumentType.Label,
                                 DocumentDirectionName = x.DocumentDirection.Code,
                                 DocumentNumber = x.DocumentNumber,
                                 DocumentDate = x.DocumentDate,
                                 UserName = (x.User != null) ? x.User.LawUnit.FullName : "",
                                 Persons = x.DocumentPersons.Select(p => new DocumentListPersonVM
                                 {
                                     Id = p.Id,
                                     Uic = p.Uic,
                                     Name = p.FullName,
                                     RoleName = p.PersonRole.Label
                                 }),
                                 CaseId = (x.Cases.Any()) ? x.Cases.Select(c => c.Id).FirstOrDefault() : x.DocumentCaseInfo.Select(dt => dt.CaseId ?? 0).FirstOrDefault(),
                                 CaseNumber = (x.Cases.Any()) ? x.Cases.Select(c => c.RegNumber).FirstOrDefault() : x.DocumentCaseInfo.Select(dt => (dt.Case != null) ? dt.Case.RegNumber : "").FirstOrDefault(),
                                 IsCaseRejected = x.Cases.Any() ? x.Cases.Select(c => c.CaseStateId).FirstOrDefault() == NomenclatureConstants.CaseState.Rejected : false,
                                 DocumentNumberValue = x.DocumentNumberValue ?? 0,
                                 Description = x.Description
                             }).AsQueryable();

            var sql = result.ToSql();
            return result;
        }

        public DocumentVM Document_Init(int documentDirection, int templateId = 0)
        {
            var model = new DocumentVM()
            {
                DocumentDirectionId = documentDirection
            };

            model.DocumentCaseInfo.CourtId = userContext.CourtId;
            model.ProcessPriorityId = DocumentConstants.ProcessPriority.Common;
            model.CaseClassifications = caseClassificationService.FillCheckListVMs(0, 0);
            if (templateId > 0)
            {
                document_InitFromTemplate(model, templateId);
            }

            return model;
        }

        private void document_InitFromTemplate(DocumentVM model, int templateId)
        {
            var template = this.GetById<DocumentTemplate>(templateId);
            if (template == null)
            {
                return;
            }
            model.TemplateId = templateId;
            model.DocumentKindId = template.DocumentKindId;
            model.DocumentGroupId = template.DocumentGroupId;
            model.DocumentTypeId = template.DocumentTypeId;
            model.Description = template.Description;
            if (template.CaseId > 0)
            {
                var caseModel = repo.GetById<Case>(template.CaseId);
                if (caseModel != null)
                {
                    model.HasCaseInfo = true;
                    model.DocumentCaseInfo.CourtId = caseModel.CourtId;
                    model.DocumentCaseInfo.CaseId = caseModel.Id;
                    if (template.SourceType == SourceTypeSelectVM.CaseSessionAct)
                    {
                        model.DocumentCaseInfo.HasLawAct = true;
                        model.DocumentCaseInfo.SessionActId = (int)template.SourceId;
                    }
                }
            }

            //Да се върже входящия документ за да се избират лицата по него в изходящия
            if (template.SourceType == SourceTypeSelectVM.DocumentDecision)
            {
                var documentDecision = repo.GetById<DocumentDecision>(template.SourceId);
                if (documentDecision != null)
                    model.PriorDocumentId = documentDecision.DocumentId;
            }
            // Да се добави лицето от призовка/съобщение 
            //if (template.SourceType == SourceTypeSelectVM.CaseNotification)
            //{
            //    var caseNotification = repo.AllReadonly<CaseNotification>()
            //                               .Where(x => x.Id == template.SourceId)
            //                               .FirstOrDefault();
            //    if (caseNotification != null)
            //    {
            //        var person = repo.AllReadonly<CasePerson>()
            //                             .Include(x => x.Addresses)
            //                             .ThenInclude(x => x.Address)
            //                             .Where(x => x.Id == caseNotification.CasePersonId)
            //                             .FirstOrDefault();
            //        if (person != null)
            //        {
            //            var docPerson = new DocumentPersonVM()
            //            {
            //                Index = 0
            //            };
            //            docPerson.CopyFrom(person);
            //            docPerson.PersonRoleId = person.PersonRoleId;
            //            docPerson.MilitaryRangId = person.MilitaryRangId;
            //            docPerson.PersonMaturityId = person.PersonMaturityId;
            //            foreach (var pAdr in person.Addresses)
            //            {
            //                var docPersonAddress = new DocumentPersonAddressVM()
            //                {
            //                    PersonIndex = docPerson.Index,
            //                    Index = docPerson.Addresses.Count
            //                };
            //                docPersonAddress.Address.CopyFrom(pAdr.Address);
            //                docPerson.Addresses.Add(docPersonAddress);
            //            }
            //            model.DocumentPersons.Add(docPerson);
            //        }
            //        //model.DocumentPersons
            //    }
            //}
        }

        private Document document_GetById(long id, bool readOnly)
        {
            IQueryable<Document> documents = repo.All<Document>();
            if (readOnly)
            {
                documents = repo.AllReadonly<Document>();
            }
            var document = documents
                            .Include(x => x.DocumentType)
                            .Include(x => x.DocumentGroup)
                            .Include(x => x.DocumentPersons)
                            .ThenInclude(x => x.Addresses)
                            .ThenInclude(x => x.Address)
                            .Include(x => x.DocumentCaseInfo)
                            .Include(x => x.DocumentInstitutionCaseInfo)
                            .ThenInclude(x => x.Institution)
                            .Include(x => x.DocumentLinks)
                            .Include(x => x.Cases)
                            .Include(x => x.DocumentResolutions)
                            .Where(x => x.Id == id)
                            .FirstOrDefault();
            return document;
        }
        public DocumentVM Document_GetById(long id)
        {
            var document = document_GetById(id, true);
            if (document == null)
            {
                return null;
            }
            var templateId = repo.AllReadonly<DocumentTemplate>()
                                    .Where(x => x.DocumentId == id)
                                    .Select(x => x.Id)
                                    .FirstOrDefault();
            var model = new DocumentVM()
            {
                Id = document.Id,
                CourtOrganizationId = document.CourtOrganizationId,
                DocumentNumber = document.DocumentNumber,
                DocumentDate = document.DocumentDate,
                DocumentDirectionId = document.DocumentDirectionId,
                DocumentKindId = document.DocumentGroup.DocumentKindId,
                DocumentGroupId = document.DocumentGroupId,
                DocumentTypeId = document.DocumentTypeId,
                Description = document.Description,
                DeliveryGroupId = document.DeliveryGroupId,
                DeliveryTypeId = document.DeliveryTypeId,
                IsRestictedAccess = document.IsRestictedAccess,
                IsSecret = document.IsSecret ?? false,
                IsOldNumber = document.IsOldNumber ?? false,
                OldDocumentDate = document.DocumentDate,
                OldDocumentNumber = document.DocumentNumber,
                ActualDocumentDate = document.ActualDocumentDate,
                MultiRegistationId = document.MultiRegistationId,
                HasDocumentResolutions = document.DocumentResolutions.AsQueryable()
                                            .Where(FilterExpireInfo<DocumentResolution>(false)).Any()
            };
            if (!string.IsNullOrEmpty(model.MultiRegistationId))
            {
                var mdocInfo = repo.AllReadonly<Document>()
                                        .Where(x => x.MultiRegistationId == model.MultiRegistationId)
                                        .OrderBy(x => x.Id)
                                        .Select(x => new
                                        {
                                            x.Id,
                                            x.DocumentNumberValue
                                        });
                if (mdocInfo.Any())
                {
                    var currentIndex = mdocInfo.Where(x => x.Id <= model.Id).OrderBy(x => x.Id).Count();

                    model.MultiRegistationInfo = $"Документ {currentIndex} от {mdocInfo.Count()}; От номер {mdocInfo.FirstOrDefault().DocumentNumberValue} до номер {mdocInfo.LastOrDefault().DocumentNumberValue}";
                }
            }
            if (templateId > 0)
            {
                model.TemplateId = templateId;
            }
            foreach (var docPerson in document.DocumentPersons)
            {
                var person = new DocumentPersonVM()
                {
                    Id = docPerson.Id,
                    MilitaryRangId = docPerson.MilitaryRangId,
                    PersonRoleId = docPerson.PersonRoleId,
                    PersonMaturityId = docPerson.PersonMaturityId
                };
                person.CopyFrom(docPerson);

                foreach (var pAddress in docPerson.Addresses)
                {
                    var address = new DocumentPersonAddressVM()
                    {
                        Id = pAddress.Id,
                        Address = pAddress.Address
                    };
                    person.Addresses.Add(address);
                }

                model.DocumentPersons.Add(person);
            }
            if (document.DocumentCaseInfo.Count > 0)
            {
                model.HasCaseInfo = true;
                var savedCaseInfo = document.DocumentCaseInfo.FirstOrDefault();
                model.DocumentCaseInfo = new DocumentCaseInfoVM()
                {
                    Id = savedCaseInfo.Id,
                    CourtId = savedCaseInfo.CourtId,
                    CaseYear = savedCaseInfo.CaseYear,
                    IsLegacyCase = savedCaseInfo.IsLegacyCase ?? false,
                    CaseRegNumber = savedCaseInfo.CaseRegNumber,
                    CaseShortNumber = savedCaseInfo.CaseShortNumber,
                    CaseId = savedCaseInfo.CaseId,

                    SessionActId = savedCaseInfo.SessionActId,

                    Description = savedCaseInfo.Description,

                    HasLawAct = savedCaseInfo.SessionActId > 0
                };
            }
            if (document.DocumentInstitutionCaseInfo.Count > 0)
            {
                foreach (var link in document.DocumentInstitutionCaseInfo)
                {
                    var item = new DocumentInstitutionCaseInfoVM()
                    {
                        Id = link.Id,
                        InstitutionTypeId = link.Institution.InstitutionTypeId,
                        InstitutionId = link.InstitutionId,
                        InstitutionCaseTypeId = link.InstitutionCaseTypeId,
                        CaseNumber = link.CaseNumber,
                        CaseYear = link.CaseYear,
                        Description = link.Description
                    };
                    model.InstitutionCaseInfo.Add(item);
                }
            }
            if (document.DocumentLinks.Count > 0)
            {
                foreach (var link in document.DocumentLinks)
                {
                    var docLink = new DocumentLinkVM()
                    {
                        Id = link.Id,
                        CourtId = link.CourtId,
                        DocumentDirectionId = link.DocumentDirectionId,
                        PrevDocumentDate = link.PrevDocumentDate,
                        PrevDocumentId = link.PrevDocumentId,
                        PrevDocumentNumber = link.PrevDocumentNumber,
                        Description = link.Description
                    };
                    docLink.IsLegacyDocument = !string.IsNullOrEmpty(docLink.PrevDocumentNumber) || (docLink.PrevDocumentId == null);
                    model.DocumentLinks.Add(docLink);
                }
            }
            if (document.Cases.Count > 0)
            {
                var _case = document.Cases.First();
                model.CaseId = _case.Id;
                model.CaseTypeId = _case.CaseTypeId;
                model.CaseCodeId = _case.CaseCodeId;
                model.EISSPNumber = _case.EISSPNumber;
                model.ProcessPriorityId = _case.ProcessPriorityId;
                model.CaseRegisterNumber = _case.RegNumber;
                model.CaseClassifications = caseClassificationService.FillCheckListVMs(_case.Id, 0);
            }
            return model;
        }

        private void document_UpdateNullables(DocumentVM model)
        {
            model.DeliveryGroupId = model.DeliveryGroupId.EmptyToNull();
            model.DeliveryTypeId = model.DeliveryTypeId.EmptyToNull();
            model.ProcessPriorityId = model.ProcessPriorityId.EmptyToNull();
            model.CaseCodeId = model.CaseCodeId.EmptyToNull().EmptyToNull(0);
            if (!model.DocumentCaseInfo.HasLawAct)
            {
                model.DocumentCaseInfo.SessionActId = null;
            }
            foreach (var person in model.DocumentPersons)
            {
                person.MilitaryRangId = person.MilitaryRangId.EmptyToNull();
                person.PersonMaturityId = person.PersonMaturityId.EmptyToNull();
            }
            if (model.DocumentCaseInfo != null)
            {
                model.DocumentCaseInfo.CaseId = model.DocumentCaseInfo.CaseId.EmptyToNull().EmptyToNull(0);
                model.EISSPNumber = model.EISSPNumber.EmptyToNull();
                if (!string.IsNullOrEmpty(model.EISSPNumber))
                {
                    model.EISSPNumber = model.EISSPNumber.ToUpper();
                }
            }
            if (!string.IsNullOrEmpty(model.OldDocumentNumber))
            {
                model.OldDocumentNumber = model.OldDocumentNumber.Trim();
            }
            else
            {
                model.OldDocumentNumber = model.OldDocumentNumber.EmptyToNull();
            }
        }

        public bool Document_SaveData(DocumentVM model)
        {
            document_UpdateNullables(model);

            if (model.Id > 0)
            {
                return document_UpdateData(model);
            }
            else
            {
                return document_InsertDataMulti(model);
            }
        }

        private bool document_InsertDataMulti(DocumentVM model)
        {
            model.MultiRegistationId = null;
            // model.MultiDocumentCounter = 5;
            bool result = true;
            if (model.MultiDocumentCounter > 1)
            {
                var counterResult = counterService.Counter_GetDocumentCounterMulti(model.MultiDocumentCounter, model.DocumentDirectionId, userContext.CourtId);
                result = (counterResult != null) && (counterResult.Value > 0);
                int currentDocumentNumber = counterResult.Value - model.MultiDocumentCounter + 1;
                var dtDocDate = DateTime.Now;
                model.MultiRegistationId = Guid.NewGuid().ToString().ToLower();
                long firstDocumentId = 0;
                for (int counterAdd = 0; counterAdd < model.MultiDocumentCounter; counterAdd++)
                {
                    counterResult.Value = currentDocumentNumber + counterAdd;
                    model.DocumentNumberValue = counterResult.Value;
                    model.DocumentNumber = counterResult.GetStringValue();
                    model.DocumentDate = dtDocDate;
                    result &= document_InsertData(model);
                    if (firstDocumentId == 0)
                    {
                        firstDocumentId = model.Id;
                    }
                    //Връща екрана в първия документ от групата
                    model.Id = firstDocumentId;
                }
            }
            else
            {
                result = document_InsertData(model);
            }
            return result;
        }

        private bool document_InsertData(DocumentVM model)
        {
            var document = new Document()
            {
                CourtId = userContext.CourtId,
                CourtOrganizationId = model.CourtOrganizationId,
                DocumentDirectionId = model.DocumentDirectionId,
                DocumentGroupId = model.DocumentGroupId,
                DocumentTypeId = model.DocumentTypeId.Value,
                DeliveryGroupId = model.DeliveryGroupId,
                DeliveryTypeId = model.DeliveryTypeId,
                IsRestictedAccess = model.IsRestictedAccess,
                IsSecret = model.IsSecret,
                IsOldNumber = model.IsOldNumber,
                MultiRegistationId = model.MultiRegistationId.EmptyToNull(),
                Description = model.Description,
                DateWrt = DateTime.Now,
                UserId = userContext.UserId
            };
            //Запис на лица и адреси
            document_SavePersons(model, document);
            document_SaveCaseInfo(model, document);
            document_SaveInstitutionCaseInfo(model, document);
            document_SaveDocumentLink(model, document);

            if (model.CaseTypeId > 0)
            {
                document_InitNewCase(model, document);
            }
            try
            {
                bool isOkNumber = false;
                document.ActualDocumentDate = DateTime.Now;
                if (document.IsOldNumber == true)
                {
                    isOkNumber = true;
                    document.DocumentNumber = model.OldDocumentNumber;
                    document.DocumentDate = model.OldDocumentDate.Value;
                    try
                    {
                        document.DocumentNumberValue = int.Parse(document.DocumentNumber);
                    }
                    catch (Exception ee)
                    {

                    }
                }
                else
                {
                    if (model.MultiDocumentCounter > 1)
                    {
                        isOkNumber = true;
                        document.DocumentNumber = model.DocumentNumber;
                        document.DocumentNumberValue = model.DocumentNumberValue;
                        document.DocumentDate = model.DocumentDate;
                        document.ActualDocumentDate = document.DocumentDate;
                    }
                    else
                    {
                        isOkNumber = counterService.Counter_GetDocumentCounter(document);
                    }
                }
                //Регистриране на номер
                if (isOkNumber)
                {
                    repo.Add<Document>(document);
                    repo.SaveChanges();
                    model.Id = document.Id;

                    epepService.AppendDocument(document, EpepConstants.ServiceMethod.Add);
                    deadlineService.DeadLineCompanyCaseStartOnDocument(document);
                    repo.SaveChanges();

                    if ((document.DocumentDirectionId == DocumentConstants.DocumentDirection.Incoming) &&
                        (document.DocumentGroupId == NomenclatureConstants.DocumentGroup.DocumentForComplain_AccompanyingDocument))
                    {
                        caseSessionActComplainService.CaseSessionActComplain_CreateFromDocument(document.Id);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при регистрация на документ Id={ model.Id }");
            }
            return false;
        }
        private bool document_UpdateData(DocumentVM model)
        {
            var document = document_GetById(model.Id, false); ;
            document.CourtOrganizationId = model.CourtOrganizationId;
            document.DocumentGroupId = model.DocumentGroupId;
            document.DocumentTypeId = model.DocumentTypeId.Value;
            document.Description = model.Description;
            document.IsRestictedAccess = model.IsRestictedAccess;
            document.IsSecret = model.IsSecret;
            document.DeliveryGroupId = model.DeliveryGroupId;
            document.DeliveryTypeId = model.DeliveryTypeId;

            document_SavePersons(model, document);
            document_UpdateCase(model);
            document_SaveCaseInfo(model, document);
            document_SaveInstitutionCaseInfo(model, document);
            document_SaveDocumentLink(model, document);

            repo.Update<Document>(document);
            deadlineService.DeadLineCompanyCaseStartOnDocument(document);
            try
            {
                repo.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при редакция на документ Id={ model.Id }");
                return false;
            }
        }

        private void document_SavePersons(DocumentVM model, Document document)
        {
            if (document != null)
            {
                foreach (var savedPerson in document.DocumentPersons)
                {
                    var modelPerson = model.DocumentPersons.FirstOrDefault(x => x.Id == savedPerson.Id);
                    if (modelPerson == null)
                    {
                        //Премахва лицето и всички адреси
                        foreach (var sPersonAddress in savedPerson.Addresses)
                        {
                            repo.Delete<Address>(sPersonAddress.AddressId);
                        }
                        repo.Delete<DocumentPerson>(savedPerson.Id);
                    }
                    else
                    {
                        //Update на savedPerson
                        foreach (var sPersonAddress in savedPerson.Addresses)
                        {
                            var modelAddress = modelPerson.Addresses.FirstOrDefault(x => x.Id == sPersonAddress.Id);
                            if (modelAddress == null)
                            {
                                //Премахва адреса, ако е изтрит от модела на View-то
                                repo.Delete<Address>(sPersonAddress.Id);
                                repo.Delete<DocumentPersonAddress>(sPersonAddress);
                            }
                            else
                            {
                                //Редактира адреса, ако го има
                                sPersonAddress.Address.CopyFrom(modelAddress.Address);
                                nomenclatureService.SetFullAddress(sPersonAddress.Address);
                                repo.Update<Address>(sPersonAddress.Address);
                            }
                        }
                        //Добавя всички новодобавени адреси
                        foreach (var newModelAddress in modelPerson.Addresses.Where(x => x.Id == 0))
                        {
                            var newAddress = new DocumentPersonAddress()
                            {
                                Address = new Address()
                            };
                            newAddress.Address.CopyFrom(newModelAddress.Address);
                            nomenclatureService.SetFullAddress(newAddress.Address);
                            savedPerson.Addresses.Add(newAddress);
                        }
                        savedPerson.CopyFrom(modelPerson);
                        savedPerson.PersonRoleId = modelPerson.PersonRoleId;
                        savedPerson.PersonMaturityId = modelPerson.PersonMaturityId;
                        savedPerson.MilitaryRangId = modelPerson.MilitaryRangId;
                        PersonNamesBase_SaveData(savedPerson);
                        repo.Update<DocumentPerson>(savedPerson);
                    }
                }
            }

            //Добавяне на новите лица
            if (model.DocumentPersons?.Count > 0)
                foreach (var person in model.DocumentPersons.Where(x => x.Id == 0))
                {
                    var newElement = new DocumentPerson()
                    {
                        PersonRoleId = person.PersonRoleId,
                        PersonMaturityId = person.PersonMaturityId,
                        MilitaryRangId = person.MilitaryRangId
                    };
                    newElement.CopyFrom(person);
                    PersonNamesBase_SaveData(newElement);
                    if (person.Addresses?.Count > 0)
                        foreach (var address in person.Addresses)
                        {
                            var docPersonAddress = new DocumentPersonAddress()
                            {
                                Address = new Address()
                            };
                            docPersonAddress.Address.CopyFrom(address.Address);
                            nomenclatureService.SetFullAddress(docPersonAddress.Address);
                            newElement.Addresses.Add(docPersonAddress);
                        }
                    document.DocumentPersons.Add(newElement);
                }
        }

        private void document_SaveCaseInfo(DocumentVM model, Document document)
        {
            if (model.HasCaseInfo && model.DocumentCaseInfo != null)
            {
                var saved = document.DocumentCaseInfo.FirstOrDefault(x => x.Id == model.DocumentCaseInfo.Id);
                if (saved != null)
                {
                    model.DocumentCaseInfo.ToEntity(saved);
                    repo.Update<DocumentCaseInfo>(saved);
                }
                else
                {
                    DocumentCaseInfo caseInfo = new DocumentCaseInfo();
                    model.DocumentCaseInfo.ToEntity(caseInfo);
                    document.DocumentCaseInfo.Add(caseInfo);
                }
            }
            else
            {
                var saved = document.DocumentCaseInfo;
                if (saved.Count > 0)
                {
                    repo.DeleteRange<DocumentCaseInfo>(saved);
                }
            }
        }

        private void document_SaveInstitutionCaseInfo(DocumentVM model, Document document)
        {
            if (document != null)
            {
                foreach (var savedCaseInfo in document.DocumentInstitutionCaseInfo)
                {
                    var modelCaseInfo = model.InstitutionCaseInfo.FirstOrDefault(x => x.Id == savedCaseInfo.Id);
                    if (modelCaseInfo == null)
                    {
                        //Премахва изтритите елементи
                        repo.Delete<DocumentInstitutionCaseInfo>(savedCaseInfo.Id);
                    }
                    else
                    {
                        //Редактира съществуващите елементи
                        modelCaseInfo.ToEntity(savedCaseInfo);
                        repo.Update<DocumentInstitutionCaseInfo>(savedCaseInfo);
                    }
                }
            }

            //Добавяне на новите елементи
            if (model.InstitutionCaseInfo?.Count > 0)
                foreach (var caseinfo in model.InstitutionCaseInfo.Where(x => x.Id == 0))
                {
                    var newElement = new DocumentInstitutionCaseInfo();
                    caseinfo.ToEntity(newElement);
                    document.DocumentInstitutionCaseInfo.Add(newElement);
                }
        }

        private void document_SaveDocumentLink(DocumentVM model, Document document)
        {
            if (document != null)
            {
                foreach (var savedLink in document.DocumentLinks)
                {
                    var modelLink = model.DocumentLinks.FirstOrDefault(x => x.Id == savedLink.Id);
                    if (modelLink == null)
                    {
                        //Премахва изтритите елементи
                        repo.Delete<DocumentLink>(savedLink.Id);
                    }
                    else
                    {
                        //Редактира съществуващите елементи
                        modelLink.ToEntity(savedLink);
                        repo.Update<DocumentLink>(savedLink);
                    }
                }
            }

            //Добавяне на новите елементи
            if (model.DocumentLinks?.Count > 0)
                foreach (var link in model.DocumentLinks.Where(x => x.Id == 0))
                {
                    var newElement = new DocumentLink();
                    link.ToEntity(newElement);
                    document.DocumentLinks.Add(newElement);
                }
        }
        /// <summary>
        /// Стартиране на ново дело по подаден документ
        /// </summary>
        /// <param name="model"></param>
        /// <param name="entity"></param>
        private void document_InitNewCase(DocumentVM model, Document entity)
        {

            //var docCaseType = repo.AllReadonly<DocumentTypeCaseType>().FirstOrDefault(x => x.DocumentTypeId == model.DocumentTypeId && x.CaseTypeId == model.CaseTypeId);
            //if (docCaseType == null)
            //{
            //    //Документа не е иницииращ
            //    return;
            //}

            var newCase = new Case()
            {
                CourtId = entity.CourtId,
                CaseGroupId = repo.GetById<CaseType>(model.CaseTypeId).CaseGroupId,
                CaseCharacterId = repo.AllReadonly<CaseTypeCharacter>(x => x.CaseTypeId == model.CaseTypeId).Select(x => x.CaseCharacterId).First(),
                CaseTypeId = model.CaseTypeId.Value,
                CaseCodeId = model.CaseCodeId,
                CaseStateId = NomenclatureConstants.CaseState.Draft,
                IsRestictedAccess = model.IsRestictedAccess,
                EISSPNumber = model.EISSPNumber,
                ProcessPriorityId = model.ProcessPriorityId,
                UserId = userContext.UserId,
                DateWrt = DateTime.Now
            };
            var classificationSecret = repo.AllReadonly<Classification>().Where(x => x.Code == "secret").FirstOrDefault();
            var classificationRestricted = repo.AllReadonly<Classification>().Where(x => x.Code == "restricted").FirstOrDefault();
            var classificationMinor = repo.AllReadonly<Classification>().Where(x => x.Code == "minors").FirstOrDefault();
            foreach (var item in model.CaseClassifications)
            {
                if (item.Value == classificationSecret.Id.ToString())
                {
                    if (!item.Checked && model.IsSecret)
                    {
                        item.Checked = true;
                    }
                }
                if (item.Value == classificationRestricted.Id.ToString())
                {
                    if (!item.Checked && model.IsRestictedAccess)
                    {
                        item.Checked = true;
                    }
                }
                if (item.Value == classificationMinor.Id.ToString())
                {
                    if (model.DocumentPersons.Any(x => x.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderAged || x.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderLegalAge) && !item.Checked)
                    {
                        item.Checked = true;
                    }
                }
            }
            newCase.CaseClassifications = model.CaseClassifications
                                            .Where(x => x.Checked)
                                            .Select(x => new CaseClassification()
                                            {
                                                DateFrom = DateTime.Now,
                                                CaseSessionId = null,
                                                ClassificationId = int.Parse(x.Value, System.Globalization.CultureInfo.InvariantCulture)
                                            }).ToList();

            entity.Cases = entity.Cases ?? new List<Case>();
            entity.Cases.Add(newCase);
        }
        private void document_UpdateCase(DocumentVM model)
        {
            var caseModel = repo.All<Case>(x => x.DocumentId == model.Id).FirstOrDefault();
            if (caseModel != null && caseModel.CaseStateId == NomenclatureConstants.CaseState.Draft)
            {
                caseModel.EISSPNumber = model.EISSPNumber;
                caseModel.ProcessPriorityId = model.ProcessPriorityId;
                caseModel.CaseTypeId = model.CaseTypeId.Value;
                caseModel.CaseGroupId = repo.GetById<CaseType>(model.CaseTypeId).CaseGroupId;
                caseModel.CaseCodeId = model.CaseCodeId;
                caseModel.CaseCharacterId = repo.AllReadonly<CaseTypeCharacter>(x => x.CaseTypeId == model.CaseTypeId).Select(x => x.CaseCharacterId).First();
                repo.Update<Case>(caseModel);

                repo.DeleteRange<CaseClassification>(x => x.CaseId == caseModel.Id);
                var newCaseClassifications = model.CaseClassifications
                                            .Where(x => x.Checked)
                                            .Select(x => new CaseClassification()
                                            {
                                                CaseId = caseModel.Id,
                                                DateFrom = DateTime.Now,
                                                CaseSessionId = null,
                                                ClassificationId = int.Parse(x.Value, System.Globalization.CultureInfo.InvariantCulture)
                                            }).ToList();
                repo.AddRange(newCaseClassifications);
            }
        }





        public List<SelectListItem> GetDeliveryGroups(int documentDirection)
        {
            return repo.AllReadonly<DeliveryDirectionGroup>()
                        .Include(x => x.DeliveryGroup)
                        .Where(x => x.DocumentDirectionId == documentDirection)
                        .Select(x => x.DeliveryGroup)
                        .OrderBy(x => x.Label)
                        .ToSelectList(x => x.Id, x => x.Label);
        }

        public IEnumerable<LabelValueVM> GetDocument(int courtId, string documentNumber)
        {
            documentNumber = documentNumber?.ToLower();

            var result = repo.AllReadonly<Document>()
                            .Include(x => x.DocumentType)
                            .Where(x => x.CourtId == courtId)
                            .Where(x => x.DocumentNumber.ToLower() == documentNumber)
                            .OrderBy(x => x.DocumentDate)
                            .Select(x => new LabelValueVM
                            {
                                Value = x.Id.ToString(),
                                Label = x.DocumentType.Label + " " + (x.DocumentNumber ?? "") + "/" + x.DocumentDate.ToString("dd.MM.yyyy")
                            }).ToList();

            return result;
        }

        public LabelValueVM GetDocumentById(int id)
        {
            return repo.AllReadonly<Document>().Where(x => x.Id == id)
                            .Include(x => x.DocumentType)
                        .Select(x => new LabelValueVM
                        {
                            Value = x.Id.ToString(),
                            Label = x.DocumentType.Label + " " + (x.DocumentNumber ?? "") + "/" + x.DocumentDate.ToString("dd.MM.yyyy")
                        }).ToList().DefaultIfEmpty(null).FirstOrDefault();
        }

        public List<SelectListItem> DocumentPerson_SelectForDropDownList(long documentId)
        {
            var result = repo.AllReadonly<DocumentPerson>()
                .Include(x => x.PersonRole)
                .Where(x => x.DocumentId == documentId)
                 .OrderBy(x => x.FullName)
                                 .Select(x => new SelectListItem()
                                 {
                                     Value = x.Id.ToString(),
                                     Text = x.FullName + "(" + (x.Uic ?? "") + ") - " + x.PersonRole.Label
                                 }).ToList();

            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });

            return result;
        }

        public bool CheckDocumentOldNumber(int courtId, int docDirectionId, string documentNumber, DateTime documentDate)
        {
            return !repo.AllReadonly<Document>()
                            .Where(x => x.CourtId == courtId
                            && x.DocumentNumber == documentNumber
                            && x.DocumentDate.Year == documentDate.Year
                            && x.DocumentDirectionId == docDirectionId)
                            .Select(x => x.Id)
                            .Any();
        }

        public string GetDataInstitutionCaseInfoForDocument(long documentId)
        {
            var result = string.Empty;

            var institutionCaseInfos = repo.AllReadonly<DocumentInstitutionCaseInfo>()
                                           .Include(x => x.Institution)
                                           .Include(x => x.InstitutionCaseType)
                                           .Where(x => x.DocumentId == documentId)
                                           .ToList();

            foreach (var institutionCaseInfo in institutionCaseInfos)
            {
                if (result != string.Empty) result += "; ";

                result += (institutionCaseInfo.InstitutionCaseType != null ? institutionCaseInfo.InstitutionCaseType.Label + " " : string.Empty) +
                          "№ " + institutionCaseInfo.CaseNumber + "/" + institutionCaseInfo.CaseYear +
                          (institutionCaseInfo.Institution != null ? " - " + institutionCaseInfo.Institution.FullName : string.Empty);
            }

            return result;
        }

        public DocumentSelectPersonsVM Case_SelectPersons(int caseId)
        {
            DocumentSelectPersonsVM model = new DocumentSelectPersonsVM();
            var caseModel = repo.AllReadonly<Case>()
                                        .Include(x => x.CaseType)
                                        .Include(x => x.CasePersons)
                                        .ThenInclude(x => x.Addresses)
                                        .ThenInclude(x => x.Address)
                                        .ThenInclude(x => x.AddressType)
                                        .Include(x => x.CasePersons)
                                        .ThenInclude(x => x.PersonRole)
                                        .Where(x => x.Id == caseId)
                                        .FirstOrDefault();

            if (caseModel == null)
            {
                return null;
            }

            model.SourceType = SourceTypeSelectVM.Case;
            model.SourceId = caseId.ToString();
            model.SourceTypeName = $"Лица по {caseModel.CaseType.Code} {caseModel.RegNumber} / {caseModel.RegDate:dd.MM.yyyy}";
            foreach (var item in caseModel.CasePersons
                                            .Where(x => (x.CaseSessionId ?? -1) == -1)
                                            .Where(x => (x.DateTo ?? DateTime.Now.AddDays(1)) >= DateTime.Now))
            {
                var newPerson = new DocumentSelectPersonItemVM()
                {
                    Id = item.Id.ToString(),
                    IsChecked = true,
                    Uic = item.Uic,
                    UicTypeLabel = item.UicTypeLabel,
                    FullName = item.FullName,
                    RoleName = item.PersonRole.Label
                };
                foreach (var adr in item.Addresses)
                {
                    var newAdr = new DocumentSelectAddressVM()
                    {
                        Id = adr.AddressId,
                        IsChecked = true,
                        AddressTypeName = adr.Address.AddressType.Label,
                        FullAddress = adr.Address.FullAddress,
                    };
                    newPerson.Addresses.Add(newAdr);
                }
                model.Persons.Add(newPerson);
            }

            return model;
        }

        public List<DocumentPersonVM> SelectDocumentPersonsFromCase(DocumentSelectPersonsVM model, int index)
        {
            var caseModel = repo.AllReadonly<Case>()
                                        .Include(x => x.CaseType)
                                        .Include(x => x.CasePersons)
                                        .ThenInclude(x => x.Addresses)
                                        .ThenInclude(x => x.Address)
                                        .ThenInclude(x => x.AddressType)
                                        .Include(x => x.CasePersons)
                                        .ThenInclude(x => x.PersonRole)
                                        .Where(x => x.Id == int.Parse(model.SourceId))
                                        .FirstOrDefault();

            var result = new List<DocumentPersonVM>();
            foreach (var person in caseModel.CasePersons.Where(x => (x.CaseSessionId ?? -1) == -1)
                                                        .Where(x => (x.DateTo ?? DateTime.Now.AddDays(1)) >= DateTime.Now))
            {
                var searchPerson = model.Persons.Where(x => x.Id == person.Id.ToString()).FirstOrDefault();
                if (searchPerson != null)
                {
                    var docPerson = new DocumentPersonVM()
                    {
                        Index = index++
                    };
                    docPerson.CopyFrom(person);
                    docPerson.PersonRoleId = person.PersonRoleId;
                    docPerson.MilitaryRangId = person.MilitaryRangId;
                    docPerson.PersonMaturityId = person.PersonMaturityId;
                    foreach (var pAdr in person.Addresses)
                    {
                        var searchAdr = searchPerson.Addresses.FirstOrDefault(sp => sp.Id == pAdr.AddressId);
                        var docPersonAddress = new DocumentPersonAddressVM()
                        {
                            PersonIndex = docPerson.Index,
                            Index = docPerson.Addresses.Count
                        };
                        docPersonAddress.Address.CopyFrom(pAdr.Address);
                        docPerson.Addresses.Add(docPersonAddress);
                    }
                    result.Add(docPerson);
                }
            }
            return result;
        }

        public IQueryable<DocumentSelectAddressVM> SelectAddressListByPerson(string uic, int uicTypeId, int? personSourceType,
                        long? personSourceId)
        {
            IQueryable<DocumentSelectAddressVM> result = null;
            if ((personSourceType ?? 0) > 0)
            {

                switch (personSourceType)
                {
                    case SourceTypeSelectVM.Court:
                        result = repo.AllReadonly<Court>()
                                        .Include(x => x.CourtAddress)
                                        .ThenInclude(x => x.AddressType)
                                        .Where(x => x.Id == personSourceId)
                                        .Where(x => x.CourtAddress.CityCode != null)
                                        .Select(x => new DocumentSelectAddressVM
                                        {
                                            Id = x.CourtAddress.Id,
                                            AddressTypeName = x.CourtAddress.AddressType.Label,
                                            FullAddress = x.CourtAddress.FullAddress
                                        });
                        break;
                    case SourceTypeSelectVM.Instutution:
                        result = repo.AllReadonly<InstitutionAddress>()
                                       .Include(x => x.Address)
                                       .ThenInclude(x => x.AddressType)
                                       .Where(x => x.InstitutionId == personSourceId)
                                       .Where(x => x.Address.CityCode != null)
                                       .Select(x => new DocumentSelectAddressVM
                                       {
                                           Id = x.Address.Id,
                                           AddressTypeName = x.Address.AddressType.Label,
                                           FullAddress = x.Address.FullAddress
                                       });
                        break;
                    case SourceTypeSelectVM.LawUnit:
                        result = repo.AllReadonly<LawUnitAddress>()
                                           .Include(x => x.LawUnit)
                                           .Include(x => x.Address)
                                           .ThenInclude(x => x.AddressType)
                                           .Where(x => x.LawUnitId == personSourceId)
                                           .Where(x => x.Address.CityCode != null)
                                           .Select(x => new DocumentSelectAddressVM
                                           {
                                               Id = x.Address.Id,
                                               AddressTypeName = x.Address.AddressType.Label,
                                               FullAddress = x.Address.FullAddress
                                           });
                        break;
                    default:
                        break;
                }


            }
            else
            {
                if (string.IsNullOrEmpty(uic) == false)
                {
                    var documentAddresses = repo.AllReadonly<DocumentPersonAddress>()
                                            .Include(x => x.DocumentPerson)
                                            .Include(x => x.Address)
                                            .ThenInclude(x => x.AddressType)
                                            .Where(x => x.DocumentPerson.Uic == uic && x.DocumentPerson.UicTypeId == uicTypeId)
                                            .Where(x => x.Address.CityCode != null)
                                            .OrderByDescending(x => x.Id)
                                            .Select(x => new DocumentSelectAddressVM
                                            {
                                                Id = x.Address.Id,
                                                AddressTypeName = x.Address.AddressType.Label,
                                                FullAddress = x.Address.FullAddress
                                            }).Take(50);

                    var caseAddresses = repo.AllReadonly<CasePersonAddress>()
                                            .Include(x => x.CasePerson)
                                            .Include(x => x.Address)
                                            .ThenInclude(x => x.AddressType)
                                            .Where(x => x.CasePerson.Uic == uic && x.CasePerson.UicTypeId == uicTypeId)
                                            .Where(x => x.Address.CityCode != null)
                                            .OrderByDescending(x => x.Id)
                                            .Select(x => new DocumentSelectAddressVM
                                            {
                                                Id = x.Address.Id,
                                                AddressTypeName = x.Address.AddressType.Label,
                                                FullAddress = x.Address.FullAddress
                                            }).Take(50);

                    var lawUnitAddresses = repo.AllReadonly<LawUnitAddress>()
                                            .Include(x => x.LawUnit)
                                            .Include(x => x.Address)
                                            .ThenInclude(x => x.AddressType)
                                            .Where(x => x.LawUnit.Uic == uic && x.LawUnit.UicTypeId == uicTypeId)
                                            .Where(x => x.Address.CityCode != null)
                                            .OrderByDescending(x => x.AddressId)
                                            .Select(x => new DocumentSelectAddressVM
                                            {
                                                Id = x.Address.Id,
                                                AddressTypeName = x.Address.AddressType.Label,
                                                FullAddress = x.Address.FullAddress
                                            }).Take(50);

                    result = documentAddresses.Union(caseAddresses).Union(lawUnitAddresses)
                                        .OrderByDescending(x => x.Id)
                                        .GroupBy(x => new { x.AddressTypeName, x.FullAddress })
                                        .Select(g => g.FirstOrDefault());
                }
            }

            if (result == null)
                result = Enumerable.Empty<DocumentSelectAddressVM>().AsQueryable();

            return result;
        }

        public (bool result, string errorMessage) DocumentDecision_SaveData(DocumentDecision model)
        {
            try
            {
                model.DecisionTypeId = model.DecisionTypeId.EmptyToNull();
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<DocumentDecision>(model.Id);
                    saved.DecisionTypeId = model.DecisionTypeId;
                    saved.Description = model.Description;
                    saved.DocumentDecisionStateId = model.DocumentDecisionStateId;
                    saved.UserId = userContext.UserId;
                    saved.DateWrt = DateTime.Now;

                    if (string.IsNullOrEmpty(saved.RegNumber) && saved.DocumentDecisionStateId != NomenclatureConstants.DocumentDecisionStates.Draft)
                    {
                        if (counterService.Counter_GetDocumentDecisionCounter(saved) == false)
                        {
                            return (result: false, errorMessage: "Проблем при вземане на номер");
                        }
                    }

                    repo.Update(saved);
                }
                else
                {
                    if (model.DocumentDecisionStateId != NomenclatureConstants.DocumentDecisionStates.Draft)
                    {
                        if (counterService.Counter_GetDocumentDecisionCounter(model) == false)
                        {
                            return (result: false, errorMessage: "Проблем при вземане на номер");
                        }
                    }

                    model.UserDecisionId = userContext.UserId;
                    model.UserId = userContext.UserId;
                    model.DateWrt = DateTime.Now;
                    repo.Add<DocumentDecision>(model);
                }

                //Ако решението е Решено да се приключи задачата
                if (model.DocumentDecisionStateId == NomenclatureConstants.DocumentDecisionStates.Resolution)
                {
                    DocumentDecision_SaveData_FinishTask(model.DocumentId);
                }

                repo.SaveChanges();
                return (result: true, errorMessage: "");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на DocumentDecision Id={ model.Id }");
                return (result: false, errorMessage: Helper.GlobalConstants.MessageConstant.Values.SaveFailed);
            }
        }

        public DocumentDecision DocumentDecision_SelectForDocument(long documentId)
        {
            return repo.AllReadonly<DocumentDecision>()
                                        .Where(x => x.DocumentId == documentId)
                                        .FirstOrDefault();
        }

        public IQueryable<DocumentDecisionListVM> DocumentDecision_Select(int courtId, DocumentDecisionFilterVM model)
        {
            Expression<Func<DocumentDecision, bool>> yearSearch = x => true;
            if (model.DocumentYear > 0)
            {
                yearSearch = x => x.Document.DocumentDate.Year == model.DocumentYear;
            }
            Expression<Func<DocumentDecision, bool>> numberSearch = x => true;
            if (!string.IsNullOrWhiteSpace(model.DocumentNumber))
            {
                model.DocumentNumber = model.DocumentNumber.Trim();
                numberSearch = x => x.Document.DocumentNumber == model.DocumentNumber;
            }

            return repo.AllReadonly<DocumentDecision>()
                                .Where(x => x.CourtId == courtId)
                                .Where(x => x.DocumentDecisionStateId != NomenclatureConstants.DocumentDecisionStates.Draft)
                                .Where(x => (x.RegDate ?? model.DateFrom).Date >= model.DateFrom.Date && (x.RegDate ?? model.DateTo).Date <= model.DateTo.Date)
                                .Where(yearSearch)
                                .Where(numberSearch)
                                .Select(x => new DocumentDecisionListVM
                                {
                                    Id = x.Id,
                                    DocumentNumber = x.Document.DocumentNumber,
                                    DocumentDate = x.Document.DocumentDate,
                                    DecisionNumber = x.RegNumber,
                                    DecisionDate = x.RegDate,
                                    DecisionName = x.DecisionType.Label,
                                    DecisionUserName = x.UserDecision.LawUnit.FullName,
                                    DocumentTypeName = x.Document.DocumentType.Label,
                                    DocumentId = x.DocumentId
                                }).AsQueryable();
        }

        private void DocumentDecision_SaveData_FinishTask(long documentId)
        {
            var myRouteTasks = repo.All<WorkTask>(
                x => x.SourceId == documentId
                && x.SourceType == SourceTypeSelectVM.Document
                && x.UserId == userContext.UserId
                && x.TaskTypeId == WorkTaskConstants.Types.DocumentDecision
                && x.TaskStateId == WorkTaskConstants.States.Accepted);

            foreach (var item in myRouteTasks)
            {
                workTaskService.CompleteTask(item);
            }
        }

        public IQueryable<DocumentDecisionCaseListVM> DocumentDecisionCase_Select(long documentDecisionId)
        {
            return repo.AllReadonly<DocumentDecisionCase>()
                                .Where(x => x.DocumentDecisionId == documentDecisionId)
                                .Select(x => new DocumentDecisionCaseListVM
                                {
                                    Id = x.Id,
                                    CaseRegNumber = x.Case.RegNumber,
                                    CaseRegDate = x.Case.RegDate,
                                    DecisionName = x.DecisionType.Label
                                }).AsQueryable();
        }

        public (bool result, string errorMessage) DocumentDecisionCase_SaveData(DocumentDecisionCase model)
        {
            try
            {
                model.DecisionTypeId = model.DecisionTypeId.EmptyToNull();
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<DocumentDecisionCase>(model.Id);
                    saved.DecisionTypeId = model.DecisionTypeId;
                    saved.Description = model.Description;

                    repo.Update(saved);
                }
                else
                {
                    repo.Add<DocumentDecisionCase>(model);
                }

                repo.SaveChanges();
                return (result: true, errorMessage: "");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на DocumentDecisionCase Id={ model.Id }");
                return (result: false, errorMessage: Helper.GlobalConstants.MessageConstant.Values.SaveFailed);
            }
        }

        public DocumentSelectPersonsVM Document_SelectPersons(long documentId)
        {
            DocumentSelectPersonsVM model = new DocumentSelectPersonsVM();
            var documentModel = repo.AllReadonly<Document>()
                                        .Include(x => x.DocumentType)
                                        .Include(x => x.DocumentPersons)
                                        .ThenInclude(x => x.Addresses)
                                        .ThenInclude(x => x.Address)
                                        .ThenInclude(x => x.AddressType)
                                        .Include(x => x.DocumentPersons)
                                        .ThenInclude(x => x.PersonRole)
                                        .Where(x => x.Id == documentId)
                                        .FirstOrDefault();

            if (documentModel == null)
            {
                return null;
            }

            model.SourceType = SourceTypeSelectVM.Document;
            model.SourceId = documentId.ToString();
            model.SourceTypeName = $"Лица по документ {documentModel.DocumentType.Code} {documentModel.DocumentNumber} / {documentModel.DocumentDate:dd.MM.yyyy}";
            foreach (var item in documentModel.DocumentPersons)
            {
                var newPerson = new DocumentSelectPersonItemVM()
                {
                    Id = item.Id.ToString(),
                    IsChecked = true,
                    Uic = item.Uic,
                    UicTypeLabel = item.UicTypeLabel,
                    FullName = item.FullName,
                    RoleName = item.PersonRole.Label
                };
                foreach (var adr in item.Addresses)
                {
                    var newAdr = new DocumentSelectAddressVM()
                    {
                        Id = adr.AddressId,
                        IsChecked = true,
                        AddressTypeName = adr.Address.AddressType.Label,
                        FullAddress = adr.Address.FullAddress,
                    };
                    newPerson.Addresses.Add(newAdr);
                }
                model.Persons.Add(newPerson);
            }

            return model;
        }

        public List<DocumentPersonVM> SelectDocumentPersonsFromDocument(DocumentSelectPersonsVM model, int index)
        {
            var documentModel = repo.AllReadonly<Document>()
                                        .Include(x => x.DocumentType)
                                        .Include(x => x.DocumentPersons)
                                        .ThenInclude(x => x.Addresses)
                                        .ThenInclude(x => x.Address)
                                        .ThenInclude(x => x.AddressType)
                                        .Include(x => x.DocumentPersons)
                                        .ThenInclude(x => x.PersonRole)
                                        .Where(x => x.Id == long.Parse(model.SourceId))
                                        .FirstOrDefault();

            var result = new List<DocumentPersonVM>();
            foreach (var person in documentModel.DocumentPersons)
            {
                var searchPerson = model.Persons.Where(x => x.Id == person.Id.ToString()).FirstOrDefault();
                if (searchPerson != null)
                {
                    var docPerson = new DocumentPersonVM()
                    {
                        Index = index++
                    };
                    docPerson.CopyFrom(person);
                    docPerson.PersonRoleId = person.PersonRoleId;
                    docPerson.MilitaryRangId = person.MilitaryRangId;
                    docPerson.PersonMaturityId = person.PersonMaturityId;
                    foreach (var pAdr in person.Addresses)
                    {
                        var searchAdr = searchPerson.Addresses.FirstOrDefault(sp => sp.Id == pAdr.AddressId);
                        var docPersonAddress = new DocumentPersonAddressVM()
                        {
                            PersonIndex = docPerson.Index,
                            Index = docPerson.Addresses.Count
                        };
                        docPersonAddress.Address.CopyFrom(pAdr.Address);
                        docPerson.Addresses.Add(docPersonAddress);
                    }
                    result.Add(docPerson);
                }
            }
            return result;
        }

        public IQueryable<DocumentCaseInfoSprVM> DocumentCaseInfoSpr_Select(DocumentCaseInfoSprFilterVM model)
        {
            model.DateFrom = NomenclatureExtensions.ForceStartDate(model.DateFrom);
            model.DateTo = NomenclatureExtensions.ForceEndDate(model.DateTo);

            var documentCaseInfos = repo.AllReadonly<DocumentCaseInfo>()
                                        .Include(x => x.Document)
                                        .ThenInclude(x => x.DocumentType)
                                        .Include(x => x.Document)
                                        .ThenInclude(x => x.DocumentGroup)
                                        .Include(x => x.Case)
                                        .ThenInclude(x => x.CaseGroup)
                                        .Include(x => x.Case)
                                        .ThenInclude(x => x.CaseType)
                                        .Include(x => x.Case)
                                        .ThenInclude(x => x.CaseCode)
                                        .Include(x => x.Case)
                                        .ThenInclude(x => x.Document)
                                        .ThenInclude(x => x.DocumentType)
                                        .Where(x => (x.CourtId == userContext.CourtId) &&
                                                    (x.Document.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.CompliantDocument) &&
                                                    (x.Document.DocumentDate >= model.DateFrom && x.Document.DocumentDate <= model.DateTo) &&
                                                    (model.DocumentGroupId > 0 ? x.Document.DocumentGroupId == model.DocumentGroupId : true) &&
                                                    (model.DocumentTypeId > 0 ? x.Document.DocumentTypeId == model.DocumentTypeId : true) &&
                                                    (model.CaseGroupId > 0 ? x.Case.CaseGroupId == model.CaseGroupId : true) &&
                                                    (model.CaseTypeId > 0 ? x.Case.CaseTypeId == model.CaseTypeId : true) &&
                                                    (model.CaseCodeId > 0 ? x.Case.CaseCodeId == model.CaseCodeId : true))
                                        .ToList();

            var caseInfoSprVMs = new List<DocumentCaseInfoSprVM>();

            foreach (var document in documentCaseInfos)
            {
                var doc = repo.AllReadonly<CaseSessionDoc>()
                              .Include(d => d.CaseSession)
                              .ThenInclude(d => d.SessionType)
                              .Where(d => d.DocumentId == document.DocumentId &&
                                          d.DateExpired == null)
                              .FirstOrDefault();

                var caseInfoSprVM = new DocumentCaseInfoSprVM
                {
                    DocumentNumberYear = document.Document.DocumentNumber + "/" + document.Document.DocumentDate.Date.Year + "г.",
                    DocumentDate = document.Document.DocumentDate,
                    DocumentTypeLabel = document.Document.DocumentType.Label,
                    CaseInfo = (document.Case != null) ? document.Case.CaseType.Code + " " + document.Case.RegNumber : string.Empty,
                    CaseId = (document.Case != null) ? document.Case.Id : (int?)null,
                    IsCase = (document.Case != null),
                    CaseCodeLabel = (document.Case != null) ? document.Case.CaseCode.Code + " " + document.Case.CaseCode.Label : string.Empty,
                    CaseDocumentInfo = (document.Case != null) ? document.Case.Document.DocumentType.Label + " " + document.Case.Document.DocumentNumber + "/" + document.Case.Document.DocumentDate.Date.Year + "г." : string.Empty,
                    CaseSessionInfo = (doc != null) ? ((doc.CaseSession != null) ? (doc.CaseSession.SessionType.Label + " " + doc.CaseSession.DateFrom.ToString("dd.MM.yyyy")) : string.Empty) : string.Empty
                };

                caseInfoSprVMs.Add(caseInfoSprVM);
            };

            var caseSessionFastDocuments = repo.AllReadonly<CaseSessionFastDocument>()
                                               .Include(x => x.SessionDocType)
                                               .Include(x => x.CaseSession)
                                               .ThenInclude(x => x.SessionType)
                                               .Include(x => x.Case)
                                               .ThenInclude(x => x.CaseGroup)
                                               .Include(x => x.Case)
                                               .ThenInclude(x => x.CaseType)
                                               .Include(x => x.Case)
                                               .ThenInclude(x => x.CaseCode)
                                               .Where(x => (x.CourtId == userContext.CourtId) &&
                                                           (x.CaseSession.DateFrom >= model.DateFrom && x.CaseSession.DateFrom <= model.DateTo) &&
                                                           (model.CaseGroupId > 0 ? x.Case.CaseGroupId == model.CaseGroupId : true) &&
                                                           (model.CaseTypeId > 0 ? x.Case.CaseTypeId == model.CaseTypeId : true) &&
                                                           (model.CaseCodeId > 0 ? x.Case.CaseCodeId == model.CaseCodeId : true))
                                               .Select(x => new DocumentCaseInfoSprVM
                                               {
                                                   DocumentNumberYear = x.CaseSession.DateFrom.Year.ToString() + "г.",
                                                   DocumentDate = x.CaseSession.DateFrom,
                                                   DocumentTypeLabel = x.SessionDocType.Label,
                                                   CaseId = x.Case.Id,
                                                   IsCase = true,
                                                   CaseInfo = x.Case.CaseType.Code + " " + x.Case.RegNumber,
                                                   CaseCodeLabel = x.Case.CaseCode.Code + " " + x.Case.CaseCode.Label,
                                                   CaseDocumentInfo = x.Case.Document.DocumentType.Label + " " + x.Case.Document.DocumentNumber + "/" + x.Case.Document.DocumentDate.Date.Year + "г.",
                                                   CaseSessionInfo = x.CaseSession.SessionType.Label + " " + x.CaseSession.DateFrom.ToString("dd.MM.yyyy")
                                               }).ToList();

            caseInfoSprVMs.AddRange(caseSessionFastDocuments);
            return caseInfoSprVMs.AsQueryable();
        }

        public Document GetByIdWithData(long id)
        {
            return repo.AllReadonly<Document>()
                    .Include(x => x.DocumentGroup)
                    .Where(x => x.Id == id)
                    .FirstOrDefault();
        }

        public bool IsCanExpireCompliantDocument(long id)
        {
            var document = repo.AllReadonly<Document>()
                               .Include(x => x.DocumentGroup)
                               .Where(x => x.Id == id)
                               .FirstOrDefault();

            if (document.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.CompliantDocument)
            {
                if (caseSessionDocService.IsExistDocumentIdDifferentStatusNerazgledan(id))
                    return false;

                if (caseSessionActComplainService.IsExistComplainByDocumentIdDifferentStatusRecived(id))
                    return false;

                if (caseMigrationService.IsExistMigrationWithComplainWithDocumentId(id))
                    return false;

                return true;
            }
            else
                return true;
        }

        public bool DocumentExpire(ExpiredInfoVM model)
        {
            try
            {
                var saved = repo.GetById<Document>(model.LongId);

                if (saved != null)
                {
                    saved.DateExpired = DateTime.Now;
                    saved.UserExpiredId = userContext.UserId;
                    saved.DescriptionExpired = model.DescriptionExpired;
                    repo.Update(saved);

                    var docTasks = repo.All<WorkTask>()
                                    .Where(x => x.SourceType == SourceTypeSelectVM.Document && x.SourceId == model.LongId)
                                    .ToList();
                    if (docTasks.Any())
                    {
                        foreach (var task in docTasks)
                        {
                            task.TaskStateId = WorkTaskConstants.States.Deleted;
                        }
                    }

                    var documentGroup = repo.GetById<DocumentGroup>(saved.DocumentGroupId);
                    if (documentGroup.DocumentKindId == DocumentConstants.DocumentKind.CompliantDocument)
                    {
                        var caseSessionDocs = repo.AllReadonly<CaseSessionDoc>()
                                                  .Where(x => x.DocumentId == model.LongId &&
                                                              x.DateExpired == null)
                                                  .ToList() ?? new List<CaseSessionDoc>();

                        foreach (var caseSessionDoc in caseSessionDocs)
                        {
                            caseSessionDoc.DateExpired = DateTime.Now;
                            caseSessionDoc.UserExpiredId = userContext.UserId;
                            caseSessionDoc.DescriptionExpired = model.DescriptionExpired;
                            repo.Update(caseSessionDoc);
                        }

                        var caseSessionActComplains = repo.AllReadonly<CaseSessionActComplain>()
                                                          .Where(x => x.ComplainDocumentId == model.LongId &&
                                                                      x.DateExpired == null)
                                                          .ToList() ?? new List<CaseSessionActComplain>();

                        foreach (var caseSessionActComplain in caseSessionActComplains)
                        {
                            caseSessionActComplain.DateExpired = DateTime.Now;
                            caseSessionActComplain.UserExpiredId = userContext.UserId;
                            caseSessionActComplain.DescriptionExpired = model.DescriptionExpired;
                            repo.Update(caseSessionActComplain);
                        }
                    }

                    //Ако документа е към темплейт се освобождава и от там
                    var docTemplate = repo.All<DocumentTemplate>().Where(x => x.DocumentId == model.LongId).FirstOrDefault();
                    if (docTemplate != null)
                    {
                        docTemplate.DocumentId = null;
                        repo.Update(docTemplate);
                    }

                    repo.SaveChanges();
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при премахване на документ Id={ model.LongId }");
                return false;
            }
        }

        /// <summary>
        /// Връща списък на всички деловодни регистратури, до които служителя има достъп
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> GetDocumentRegistratures(bool appendallItem = false)
        {
            int[] userDocRegs = userContext.SubDocRegistry;

            if (userDocRegs == null || userDocRegs.Count() == 0)
            {
                return null;
            }
            var userOrganizationId = userDocRegs[0];
            var isPowerUser = userContext.IsUserInRole(AccountConstants.Roles.PowerUser);

            Expression<Func<CourtOrganization, bool>> docRegSearch = x => true;
            if (!isPowerUser)
            {
                //Ако не е POWER_USER - само нивото на служителя
                docRegSearch = x => x.Id == userOrganizationId;
            }
            else
            {
                //Ако е POWER_USER - нивото и всички подчинени регистратури
                docRegSearch = x => userDocRegs.Contains(x.Id);
            }

            var result = repo.AllReadonly<CourtOrganization>()
                            .Where(x => x.CourtId == userContext.CourtId)
                            .Where(x => x.DateFrom <= DateTime.Now && (x.DateTo ?? DateTime.MaxValue) >= DateTime.Now)
                            .Where(docRegSearch)
                            .ToSelectList(x => x.Id, x => x.Label);

            if (appendallItem)
            {
                result = result.Prepend(new SelectListItem("Избери", "-1")).ToList();
            }
            return result;
        }

        public bool Reactivate(DocumentReactivateVM model)
        {
            if (model.Id == 0)
            {
                var info = repo.AllReadonly<Document>()
                                    .Include(x => x.DocumentGroup)
                                    .Include(x => x.DocumentType)
                                    .Where(x => x.CourtId == userContext.CourtId)
                                    .Where(x => x.DocumentDirectionId == model.DocumentDirectionId)
                                    .Where(x => x.DocumentNumber == model.DocumentNumber && x.DocumentDate.Date == model.DocumentDate.Date)
                                    .Where(x => x.DateExpired != null)
                                    .Select(x => new
                                    {
                                        Id = x.Id,
                                        Info = $"{x.DocumentGroup.Label}\\{x.DocumentType.Label} {model.DocumentNumber} от {model.DocumentDate:dd.MM.yyyy}"
                                    }).FirstOrDefault();

                if (info != null)
                {
                    model.Id = info.Id;
                    model.DocumentInfo = info.Info;
                    model.IsFound = true;
                    model.IsActivated = false;
                    return true;
                }
                else
                {
                    model.FindMessage = "Няма намерен премахнат документ с подадения номер и дата";
                }
            }
            else
            {
                var expired = repo.GetById<Document>(model.Id);
                if (expired != null)
                {
                    expired.DateExpired = null;
                    expired.UserExpiredId = null;
                    expired.DescriptionExpired = null;
                    repo.SaveChanges();

                    model.IsActivated = true;
                    return true;
                }
            }

            return false;
        }

        public IQueryable<DocumentInstitutionCaseInfoListVM> DocumentInstitutionCaseInfo_Select(long documentId)
        {
            return repo.AllReadonly<DocumentInstitutionCaseInfo>()
                       .Where(x => x.DocumentId == documentId)
                       .Select(x => new DocumentInstitutionCaseInfoListVM()
                       {
                           Id = x.Id,
                           InstitutionLabel = x.Institution.FullName,
                           InstitutionCaseTypeLabel = x.InstitutionCaseType.Label,
                           CaseNumber = x.CaseNumber,
                           CaseYear = x.CaseYear
                       })
                       .AsQueryable();
        }

        public bool DocumentInstitutionCaseInfo_SaveData(DocumentInstitutionCaseInfoEditVM model)
        {
            try
            {
                DocumentInstitutionCaseInfo saveModel = new DocumentInstitutionCaseInfo();
                model.ToEntity(saveModel);

                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<DocumentInstitutionCaseInfo>(saveModel.Id);
                    saved.InstitutionId = saveModel.InstitutionId;
                    saved.InstitutionCaseTypeId = saveModel.InstitutionCaseTypeId;
                    saved.CaseNumber = saveModel.CaseNumber;
                    saved.CaseYear = saveModel.CaseYear;
                    saved.Description = saveModel.Description;

                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    //Insert
                    repo.Add<DocumentInstitutionCaseInfo>(saveModel);
                    repo.SaveChanges();
                }
                model.Id = saveModel.Id;
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на интервал по дело Id={ model.Id }");
                return false;
            }
        }

        public DocumentInstitutionCaseInfoEditVM GetById_InstitutionCaseInfoEditVM(long Id)
        {
            var result = repo.AllReadonly<DocumentInstitutionCaseInfo>()
                             .Where(x => x.Id == Id)
                             .Select(x => new DocumentInstitutionCaseInfoEditVM()
                             {
                                 Id = x.Id,
                                 DocumentId = x.DocumentId,
                                 InstitutionTypeId = x.Institution.InstitutionTypeId,
                                 InstitutionId = x.InstitutionId,
                                 CaseNumber = x.CaseNumber,
                                 CaseYear = x.CaseYear,
                                 Description = x.Description,
                                 InstitutionCaseTypeId = x.InstitutionCaseTypeId
                             })
                             .FirstOrDefault();

            var caseCase = repo.AllReadonly<Case>()
                               .Where(x => x.DocumentId == result.DocumentId)
                               .FirstOrDefault();
            result.CaseId = caseCase.Id;

            return result;
        }

        public bool Document_CorrectData(DocumentVM model)
        {
            var saved = repo.GetById<Document>(model.Id);
            if (saved != null)
            {
                saved.DeliveryGroupId = model.DeliveryGroupId;
                saved.Description = model.Description;
                saved.IsRestictedAccess = model.IsRestictedAccess;
                saved.IsSecret = model.IsSecret;
                repo.Update(saved);
                repo.SaveChanges();
                return true;
            }

            return false;
        }
    }
}
