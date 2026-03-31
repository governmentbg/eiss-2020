using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Extensions;
using IOWebApplication.Core.Helper;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Money;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Data.Models.Regix;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Models.Documents;
using IOWebApplication.Infrastructure.Models.Integrations.EpepFastProcess;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Documents;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Services
{
    public class DocumentService : BaseService, IDocumentService
    {
        private readonly ICounterService counterService;
        private readonly INomenclatureService nomenclatureService;
        private readonly ICaseClassificationService caseClassificationService;
        private readonly IWorkTaskService workTaskService;
        private readonly IMQEpepService epepService;
        private readonly ICaseDeadlineService deadlineService;
        private readonly ICaseSessionActComplainService caseSessionActComplainService;
        private readonly ICaseSessionDocService caseSessionDocService;
        private readonly ICaseMigrationService caseMigrationService;
        private readonly ITransactionService transactionService;
        private readonly IWorkNotificationService workNotificationService;
        private readonly ICdnService cdnService;

        public DocumentService(ILogger<DocumentService> _logger,
                               ICounterService _counterService,
                               INomenclatureService _nomenclatureService,
                               ICaseClassificationService _caseClassificationService,
                               IWorkTaskService _workTaskService,
                               IMQEpepService _epepService,
                               ICaseDeadlineService _deadlineService,
                               ICaseSessionActComplainService _caseSessionActComplainService,
                               ICaseSessionDocService _caseSessionDocService,
                               ICaseMigrationService _caseMigrationService,
                               ITransactionService _transactionService,
                               IRepository _repo,
                               IUserContext _userContext,
                               IWorkNotificationService _workNotificationService,
                               ICdnService _cdnService)
        {
            logger = _logger;
            counterService = _counterService;
            nomenclatureService = _nomenclatureService;
            caseClassificationService = _caseClassificationService;
            workTaskService = _workTaskService;
            epepService = _epepService;
            deadlineService = _deadlineService;
            caseSessionActComplainService = _caseSessionActComplainService;
            repo = _repo;
            userContext = _userContext;
            caseSessionDocService = _caseSessionDocService;
            caseMigrationService = _caseMigrationService;
            transactionService = _transactionService;
            workNotificationService = _workNotificationService;
            cdnService = _cdnService;
        }

        /// <summary>
        /// Метод извличащ данни за регистрирани документи
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<DocumentListVM> Document_Select(DocumentFilterVM filter)
        {
            filter.NormalizeValues();

            Expression<Func<Document, bool>> whereDocumentDirection = x => true;
            if (filter.DocumentDirectionId > 0)
                whereDocumentDirection = x => x.DocumentDirectionId == filter.DocumentDirectionId.Value;

            Expression<Func<Document, bool>> whereDocumentKind = x => true;
            if (filter.DocumentKindId > 0)
                whereDocumentKind = x => x.DocumentType.DocumentGroup.DocumentKindId == filter.DocumentKindId.Value;

            Expression<Func<Document, bool>> whereDocumentGroup = x => true;
            if (filter.DocumentGroupId > 0)
                whereDocumentGroup = x => x.DocumentGroupId == filter.DocumentGroupId.Value;

            Expression<Func<Document, bool>> whereDocumentType = x => true;
            if (filter.DocumentTypeId > 0)
                whereDocumentType = x => x.DocumentTypeId == filter.DocumentTypeId.Value;

            Expression<Func<Document, bool>> whereDocumentDateFrom = x => true;
            if (filter.DateFrom.HasValue)
                whereDocumentDateFrom = x => x.DocumentDate >= filter.DateFrom.Value;

            Expression<Func<Document, bool>> whereDocumentDateTo = x => true;
            if (filter.DateTo.HasValue)
                whereDocumentDateTo = x => x.DocumentDate <= filter.DateTo.Value;

            Expression<Func<Document, bool>> yearSearch = x => true;
            if (filter.DocumentYear > 0)
            {
                DateTime documentDateFrom = new DateTime(filter.DocumentYear.Value, 1, 1);
                DateTime documentDateTo = documentDateFrom.AddYears(1);
                yearSearch = x => x.DocumentDate >= documentDateFrom && x.DocumentDate < documentDateTo;
            }

            Expression<Func<Document, bool>> numberSearch = x => true;
            if (!string.IsNullOrWhiteSpace(filter.DocumentNumber))
            {
                filter.DocumentNumber = filter.DocumentNumber.Trim();
                numberSearch = x => x.DocumentNumber == filter.DocumentNumber;
            }

            Expression<Func<Document, bool>> personSearch = x => true;
            if (!string.IsNullOrEmpty(filter.PersonName) || filter.PersonRoleId > 0)
                personSearch = x => x.DocumentPersons.Any(p => EF.Functions.ILike(p.FullName, filter.PersonName.ToPaternSearch()) &&
                                                               (p.PersonRoleId == (filter.PersonRoleId ?? p.PersonRoleId)));

            Expression<Func<Document, bool>> personUicSearch = x => true;
            if (!string.IsNullOrEmpty(filter.PersonUIC))
                personUicSearch = x => x.DocumentPersons.Any(p => p.Uic == filter.PersonUIC);

            Expression<Func<Document, bool>> courtOrgSearch = x => true;
            if (filter.CourtOrganizationId > 0)
                courtOrgSearch = x => x.CourtOrganizationId == filter.CourtOrganizationId;

            //СВЪРЗАНИ ДЕЛА
            Expression<Func<Document, bool>> linkCaseCourtWhere = x => true;
            if (filter.LinkDelo_CourtId > 0)
                linkCaseCourtWhere = x => x.DocumentCaseInfo.Any(a => a.Case.CourtId == filter.LinkDelo_CourtId);

            Expression<Func<Document, bool>> linkCaseIdWhere = x => true;
            if (filter.LinkDelo_CaseId > 0)
                linkCaseIdWhere = x => x.DocumentCaseInfo.Any(a => a.CaseId == filter.LinkDelo_CaseId);

            Expression<Func<Document, bool>> linkDescriptionWhere = x => true;
            if (!string.IsNullOrEmpty(filter.LinkDelo_Description))
            {
                var stringFind = filter.LinkDelo_Description.ToUpper();
                linkDescriptionWhere = x => x.DocumentCaseInfo.Any(a => EF.Functions.ILike(a.Description, filter.LinkDelo_Description.ToPaternSearch()));
            }

            Expression<Func<Document, bool>> regNumOtherSystem = x => true;
            if (!string.IsNullOrEmpty(filter.RegNumberOtherSystem))
                regNumOtherSystem = x => x.DocumentCaseInfo.Any(a => (EF.Functions.ILike(a.CaseRegNumber, filter.RegNumberOtherSystem.ToCasePaternSearch())) && (a.IsLegacyCase ?? false));

            Expression<Func<Document, bool>> yearOtherSystem = x => true;
            if (filter.YearOtherSystem > 0)
                yearOtherSystem = x => x.DocumentCaseInfo.Any(a => (a.CaseYear == filter.YearOtherSystem) && (a.IsLegacyCase ?? false));

            Expression<Func<Document, bool>> courtOtherSystem = x => true;
            if (filter.CourtOtherSystem > 0)
                courtOtherSystem = x => x.DocumentCaseInfo.Any(a => (a.CourtId == filter.CourtOtherSystem) && (a.IsLegacyCase ?? false));

            List<long> documentIds = new List<long>();
            Expression<Func<Document, bool>> caseRegNumberWhere = x => true;
            if (string.IsNullOrEmpty(filter.CaseRegNumber) == false)
            {
                documentIds.AddRange(repo.AllReadonly<Case>()
                     .Where(x => x.RegNumber != null)
                     .Where(x => EF.Functions.ILike(x.RegNumber, filter.CaseRegNumber.ToCasePaternSearch()))
                     .Select(x => x.DocumentId));

                documentIds.AddRange(repo.AllReadonly<DocumentCaseInfo>()
                     .Where(x => x.CaseId != null)
                     .Where(x => EF.Functions.ILike(x.Case.RegNumber, filter.CaseRegNumber.ToCasePaternSearch()))
                     .Select(x => x.DocumentId));

                caseRegNumberWhere = x => documentIds.Contains(x.Id);
            }

            Expression<Func<Document, bool>> caseEisppNumberWhere = x => true;
            if (!string.IsNullOrEmpty(filter.CaseEisppNumber))
                caseEisppNumberWhere = x => x.Cases.Any(c => EF.Functions.ILike(c.EISSPNumber, filter.CaseEisppNumber.ToPaternSearch()));

            Expression<Func<Document, bool>> descriptionWhere = x => true;
            if (!string.IsNullOrEmpty(filter.Description))
                descriptionWhere = x => EF.Functions.ILike(x.Description, filter.Description.ToPaternSearch());

            Expression<Func<Document, bool>> instCaseWhere = x => true;
            if (filter.InstitutionId > 0 || filter.InstitutionCaseYear > 0 || !string.IsNullOrEmpty(filter.InstitutionCaseNumber))
            {
                instCaseWhere = x => x.DocumentInstitutionCaseInfo.Any(i => i.Institution.InstitutionTypeId == (filter.InstitutionTypeId ?? i.Institution.InstitutionTypeId) &&
                                                                            i.InstitutionId == (filter.InstitutionId ?? i.InstitutionId) &&
                                                                            EF.Functions.ILike(i.CaseNumber, filter.InstitutionCaseNumber.ToPaternSearch()) &&
                                                                            i.CaseYear == (filter.InstitutionCaseYear ?? i.CaseYear));
            }

            Expression<Func<Document, bool>> whereDeliveryGroupInputId = x => true;
            if (filter.DocumentDirectionId == DocumentConstants.DocumentDirection.Incoming && filter.DeliveryGroupInputId > -1)
                whereDeliveryGroupInputId = x => x.DeliveryGroupId == filter.DeliveryGroupInputId;

            Expression<Func<Document, bool>> whereDeliveryGroupOutputId = x => true;
            if (filter.DocumentDirectionId == DocumentConstants.DocumentDirection.OutGoing && filter.DeliveryGroupOutputId > -1)
                whereDeliveryGroupOutputId = x => x.DeliveryGroupId == filter.DeliveryGroupOutputId;

            Expression<Func<Document, bool>> whereCourtId = x => x.CourtId == userContext.CourtId;
            if (filter.GlobalAssignmentRegister)
            {
                whereCourtId = x => x.CourtId == NomenclatureConstants.Courts.RandomAssignment;
            }

            Expression<Func<Document, bool>> whereEpepNumber = x => true;
            if (!string.IsNullOrEmpty(filter.EpepNumber))
            {
                whereEpepNumber = x => x.ElectronicDocument.ApplyNumber == filter.EpepNumber;
            }


            Expression<Func<Document, DocumentListVM>> selectExpression = x => new DocumentListVM
            {
                Id = x.Id,
                DocumentTypeName = x.DocumentType.Label,
                DocumentRequestName = (x.DocumentRequestTypeId > 0) ? x.DocumentRequestType.Label : "",
                DocumentDirectionName = x.DocumentDirection.Code,
                DocumentNumber = x.DocumentNumber,
                DocumentDate = x.DocumentDate,
                UserName = (x.RegisterUserId != null) ? x.RegisterUser.LawUnit.FullName : ((x.UserId != null) ? x.User.LawUnit.FullName : ""),
                Persons = x.DocumentPersons.Select(p => new DocumentListPersonVM
                {
                    Id = p.Id,
                    Uic = p.Uic,
                    Name = p.FullName,
                    RoleName = p.PersonRole.Label
                }).ToArray(),
                CaseId = (x.Cases.Any()) ? x.Cases.Select(c => c.Id).FirstOrDefault() : x.DocumentCaseInfo.Select(dt => dt.CaseId ?? 0).FirstOrDefault(),
                CaseNumber = (x.Cases.Any()) ? x.Cases.Select(c => c.RegNumber).FirstOrDefault() : x.DocumentCaseInfo.Select(dt => (dt.Case != null) ? dt.Case.RegNumber : "").FirstOrDefault(),
                IsCaseRejected = x.Cases.Any() ? NomenclatureConstants.CaseState.UnregisteredManageble.Contains(x.Cases.Select(c => c.CaseStateId).FirstOrDefault()) : false,
                RejectedStateName = x.Cases.Select(c => c.CaseState.Label).FirstOrDefault(),
                DocumentNumberValue = x.DocumentNumberValue ?? 0,
                Description = x.Description
            };

            //----GlobalRegister

            Expression<Func<Document, bool>> whereCreatedCourt = x => true;
            Expression<Func<Document, bool>> whereAssignedInCourt = x => true;
            Expression<Func<Document, bool>> whereAssignedCaseNumber = x => true;
            Expression<Func<Document, bool>> whereDocumentRequestType = x => true;

            Expression<Func<Document, bool>> whereStartDocumentId = x => true;
            if (filter.GlobalAssignmentRegister)
            {
                long startCrDocumentId = 0;
                try
                {
                    startCrDocumentId = long.Parse(GetParamValue(NomenclatureConstants.SystemParamName.ZP_StartDocumentId, "1")) - 1;
                    if (startCrDocumentId > 0)
                    {
                        whereStartDocumentId = x => x.Id > startCrDocumentId;
                    }
                }
                catch (Exception ex)
                {

                }


                if (filter.CreatedCourtId > 0)
                {
                    whereCreatedCourt = x => x.CreatedCourtId == filter.CreatedCourtId.Value;
                }
                if (filter.AssignedInCourtId > 0)
                {
                    whereAssignedInCourt = x => x.AssignedDocuments.Where(d => d.CourtId == filter.AssignedInCourtId.Value).Any();
                }
                if (!string.IsNullOrEmpty(filter.AssignedCaseNumber))
                {
                    whereAssignedCaseNumber = x => x.AssignedDocuments.SelectMany(c => c.Cases).Where(c => EF.Functions.ILike(c.RegNumber, filter.AssignedCaseNumber.ToCasePaternSearch())).Any();
                }
                if (filter.DocumentRequestTypeId > 0)
                {
                    whereDocumentRequestType = x => x.DocumentRequestTypeId == filter.DocumentRequestTypeId.Value;
                }


                selectExpression = x => new DocumentListVM
                {
                    Id = x.Id,
                    DocumentTypeName = x.DocumentType.Label,
                    //DocumentRequestName = (x.DocumentRequestTypeId > 0) ? x.DocumentRequestType.Label : "",
                    //DocumentDirectionName = x.DocumentDirection.Code,
                    DocumentNumber = x.DocumentNumber,
                    DocumentDate = x.DocumentDate,
                    UserName = (x.RegisterUserId != null) ? x.RegisterUser.LawUnit.FullName : ((x.UserId != null) ? x.User.LawUnit.FullName : ""),
                    CreatedCourtName = (x.CreatedCourtId > 0) ? x.CreatedCourt.Label : x.DeliveryGroup.Label,
                    Persons = x.DocumentPersons.Select(p => new DocumentListPersonVM
                    {
                        //Id = p.Id,
                        //Uic = p.Uic,
                        Name = p.FullName,
                        RoleName = p.PersonRole.Label
                    }).ToArray(),
                    RejectedStateName = x.Cases.Select(c => c.CaseState.Label).FirstOrDefault(),
                    DocumentNumberValue = x.DocumentNumberValue ?? 0,
                    Description = x.Description,
                    CaseCodeName = x.Cases.Select(c => $"{c.CaseCode.Code} {c.CaseCode.Label}").FirstOrDefault(),
                    RegCases = x.AssignedDocuments.Where(a => a.Id > startCrDocumentId).SelectMany(d => d.Cases)
                                                        .Where(c => !NomenclatureConstants.CaseState.UnregisteredManageble.Contains(c.CaseStateId))
                                                        .Select(c => new DocumentCaseLinkVM
                                                        {
                                                            CaseId = c.Id,
                                                            CaseCourt = c.Court.Label,
                                                            CaseNumber = c.RegNumber
                                                        }).ToArray()
                };
            }

            return repo.AllReadonly<Document>()
                       .Where(whereStartDocumentId)
                       .Where(whereCourtId)
                       .Where(courtOrgSearch)
                       .Where(whereDocumentDirection)
                       .Where(whereDocumentKind)
                       .Where(whereDocumentGroup)
                       .Where(whereDocumentType)
                       .Where(yearSearch)
                       .Where(numberSearch)
                       .Where(personSearch)
                       .Where(personUicSearch)
                       .Where(whereDocumentDateFrom)
                       .Where(whereDocumentDateTo)
                       .Where(linkCaseCourtWhere)
                       .Where(linkCaseIdWhere)
                       .Where(linkDescriptionWhere)
                       .Where(regNumOtherSystem)
                       .Where(yearOtherSystem)
                       .Where(courtOtherSystem)
                       .Where(caseRegNumberWhere)
                       .Where(caseEisppNumberWhere)
                       .Where(descriptionWhere)
                       .Where(instCaseWhere)
                       .Where(whereDeliveryGroupInputId)
                       .Where(whereDeliveryGroupOutputId)

                       .Where(whereEpepNumber)
                       .Where(whereCreatedCourt)
                       .Where(whereAssignedInCourt)
                       .Where(whereAssignedCaseNumber)
                       .Where(whereDocumentRequestType)
                       .Where(FilterExpireInfo<Document>(false))
                       //.Where(x => x.DocumentDeclaredDate != null)
                       .Select(selectExpression);
        }

        public async Task<long> CheckForRegisteredDocumentByElectronicId(long electronicDocumentId)
        {
            var documentId = await repo.AllReadonly<Document>()
                                        .Where(x => x.ElectronicDocumentId == electronicDocumentId)
                                        .Select(x => x.Id)
                                        .FirstOrDefaultAsync().ConfigureAwait(false);


            if (documentId > 0)
            {
                var transResult = await transactionService.AppendTransaction(SourceTypeSelectVM.ElectronicDocument, electronicDocumentId, NomenclatureConstants.MainTransactionTypes.Finish)
                .ConfigureAwait(false);
            }

            return documentId;
        }

        public async Task<DocumentVM> Document_InitFromRequestCodeForAssignment(int requestTypeId)
        {
            var document = await Document_Init(DocumentConstants.DocumentDirection.Incoming);
            document.CourtId = NomenclatureConstants.Courts.RandomAssignment;
            await InitializeDocumentVMFromRequest(document, null, requestTypeId);
            return document;
        }

        public async Task<bool> InitializeDocumentVMFromRequest(DocumentVM documentModel, string requestCode, int id = 0)
        {
            Expression<Func<DocumentRequestType, bool>> whereFilter = x => x.RequestCode == requestCode;
            if (id > 0)
            {
                whereFilter = x => x.Id == id;
            }
            var requestInfo = repo.AllReadonly<DocumentRequestType>()
                                    .Where(whereFilter)
                                    .FirstOrDefault();
            if (requestInfo == null)
            {
                return false;
            }

            //int documentGroupId = 0;
            //int documentTypeId = 0;
            //int caseTypeId = 0;
            //int caseCodeId = 0;
            //switch (requestCode)
            //{
            //    case DocumentConstants.ElectronicDocumentRequestTypes.FastProcess410:

            //        documentGroupId = 3;//Заявление
            //        documentTypeId = 11;//Заявление за издаване заповед за изпълнение
            //        caseTypeId = 15;//Частно гражданско дело (Гражданско дело)
            //        caseCodeId = 170;//1101-1 Заявления по чл. 410 ГПК
            //        break;
            //    case DocumentConstants.ElectronicDocumentRequestTypes.FastProcess417:

            //        documentGroupId = 3;//Заявление
            //        documentTypeId = 11;//Заявление за издаване заповед за изпълнение
            //        caseTypeId = 15;//Частно гражданско дело (Гражданско дело)
            //        caseCodeId = 172;//1102-1 Заявления по чл. 417 ГПК
            //        break;
            //    default:
            //        //TODO: Какво ги правим такива?
            //        return false;
            //}

            documentModel.DocumentGroupId = requestInfo.DocumentGroupId;
            documentModel.DocumentTypeId = requestInfo.DocumentTypeId;
            documentModel.CaseTypeId = requestInfo.CaseTypeId;
            documentModel.CaseCodeId = requestInfo.CaseCodeId;
            documentModel.RequestTypeId = requestInfo.Id;
            documentModel.RequestTypeCode = requestInfo.RequestCode;

            if (requestInfo.RequestCode == FastProcessRequestVM.FastProcess417 && documentModel.ElectronicDocumentId > 0)
            {
                try
                {
                    var requestJson = await cdnService.LoadHtmlFileTemplate(new Infrastructure.Models.Cdn.CdnFileSelect() { SourceType = SourceTypeSelectVM.ElectronicDocumentRequest, SourceId = documentModel.ElectronicDocumentId.Value.ToString() });
                    FastProcessRequestVM requestData = JsonConvert.DeserializeObject<FastProcessRequestVM>(requestJson);
                    if (requestData.CompetencyBase417 != null && requestData.CompetencyBase417.ForCompetencyBase)
                    {
                        documentModel.CaseCodeId = NomenclatureConstants.CaseCode.FP417_t3610;
                        documentModel.RequestTypeId = DocumentConstants.ElectronicDocumentRequestTypes.IDs.FastProcess417a1t3610;
                    }
                }
                catch (Exception ex)
                {

                }
            }

            return true;
        }

        public async Task<DocumentVM> Document_Init(int documentDirection, int templateId = 0, long electronicDocumentId = 0)
        {
            var model = new DocumentVM()
            {
                DocumentDirectionId = documentDirection
            };

            //model.DocumentCaseInfo.CourtId = userContext.CourtId;
            model.ProcessPriorityId = DocumentConstants.ProcessPriority.Common;
            model.CaseClassifications = caseClassificationService.FillCheckListVMs(0, 0);
            if (templateId > 0)
            {
                await document_InitFromTemplate(model, templateId);
            }
            if (electronicDocumentId > 0)
            {

                await document_InitFromElectronicDocument(model, electronicDocumentId);
            }

            //Ако е изходящ документ начина на изпращане да е поща 
            if (documentDirection == DocumentConstants.DocumentDirection.OutGoing)
                model.DeliveryGroupId = DocumentConstants.DeliveryGroups.PostOffice;

            //Guid за Regix
            model.RegixRequestReason.RegixReasonGuid = Guid.NewGuid().ToString().ToLower();
            model.RegixRequestReason.RegixReasonDescription = "Регистрацията на документа не е завършена от потребител";

            //Тестово създаване на много лица
            //for (int i = 0; i < 1500; i++)
            //{
            //    var dp = new DocumentPersonVM()
            //    {
            //        UicTypeId = 1,
            //        FirstName = $"Лице {i + 1}",
            //        PersonRoleId = 1
            //    };
            //    dp.NewDynamicItem = false;
            //    model.DocumentPersons.Add(dp);
            //}

            return model;
        }

        private async Task document_InitFromOutDocumentMigration(DocumentVM model, long outDocumentId)
        {
            var docInfo = await repo.AllReadonly<Document>()
                                    .Where(x => x.Id == outDocumentId)
                                    .Select(x => new
                                    {
                                        x.Id,
                                        CourtId = x.DocumentCaseInfo.Select(c => c.CourtId).FirstOrDefault(),
                                        CaseId = x.DocumentCaseInfo.Select(c => c.CaseId).FirstOrDefault()
                                    }).FirstOrDefaultAsync();

            model.DocumentDirectionId = DocumentConstants.DocumentDirection.Incoming;
            model.HasCaseInfo = true;
            model.DocumentCaseInfo.CourtId = docInfo.CourtId;
            model.DocumentCaseInfo.CaseId = docInfo.CaseId;
            model.DocumentLinks.Add(new DocumentLinkVM()
            {
                CourtId = docInfo.CourtId,
                IsLegacyDocument = false,
                PrevDocumentId = docInfo.Id
            });

        }
        private async Task document_InitFromElectronicDocument(DocumentVM model, long electronicDocumentId)
        {
            var elDoc = await repo.AllReadonly<ElectronicDocument>()
                                .Include(x => x.Persons)
                                .ThenInclude(x => x.Addresses)
                                .ThenInclude(x => x.Address)
                                .Where(x => x.Id == electronicDocumentId)
                                .AsSplitQuery()
                                .FirstOrDefaultAsync();
            if (elDoc == null)
            {
                return;
            }
            model.CourtId = elDoc.CourtId;
            model.ElectronicDocumentId = electronicDocumentId;
            model.DocumentDirectionId = DocumentConstants.DocumentDirection.Incoming;
            model.DeliveryGroupId = DocumentConstants.DeliveryGroups.WebPortal;
            model.DocumentGroupId = elDoc.DocumentGroupId;
            model.DocumentKindId = repo.GetPropById<DocumentGroup, int>(x => x.Id == elDoc.DocumentGroupId, x => x.DocumentKindId);
            model.Description = elDoc.Description;
            if (elDoc.CaseId > 0)
            {
                var caseModel = await GetByIdAsync<Case>(elDoc.CaseId.Value);
                model.HasCaseInfo = true;
                model.DocumentCaseInfo.CourtId = caseModel.CourtId;
                model.DocumentCaseInfo.CaseId = caseModel.Id;
            }
            foreach (var person in elDoc.Persons)
            {
                var docPerson = new DocumentPersonVM()
                {
                    UicTypeId = person.UicTypeId,
                    Uic = person.Uic,
                    FirstName = person.FirstName,
                    MiddleName = person.MiddleName,
                    FamilyName = person.FamilyName,
                    FullName = person.FullName,
                    PersonRoleId = person.PersonRoleId,
                    PersonGid = person.PersonGid,
                    RepresentsGid = person.RepresentsGid
                };

                if (!string.IsNullOrEmpty(docPerson.Uic))
                {
                    var lawyer = await repo.AllReadonly<LawUnit>()
                                        .Where(x => x.Code == docPerson.Uic)
                                        .Where(x => x.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Lawyer)
                                        .Where(x => x.DateTo == null)
                                        .FirstOrDefaultAsync();
                    if (lawyer != null)
                    {
                        docPerson.FullName = $"{lawyer.FullName} ({lawyer.Code} {lawyer.Department})";
                        docPerson.Person_SourceType = SourceTypeSelectVM.LawUnit;
                        docPerson.Person_SourceId = lawyer.Id;
                        docPerson.Uic = null;
                    }
                }

                if (person.Addresses != null)
                {
                    docPerson.Addresses = person.Addresses.Select(x => new DocumentPersonAddressVM
                    {
                        Address = x.Address
                    }).ToList();
                    for (int i = 0; i < docPerson.Addresses.Count; i++)
                    {
                        docPerson.Addresses[i].Id = i;
                        docPerson.Addresses[i].Id = 0;
                        docPerson.Addresses[i].Address.Id = 0;
                    }
                }

                model.DocumentPersons.Add(docPerson);
            }
            await InitializeDocumentVMFromRequest(model, elDoc.RequestTypeCode);
        }

        public Task<ElectronicDocumentInfoVM> GetElectronicDocumentInfo(long id)
        {
            return repo.AllReadonly<ElectronicDocument>()
                                    .Where(x => x.Id == id)
                                    .Select(x => new ElectronicDocumentInfoVM
                                    {
                                        EpepUserInfo = $"{(!string.IsNullOrEmpty(x.EpepUser.LawyerNumber) ? x.EpepUser.LawyerNumber : x.EpepUser.Uic)} {x.EpepUser.FullName}",
                                        ApplyDate = x.ApplyDate,
                                        ApplyNumber = x.ApplyNumber,
                                        PaidDate = x.PaidDate,
                                        Description = x.Description,
                                        CurrencyCode = x.CurrencyCode,
                                        TaxAmount = x.TaxAmount,
                                        BaseAmount = x.BaseAmount,
                                        CaseInfo = (x.CaseId > 0) ? $"{x.Case.CaseType.Code} {x.Case.ShortNumberValue}/{x.Case.RegDate:yyyy}" : null,
                                        PersonInfo = (x.CasePersonId > 0) ? $"{x.CasePerson.FullName} ({x.CasePerson.PersonRole.Label})" : null,
                                        PaymentType = (x.PaymentTypeId > 0) ? x.PaymentType.Label : "",
                                        PaymentTypeId = x.PaymentTypeId ?? 0,
                                        PaidInCourtName = (x.VPOSPaidInCourtId) > 0 ? x.VPOSPaidInCourt.Label : ""
                                    }).FirstOrDefaultAsync();
        }

        private async Task document_InitFromTemplate(DocumentVM model, int templateId)
        {
            var template = await this.GetByIdAsync<DocumentTemplate>(templateId);
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
                var caseModel = await repo.GetByIdAsync<Case>(template.CaseId);
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
                var documentDecision = await repo.GetByIdAsync<DocumentDecision>(template.SourceId);
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

        private Task<Document> document_GetById(long id, bool readOnly)
        {
            IQueryable<Document> documents = repo.All<Document>();
            if (readOnly)
            {
                documents = repo.AllReadonly<Document>();
            }
            return documents
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
                            .ThenInclude(x => x.CaseState)
                            .Include(x => x.DocumentResolutions)
                            .Where(x => x.Id == id)
                            .AsSplitQuery()
                            .FirstOrDefaultAsync();
        }
        public async Task<DocumentVM> Document_GetById(long id)
        {
            var document = await repo.AllReadonly<Document>()
                                    .Include(x => x.DocumentType)
                                    .Include(x => x.DocumentGroup)
                                    .Include(x => x.DocumentPersons)
                                    .ThenInclude(x => x.Addresses)
                                    .ThenInclude(x => x.Address)
                                    .Include(x => x.DocumentPersons)
                                    .ThenInclude(x => x.PersonRole)
                                    .Include(x => x.DocumentCaseInfo)
                                    .Include(x => x.DocumentInstitutionCaseInfo)
                                    .ThenInclude(x => x.Institution)
                                    .Include(x => x.DocumentLinks)
                                    .Include(x => x.Cases)
                                    .ThenInclude(x => x.CaseState)
                                    .Include(x => x.DocumentResolutions)
                                    .Include(x => x.DocumentRequestType)
                                    .Where(x => x.Id == id)
                                    .AsSplitQuery()
                                    .FirstOrDefaultAsync();

            if (document == null)
            {
                return null;
            }
            var templateId = await repo.AllReadonly<DocumentTemplate>()
                                    .Where(x => x.DocumentId == id)
                                    .Select(x => x.Id)
                                    .FirstOrDefaultAsync();
            var model = new DocumentVM()
            {
                Id = document.Id,
                RequestTypeId = document.DocumentRequestTypeId,
                RequestTypeCode = (document.DocumentRequestType != null) ? document.DocumentRequestType.RequestCode : null,
                CourtId = document.CourtId,
                CourtOrganizationId = document.CourtOrganizationId,
                DocumentNumber = document.DocumentNumber,
                DocumentDate = document.DocumentDate,
                ElectronicDocumentId = document.ElectronicDocumentId,
                AssignmentDocumentId = document.AssignmentDocumentId,
                DocumentDirectionId = document.DocumentDirectionId,
                DocumentKindId = document.DocumentGroup.DocumentKindId,
                DocumentGroupId = document.DocumentGroupId,
                DocumentTypeId = document.DocumentTypeId,
                Description = document.Description,
                DeliveryGroupId = document.DeliveryGroupId,
                DeliveryTypeId = document.DeliveryTypeId,
                PostOfficeDate = document.PostOfficeDate,
                IsRestictedAccess = document.IsRestictedAccess,
                IsSecret = document.IsSecret ?? false,
                IsOldNumber = document.IsOldNumber ?? false,
                OldDocumentDate = document.DocumentDate,
                OldDocumentNumber = document.DocumentNumber,
                ActualDocumentDate = document.ActualDocumentDate,
                MultiRegistationId = document.MultiRegistationId,
                HasDocumentResolutions = document.DocumentResolutions.AsQueryable()
                                            .Where(FilterExpireInfo<DocumentResolution>(false)).Any(),
                DateExpired = document.DateExpired
            };
            if (model.AssignmentDocumentId > 0)
            {
                var assInfo = await repo.AllReadonly<Document>()
                                        .Where(x => x.Id == model.AssignmentDocumentId.Value)
                                        .Select(x => new
                                        {
                                            CourtLabel = x.Court.Label,
                                            CreatedCourtLabel = x.CreatedCourt.Label,
                                            x.DocumentNumber,
                                            x.DocumentDate
                                        }).FirstOrDefaultAsync();
                if (assInfo != null)
                {
                    model.AssignmentDocumentCourt = assInfo.CreatedCourtLabel ?? assInfo.CourtLabel;
                    model.AssignmentDocumentNumber = assInfo.DocumentNumber;
                    model.AssignmentDocumentDate = assInfo.DocumentDate;
                }
            }
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
                if (await mdocInfo.AnyAsync())
                {
                    var currentIndex = await mdocInfo.Where(x => x.Id <= model.Id).OrderBy(x => x.Id).CountAsync();

                    model.MultiRegistationInfo = $"Документ {currentIndex} от {await mdocInfo.CountAsync()}; От номер {(await mdocInfo.FirstOrDefaultAsync()).DocumentNumberValue} до номер {(await mdocInfo.LastOrDefaultAsync()).DocumentNumberValue}";
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
                    PersonRoleLabel = docPerson.PersonRole.Label,
                    PersonRoleId = docPerson.PersonRoleId,
                    PersonRoleKind = docPerson.PersonRole.RoleKindId,
                    PersonMaturityId = docPerson.PersonMaturityId,
                    IsDeceased = docPerson.IsDeceased,
                    DateDeceased = docPerson.DateDeceased,
                    PersonGid = docPerson.PersonGid,
                    RepresentsGid = docPerson.RepresentsGid
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
                person.NewDynamicItem = false;
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
                if (model.DocumentCaseInfo.CourtId > 0)
                {
                    model.DocumentCaseInfo.CourtName = await repo.GetPropByIdAsync<Court, string>(x => x.Id == model.DocumentCaseInfo.CourtId, x => x.Label);
                }
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
                model.CaseIsRejected = NomenclatureConstants.CaseState.UnregisteredManageble.Contains(_case.CaseStateId);
                model.CaseRejectStateName = _case.CaseState.Label;
                model.CaseRegisterNumber = _case.RegNumber;
                model.CaseClassifications = caseClassificationService.FillCheckListVMs(_case.Id, 0);
            }

            model.DocumentLinkToOther = await repo.AllReadonly<DocumentLink>()
                                            .Where(x => x.PrevDocumentId == model.Id)
                                            .Select(x => new DocumentLinkToOtherVM
                                            {
                                                DocumentId = x.Document.Id,
                                                CourtName = x.Document.Court.Label,
                                                DocumentTypeName = x.Document.DocumentType.Label,
                                                DocumentNumber = x.Document.DocumentNumber,
                                                DocumentDate = x.Document.DocumentDate
                                            })
                                            .ToListAsync();
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

        public Task<bool> Document_SaveData(DocumentVM model)
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

        private async Task<bool> document_InsertDataMulti(DocumentVM model)
        {
            model.MultiRegistationId = null;
            if (model.CourtId == 0)
            {
                model.CourtId = userContext.CourtId;
            }
            // model.MultiDocumentCounter = 5;
            bool result = true;
            if (model.MultiDocumentCounter > 1)
            {
                var counterResult = counterService.Counter_GetDocumentCounterMulti(model.MultiDocumentCounter, model.DocumentDirectionId, model.CourtId);
                result = (counterResult != null) && (counterResult.Value > 0);
                int currentDocumentNumber = counterResult.Value - model.MultiDocumentCounter + 1;
                var dtDocDate = DateTime.Now;
                //dtDocDate = dtDocDate.AddMilliseconds(-dtDocDate.Millisecond);
                model.MultiRegistationId = Guid.NewGuid().ToString().ToLower();
                long firstDocumentId = 0;
                int retryCount = 10;
                for (int counterAdd = 0; counterAdd < model.MultiDocumentCounter; counterAdd++)
                {
                    counterResult.Value = currentDocumentNumber + counterAdd;
                    bool docSavedOk = false;
                    int attemptNo = 0;
                    bool loopExit;
                    do
                    {
                        model.DocumentNumberValue = counterResult.Value;
                        model.DocumentNumber = counterResult.GetStringValue();
                        //Добавят се по 1 милисекунда на всеки документ за да може да се подреждат правилно по дата
                        model.DocumentDate = dtDocDate;
                        //model.DocumentDate = dtDocDate.AddMilliseconds(counterAdd);
                        docSavedOk = await document_InsertData(model);
                        attemptNo++;
                        if (!docSavedOk)
                        {
                        }
                        loopExit = (attemptNo > retryCount) || docSavedOk;

                    } while (!loopExit);
                    result &= docSavedOk;
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
                result = await document_InsertData(model);
            }
            return result;
        }

        private async Task<bool> document_InsertData(DocumentVM model)
        {
            try
            {
                using (var ts = repo.BeginTransaction(model.DisableTransaction))
                {

                    var document = new Document()
                    {
                        CourtId = model.CourtId,
                        CreatedCourtId = model.CourtId,
                        DocumentRequestTypeId = model.RequestTypeId,
                        ElectronicDocumentId = model.ElectronicDocumentId,
                        AssignmentDocumentId = model.AssignmentDocumentId,
                        CourtOrganizationId = model.CourtOrganizationId,
                        DocumentDirectionId = model.DocumentDirectionId,
                        DocumentGroupId = model.DocumentGroupId,
                        DocumentTypeId = model.DocumentTypeId.Value,
                        DeliveryGroupId = model.DeliveryGroupId,
                        DeliveryTypeId = model.DeliveryTypeId,
                        PostOfficeDate = model.PostOfficeDate,
                        IsRestictedAccess = model.IsRestictedAccess,
                        IsSecret = model.IsSecret,
                        IsOldNumber = model.IsOldNumber,
                        MultiRegistationId = model.MultiRegistationId.EmptyToNull(),
                        Description = model.Description,
                        DateExpired = model.DateExpired
                    };
                    if (userContext.CourtId > 0)
                    {
                        document.CreatedCourtId = userContext.CourtId;
                    }
                    SetUserDateWRT(document);
                    document.RegisterUserId = userContext.UserId;
                    //Запис на лица и адреси
                    document_SavePersons(model, document);
                    document_SaveCaseInfo(model, document);
                    document_SaveInstitutionCaseInfo(model, document);
                    document_SaveDocumentLink(model, document);
                    document_UpdateRegix(model, document);
                    if (model.CaseTypeId > 0)
                    {
                        await document_InitNewCase(model, document);
                    }

                    bool isOkNumber = false;
                    //TODO: евентуално timestamp....
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
                    if (!string.IsNullOrEmpty(document.DocumentNumber))
                    {
                        document.DocumentDeclaredDate = document.DocumentDate;
                    }
                    //Регистриране на номер
                    if (isOkNumber)
                    {
                        repo.Add<Document>(document);
                        await repo.SaveChangesAsync();
                        model.Id = document.Id;
                        if (document.Cases != null && document.Cases.Count > 0)
                        {
                            model.CaseId = document.Cases.First().Id;
                        }

                        if (document.CourtId != NomenclatureConstants.Courts.RandomAssignment)
                        {
                            if (model.TemplateId == 0)
                            {
                                await epepService.AppendDocument(document, EpepConstants.ServiceMethod.Add);
                            }
                            deadlineService.DeadLineCompanyCaseStartOnDocument(document);

                            await repo.SaveChangesAsync();

                            if ((document.DocumentDirectionId == DocumentConstants.DocumentDirection.Incoming) &&
                                (document.DocumentGroupId == NomenclatureConstants.DocumentGroup.DocumentForComplain_AccompanyingDocument))
                            {
                                caseSessionActComplainService.CaseSessionActComplain_CreateFromDocument(document.Id);
                            }
                        }

                        await workNotificationService.SaveNotificationsForCompliantDocumentCaseFastProcess(document.Id);
                        await deadlineService.StartMissingActForCompliantDocumentFastProcess(document.Id);
                        await workNotificationService.ExpiredNotificationsForExpressingOpinionObjectionFastProcess(document.Id, false);
                        await workNotificationService.TurnOffNotificationsObjectionForLackSubmittedObjectionFastProcess(document.Id, false);
                        await repo.SaveChangesAsync();

                        await FinishElectronicDocumentSave(document, document.DocumentPersons.FirstOrDefault());
                        ts.Commit();
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                if (model.MultiDocumentCounter > 1)
                {
                    logger.LogError(ex, $"Грешка при регистрация на multi документ No={model.DocumentNumber}; court:{userContext.CourtName}");
                }
                else
                {
                    logger.LogError(ex, $"Грешка при регистрация на документ Id={model.Id}");
                }
                model.Id = 0;
                model.DocumentNumber = null;
            }
            return false;
        }

        public async Task<SaveResultVM> ValidatePersonOrgs(DocumentVM documentModel)
        {
            if (documentModel.CourtId != NomenclatureConstants.Courts.RandomAssignment)
            {
                return new SaveResultVM(true);
            }

            //Ако е отбелязана класификация Лица по чл.50 и 52 се допуска подаване и на гише
            if (documentModel.CaseClassifications.Any(c => c.Value == NomenclatureConstants.CaseClassifications.FP_5152.ToString()))
            {
                return new SaveResultVM(true);
            }
            //Валидира подадените лица по ЕИК/ЕГН дали не съществуват в списъка на организации,
            //задължени да подават документи само през ЕПЕП
            int checkSize = 50;
            int checkNo = 0;
            while (documentModel.DocumentPersons.Skip(checkNo * checkSize).Take(checkSize).Count() > 0)
            {
                var checkUicsArr = documentModel.DocumentPersons.Skip(checkNo * checkSize).Take(checkSize)
                                .Select(x => x.Uic)
                                .Where(x => x != null && x != "")
                                .Distinct()
                                .ToArray();

                var orgListEpepOnly = await repo.AllReadonly<Institution>()
                                                .Where(x => x.InstitutionTypeId == NomenclatureConstants.InstitutionTypes.OrgEpepOnly)
                                                .Where(x => x.DateTo == null)
                                                .Where(x => checkUicsArr.Contains(x.Uic))
                                                .Select(x => x.FullName)
                                                .ToListAsync();

                if (orgListEpepOnly.Any())
                {
                    return new SaveResultVM(false, $"{orgListEpepOnly.First()} са задължени да подават документи само по електронен път.");
                }

                checkNo++;

            }
            return new SaveResultVM(true);
        }

        /// <summary>
        /// Валидиране на документи, извън Централна регистратура
        /// </summary>
        /// <param name="documentModel"></param>
        /// <returns></returns>
        public async Task<SaveResultVM> ValidateDocumentAfterCR(DocumentVM documentModel)
        {
            if (documentModel.CourtId == NomenclatureConstants.Courts.RandomAssignment)
            {
                return await Task.FromResult(new SaveResultVM(true));
            }

            //Валидацията е премахната на 02.07 Защото можело и така
            //if (documentModel.DocumentKindId == DocumentConstants.DocumentKind.InitialDocument && documentModel.CaseCodeId > 0)
            //{
            //    if (await GetParamValueDate(NomenclatureConstants.SystemParamName.ZP_StartRegDate, "01.07.2025") < DateTime.Now)
            //    {
            //        if (await repo.AllReadonly<DocumentRequestType>()
            //                        .Where(x => x.CaseCodeId == documentModel.CaseCodeId)
            //                        .AnyAsync())
            //        {
            //            return new SaveResultVM(false, "Не може да входирате документ по този шифър извън Централна регистратура");
            //        }
            //    }
            //}

            return await Task.FromResult(new SaveResultVM(true));
        }

        /// <summary>
        /// Копира CommonMongoFile редовете от електронния документ в документа от регистратура, без копиране на файловете, защото не се променят
        /// </summary>
        /// <param name="model"></param>
        private async Task FinishElectronicDocumentSave(Document model, DocumentPerson firstPerson)
        {
            if (model.AssignmentDocumentId > 0)
            {
                await copyFilesFromElectronicOrAssignmentDocument(false, model.AssignmentDocumentId.Value, model.Id, model.DocumentDate, model.CourtId);
                if ((model.ElectronicDocumentId ?? 0) > 0)
                {
                    //Ако по този централен документ вече има начислени задължения - не генерира нови
                    bool hasObligations = await repo.AllReadonly<Obligation>()
                                                    .Where(x => x.Document.AssignmentDocumentId == model.AssignmentDocumentId.Value)
                                                    .Where(x => x.Document.ElectronicDocumentId == model.ElectronicDocumentId.Value)
                                                    .AnyAsync();
                    if (!hasObligations)
                    {
                        await FinishElectronicDocumentSaveMoney(model.ElectronicDocumentId, model, firstPerson);
                    }
                }
            }
            else
            {
                if ((model.ElectronicDocumentId ?? 0) > 0)
                {
                    await FinishElectronicDocumentSaveMoney(model.ElectronicDocumentId, model, firstPerson);
                    await copyFilesFromElectronicOrAssignmentDocument(true, model.ElectronicDocumentId.Value, model.Id, model.DocumentDate, model.CourtId);

                    if (model.CourtId != NomenclatureConstants.Courts.RandomAssignment && model.AssignmentDocumentId == null)
                    {
                        var transResult = await transactionService.AppendTransaction(SourceTypeSelectVM.ElectronicDocument, model.ElectronicDocumentId.Value, NomenclatureConstants.MainTransactionTypes.Finish);
                    }
                }
            }
        }

        async Task<bool> copyFilesFromElectronicOrAssignmentDocument(bool fromElDoc, long fromDocumentId, long toDocumentId, DateTime dtUploaded, int courtId)
        {
            List<MongoFile> filesToCopy = new();
            int[] sourceTypesToCopy = SourceTypeSelectVM.ElectronicDocumentAllFilesForCopy;
            if (!fromElDoc)
            {
                sourceTypesToCopy = SourceTypeSelectVM.DocumentAllFilesForCopy;
            }
            //Типовете на документите се подменят само от електронните документи
            //От ЦР -> В локален документ, са същите като в ЦР

            filesToCopy = await repo.AllReadonly<MongoFile>()
                                                        .Where(x => x.SourceId == fromDocumentId.ToString())
                                                        .Where(x => sourceTypesToCopy.Contains(x.SourceType))
                                                        .ToListAsync();

            foreach (var file in filesToCopy)
            {
                file.Id = 0;
                switch (file.SourceType)
                {
                    case SourceTypeSelectVM.Document:
                        //Този тип не се променя
                        break;
                    case SourceTypeSelectVM.ElectronicDocumentRequest:
                        //Заявление от електронен документ се пренася като заявление в документ от деловодство
                        file.SourceType = SourceTypeSelectVM.DocumentRequest;
                        break;
                    default:
                        //Ако се копира от електронен документ
                        if (fromElDoc)
                        {
                            file.SourceType = SourceTypeSelectVM.DocumentFromElectronicDocument;
                        }
                        break;
                }

                file.SourceId = toDocumentId.ToString();
                file.DateUploaded = dtUploaded;
                repo.Add(file);
            }



            if (filesToCopy.Any())
            {
                await repo.SaveChangesAsync();
                if (courtId != NomenclatureConstants.Courts.RandomAssignment)
                {
                    foreach (var file in filesToCopy)
                    {
                        if (SourceTypeSelectVM.DocumentsNoCopytoEPEP.Contains(file.SourceType))
                        {
                            continue;
                        }
                        epepService.AppendFile(new Infrastructure.Models.Cdn.CdnUploadRequest()
                        {
                            SourceType = file.SourceType,
                            SourceId = toDocumentId.ToString(),
                            MongoFileId = file.Id

                        }, EpepConstants.ServiceMethod.Add);
                    }
                }
            }

            return true;
        }
        public async Task<bool> FinishElectronicDocumentSaveMoney(long? electronicDocumentId, Document model, DocumentPerson firstPerson)
        {
            //Пари се начисляват само в документите, регистрирани в истински съд, не в Случайно разпределение
            if ((electronicDocumentId ?? 0) <= 0 || model.CourtId == NomenclatureConstants.Courts.RandomAssignment)
            {
                return false;
            }

            if (firstPerson == null)
            {
                return false;
            }

            var elDocModel = await repo.GetByIdAsync<ElectronicDocument>(electronicDocumentId.Value);
            if (elDocModel.TaxAmount > 0M && elDocModel.MoneyFeeTypeId > 0 && elDocModel.PaidDate.HasValue)
            {
                decimal docTaxAmount = elDocModel.TaxAmount.Value;
                decimal docTaxAmountBGN = docTaxAmount;
                if (elDocModel.CurrencyCode == NomenclatureConstants.CurrencyCode.EUR)
                {
                    docTaxAmountBGN = DbEuroConfig.GetBGNFromEUR(docTaxAmount, elDocModel.PaidDate);
                }
                if (elDocModel.CurrencyCode == NomenclatureConstants.CurrencyCode.BGN && DbEuroConfig.IsInEuro)
                {
                    docTaxAmountBGN = elDocModel.TaxAmount.Value;
                    docTaxAmount = DbEuroConfig.GetEURFromBGN(docTaxAmountBGN);
                }

                int paidInCourt = model.CourtId;
                if (elDocModel.VPOSPaidInCourtId > 0)
                {
                    paidInCourt = elDocModel.VPOSPaidInCourtId.Value;
                }

                var newObligation = new Obligation()
                {
                    Amount = docTaxAmount,
                    AmountBGN = docTaxAmountBGN,
                    DocumentId = model.Id,
                    CourtId = model.CourtId,
                    MoneyTypeId = NomenclatureConstants.MoneyType.StateFee,
                    MoneyFeeTypeId = elDocModel.MoneyFeeTypeId,
                    ObligationDate = elDocModel.ApplyDate,
                    MoneySign = NomenclatureConstants.MoneySign.SignPlus,

                    IsActive = true,
                    DateWrt = model.DocumentDate,
                    UserId = model.UserId
                };



                if (model.Cases != null && model.Cases.Count > 0)
                {
                    newObligation.CaseId = model.Cases.First().Id;
                }
                else
                {
                    if (model.DocumentCaseInfo != null && model.DocumentCaseInfo.Count > 0)
                    {
                        var caseInfo = model.DocumentCaseInfo.First();
                        if (caseInfo.CourtId == model.CourtId && caseInfo.CaseId > 0)
                        {
                            newObligation.CaseId = caseInfo.CaseId;
                        }
                    }
                }

                newObligation.CopyFrom(firstPerson);
                newObligation.Person_SourceType = SourceTypeSelectVM.DocumentPerson;
                newObligation.Person_SourceId = firstPerson.Id;

                if (counterService.Counter_GetObligationCounter(newObligation))
                {

                    if (NomenclatureConstants.PaymentType.InstantPayments.Contains(elDocModel.PaymentTypeId ?? 0))
                    {
                        //Само платените на ПОС/ВПОС документи се прихващат плащанията
                        var oblPayment = new ObligationPayment()
                        {
                            Amount = newObligation.Amount,
                            AmountBGN = newObligation.AmountBGN,
                            IsActive = true,
                            UserId = model.UserId,
                            DateWrt = model.DocumentDate
                        };

                        var newPayment = new Payment()
                        {
                            CourtId = paidInCourt,
                            IsActive = true,
                            IsAvans = false,
                            Amount = newObligation.Amount,
                            AmountBGN = newObligation.AmountBGN,
                            PaymentTypeId = elDocModel.PaymentTypeId ?? NomenclatureConstants.PaymentType.EPEP,
                            SenderName = firstPerson.FullName,
                            PaidDate = elDocModel.PaidDate.Value,
                            PaymentDescription = "Платено през ЕПЕП",
                            DateWrt = DateTime.Now
                        };
                        if (counterService.Counter_GetPaymentCounter(newPayment))
                        {
                            oblPayment.Payment = newPayment;
                            newObligation.ObligationPayments.Add(oblPayment);
                        }
                    }

                    repo.Add(newObligation);
                    await repo.SaveChangesAsync();
                    return true;

                }
            }
            return false;
        }

        private async Task<bool> document_UpdateData(DocumentVM model)
        {
            var document = await document_GetById(model.Id, false);
            document.CourtOrganizationId = model.CourtOrganizationId;
            document.DocumentGroupId = model.DocumentGroupId;
            document.DocumentTypeId = model.DocumentTypeId.Value;
            document.Description = model.Description;
            document.IsRestictedAccess = model.IsRestictedAccess;
            document.IsSecret = model.IsSecret;
            document.DeliveryGroupId = model.DeliveryGroupId;
            document.DeliveryTypeId = model.DeliveryTypeId;
            document.PostOfficeDate = model.PostOfficeDate;

            document_SavePersons(model, document);
            await document_UpdateCase(model);
            document_SaveCaseInfo(model, document);

            bool hasCaseInfoChange = ((model.HasCaseInfo ? 1 : 0) != document.DocumentCaseInfo.Count);
            if (!hasCaseInfoChange)
            {
                if (document.DocumentCaseInfo.Count > 0 && document.DocumentCaseInfo.FirstOrDefault().CaseId != model.DocumentCaseInfo.CaseId)
                {
                    hasCaseInfoChange = true;
                }
            }

            document_SaveInstitutionCaseInfo(model, document);
            document_SaveDocumentLink(model, document);
            SetUserDateWRT(document);
            repo.Update<Document>(document);
            deadlineService.DeadLineCompanyCaseStartOnDocument(document);
            try
            {
                await repo.SaveChangesAsync().ConfigureAwait(true);
                if (model.TemplateId == 0 || (model.TemplateId > 0 && epepService.CheckOutDocumentForSend(model.Id)))
                {
                    await epepService.AppendDocument(document, EpepConstants.ServiceMethod.Update);
                }
                if (hasCaseInfoChange)
                {
                    await workNotificationService.ExpiredEditNotificationsForCompliantDocumentCaseFastProcess(document.Id);
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при редакция на документ Id={model.Id}");
                return false;
            }
        }

        public async Task<bool> Document_SaveCommonToCompliant(DocumentVM model)
        {
            var document = await document_GetById(model.Id, false).ConfigureAwait(true);
            document.DocumentGroupId = model.DocumentGroupId;
            document.DocumentTypeId = model.DocumentTypeId.Value;
            document.Description = model.Description;
            SetUserDateWRT(document);

            model.HasCaseInfo = true;

            document_SaveCaseInfo(model, document);
            try
            {
                await repo.SaveChangesAsync().ConfigureAwait(true);
                await epepService.AppendDocument(document, EpepConstants.ServiceMethod.Add);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при редакция на документ Id={model.Id}");
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
                            repo.Delete<DocumentPersonAddress>(sPersonAddress);
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
                                repo.Delete<Address>(sPersonAddress.AddressId);
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
                        savedPerson.PersonGid = modelPerson.PersonGid;
                        savedPerson.RepresentsGid = modelPerson.RepresentsGid;
                        PersonNamesBase_SaveData(savedPerson, document.Id > 0);
                        //repo.Update<DocumentPerson>(savedPerson);
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
                        MilitaryRangId = person.MilitaryRangId,
                        IsDeceased = person.IsDeceased,
                        DateDeceased = person.DateDeceased,
                        PersonGid = person.PersonGid,
                        RepresentsGid = person.RepresentsGid
                    };
                    newElement.CopyFrom(person);
                    PersonNamesBase_SaveData(newElement, false);
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
        private async Task document_InitNewCase(DocumentVM model, Document entity)
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
                CaseGroupId = await repo.GetPropByIdAsync<CaseType, int>(x => x.Id == model.CaseTypeId, x => x.CaseGroupId),
                CaseCharacterId = await repo.AllReadonly<CaseTypeCharacter>(x => x.CaseTypeId == model.CaseTypeId).Select(x => x.CaseCharacterId).FirstOrDefaultAsync(),
                CaseTypeId = model.CaseTypeId.Value,
                CaseCodeId = model.CaseCodeId,
                CaseStateId = NomenclatureConstants.CaseState.Draft,
                IsRestictedAccess = model.IsRestictedAccess,
                EISSPNumber = model.EISSPNumber,
                ProcessPriorityId = model.ProcessPriorityId,
                UserId = userContext.UserId,
                DateWrt = DateTime.Now,
                //FIX -infinity
                //RegDate = new DateTime(1,1,1)
            };
            var classificationSecret = await repo.AllReadonly<Classification>().Where(x => x.Code == "secret").FirstOrDefaultAsync();
            var classificationRestricted = await repo.AllReadonly<Classification>().Where(x => x.Code == "restricted").FirstOrDefaultAsync();
            var classificationMinor = await repo.AllReadonly<Classification>().Where(x => x.Code == "minors").FirstOrDefaultAsync();
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
        private async Task document_UpdateCase(DocumentVM model)
        {
            var caseModel = await repo.All<Case>(x => x.DocumentId == model.Id).FirstOrDefaultAsync();
            if (caseModel != null && caseModel.CaseStateId == NomenclatureConstants.CaseState.Draft)
            {
                caseModel.EISSPNumber = model.EISSPNumber;
                caseModel.ProcessPriorityId = model.ProcessPriorityId;
                caseModel.CaseTypeId = model.CaseTypeId.Value;
                caseModel.CaseGroupId = await repo.GetPropByIdAsync<CaseType, int>(x => x.Id == model.CaseTypeId, x => x.CaseGroupId);
                caseModel.CaseCodeId = model.CaseCodeId;
                caseModel.CaseCharacterId = await repo.AllReadonly<CaseTypeCharacter>(x => x.CaseTypeId == model.CaseTypeId).Select(x => x.CaseCharacterId).FirstOrDefaultAsync();

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

        /// <summary>
        /// Метод зареждащ данни за начини на получаване/изпращане по направление на документ
        /// </summary>
        /// <param name="documentDirection">Направление</param>
        /// <param name="addDefaultElement">Флаг дали да добави елемент "Избери"</param>
        /// <returns></returns>
        public List<SelectListItem> GetDeliveryGroups(int documentDirection, bool addDefaultElement = false)
        {
            List<SelectListItem> result = repo.AllReadonly<DeliveryDirectionGroup>()
                                              .Where(x => x.DocumentDirectionId == documentDirection)
                                              .Select(x => new SelectListItem()
                                              {
                                                  Value = x.DeliveryGroup.Id.ToString(),
                                                  Text = x.DeliveryGroup.Label
                                              })
                                              .OrderBy(x => x.Text)
                                              .ToList();

            if (addDefaultElement)
                result = result.Prepend(new SelectListItem() { Text = "Избери", Value = "-1" }).ToList();

            return result;
        }

        public IEnumerable<LabelValueVM> GetDocument(int courtId, string documentNumber, int docDirection)
        {
            documentNumber = documentNumber?.ToLower();
            Expression<Func<Document, bool>> whereDir = x => true;
            if (docDirection > 0)
            {
                whereDir = x => x.DocumentDirectionId == docDirection;
            }

            var result = repo.AllReadonly<Document>()
                            .Where(x => x.CourtId == courtId)
                            .Where(x => x.DocumentNumber == documentNumber)
                            .Where(whereDir)
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
                        .Select(x => new LabelValueVM
                        {
                            Value = x.Id.ToString(),
                            Label = x.DocumentType.Label + " " + (x.DocumentNumber ?? "") + "/" + x.DocumentDate.ToString("dd.MM.yyyy")
                        }).FirstOrDefault();
        }

        public List<SelectListItem> DocumentPerson_SelectForDropDownList(long documentId)
        {
            var result = repo.AllReadonly<DocumentPerson>()
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
                if (!string.IsNullOrEmpty(result)) result += "; ";

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
                                        .AsSplitQuery()
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
                        Id = adr.AddressId.ToString(),
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

        public async Task<List<DocumentPersonVM>> SelectDocumentPersonsFromCase(DocumentSelectPersonsVM model, int index)
        {
            var caseModel = await repo.AllReadonly<Case>()
                                        .Include(x => x.CaseType)
                                        .Include(x => x.CasePersons.Where(p => p.CaseSessionId == null))
                                        .ThenInclude(x => x.Addresses)
                                        .ThenInclude(x => x.Address)
                                        //.ThenInclude(x => x.AddressType)
                                        .Include(x => x.CasePersons)
                                        .ThenInclude(x => x.PersonRole)
                                        .Where(x => x.Id == int.Parse(model.SourceId))
                                        .AsSplitQuery()
                                        .FirstOrDefaultAsync();

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
                    docPerson.PersonRoleLabel = person.PersonRole.Label;
                    docPerson.MilitaryRangId = person.MilitaryRangId;
                    docPerson.PersonMaturityId = person.PersonMaturityId;
                    //docPerson.PersonGid = person.PersonGid ?? ;
                    foreach (var pAdr in person.Addresses)
                    {
                        var searchAdr = searchPerson.Addresses.FirstOrDefault(sp => sp.Id == pAdr.AddressId.ToString());
                        if (searchAdr != null)
                        {
                            var docPersonAddress = new DocumentPersonAddressVM()
                            {
                                PersonIndex = docPerson.Index,
                                Index = docPerson.Addresses.Count
                            };
                            docPersonAddress.Address.CopyFrom(pAdr.Address);
                            docPerson.Addresses.Add(docPersonAddress);
                        }
                    }
                    result.Add(docPerson);
                }
            }
            return result;
        }

        public async Task<IQueryable<DocumentSelectAddressVM>> SelectAddressListByPerson(string uic, int uicTypeId, int? personSourceType,
                        long? personSourceId)
        {
            IQueryable<DocumentSelectAddressVM> result = null;
            if ((personSourceType ?? 0) > 0)
            {

                switch (personSourceType)
                {
                    case SourceTypeSelectVM.Court:
                        result = repo.AllReadonly<Court>()
                                        .Where(x => x.Id == personSourceId)
                                        .Where(x => x.CourtAddress.CityCode != null)
                                        .Select(x => new DocumentSelectAddressVM
                                        {
                                            Id = x.CourtAddress.Id.ToString(),
                                            AddressTypeName = x.CourtAddress.AddressType.Label,
                                            FullAddress = x.CourtAddress.FullAddress
                                        });
                        break;
                    case SourceTypeSelectVM.Instutution:
                        result = repo.AllReadonly<InstitutionAddress>()
                                       .Where(x => x.InstitutionId == personSourceId)
                                       .Where(x => x.Address.CityCode != null)
                                       .Select(x => new DocumentSelectAddressVM
                                       {
                                           Id = x.Address.Id.ToString(),
                                           AddressTypeName = x.Address.AddressType.Label,
                                           FullAddress = x.Address.FullAddress
                                       });
                        break;
                    case SourceTypeSelectVM.LawUnit:
                        result = repo.AllReadonly<LawUnitAddress>()
                                           .Where(x => x.LawUnitId == personSourceId)
                                           .Where(x => x.Address.CityCode != null)
                                           .Select(x => new DocumentSelectAddressVM
                                           {
                                               Id = x.Address.Id.ToString(),
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
                    var documentAddresses = await repo.AllReadonly<DocumentPersonAddress>()
                                            .Where(x => x.DocumentPerson.Uic == uic && x.DocumentPerson.UicTypeId == uicTypeId)
                                            .Where(x => x.Address.CityCode != null)
                                            .OrderByDescending(x => x.Id)
                                            .Select(x => new DocumentSelectAddressVM
                                            {
                                                Id = x.Address.Id.ToString(),
                                                AddressTypeName = x.Address.AddressType.Label,
                                                FullAddress = x.Address.FullAddress
                                            }).Take(50)
                                            .ToListAsync();

                    var caseAddresses = await repo.AllReadonly<CasePersonAddress>()
                                            .Where(x => x.CasePerson.Uic == uic && x.CasePerson.UicTypeId == uicTypeId)
                                            .Where(x => x.Address.CityCode != null)
                                            .Where(FilterExpireInfo<CasePersonAddress>(false))
                                            .OrderByDescending(x => x.Id)
                                            .Select(x => new DocumentSelectAddressVM
                                            {
                                                Id = x.Address.Id.ToString(),
                                                AddressTypeName = x.Address.AddressType.Label,
                                                FullAddress = x.Address.FullAddress
                                            }).Take(50)
                                            .ToListAsync();

                    var lawUnitAddresses = await repo.AllReadonly<LawUnitAddress>()
                                            .Where(x => x.LawUnit.Uic == uic && x.LawUnit.UicTypeId == uicTypeId)
                                            .Where(x => x.Address.CityCode != null)
                                            .OrderByDescending(x => x.AddressId)
                                            .Select(x => new DocumentSelectAddressVM
                                            {
                                                Id = x.Address.Id.ToString(),
                                                AddressTypeName = x.Address.AddressType.Label,
                                                FullAddress = x.Address.FullAddress
                                            }).Take(50)
                                            .ToListAsync();

                    result = documentAddresses.Union(caseAddresses).Union(lawUnitAddresses)
                                        .OrderByDescending(x => x.Id)
                                        .AsQueryable()
                                        .GroupBy(x => new { x.AddressTypeName, x.FullAddress })
                                        .Select(g => g.FirstOrDefault());
                }
            }

            if (result == null)
                result = Enumerable.Empty<DocumentSelectAddressVM>().AsQueryable();

            return result;
        }

        public async Task<(bool result, string errorMessage)> DocumentDecision_SaveData(DocumentDecision model)
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
                    await DocumentDecision_SaveData_FinishTask(model.DocumentId);
                }

                await repo.SaveChangesAsync();
                return (result: true, errorMessage: "");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на DocumentDecision Id={model.Id}");
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

        private async Task DocumentDecision_SaveData_FinishTask(long documentId)
        {
            var myRouteTasks = await repo.AllReadonly<WorkTask>(
                x => x.SourceId == documentId
                && x.SourceType == SourceTypeSelectVM.Document
                && x.UserId == userContext.UserId
                && x.TaskTypeId == WorkTaskConstants.Types.DocumentDecision
                && x.TaskStateId == WorkTaskConstants.States.Accepted)
                .ToListAsync();

            foreach (var item in myRouteTasks)
            {
                await workTaskService.CompleteTask(item);
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
                                    DecisionName = x.DecisionType.Label,
                                    DecisionRequestTypeName = x.DecisionRequestType.Label,
                                }).AsQueryable();
        }

        public IQueryable<DocumentDecisionCaseListVM> DocumentDecisionCaseByCase_Select(int CaseId)
        {
            return repo.AllReadonly<DocumentDecisionCase>()
                                .Where(x => x.CaseId == CaseId)
                                .Select(x => new DocumentDecisionCaseListVM
                                {
                                    Id = x.Id,
                                    CaseRegNumber = x.Case.RegNumber,
                                    CaseRegDate = x.Case.RegDate,
                                    DecisionName = x.DecisionType.Label,
                                    DecisionRequestTypeName = x.DecisionRequestType.Label,
                                    DocumentLable = x.DocumentDecision.Document.DocumentType.Label + " " + x.DocumentDecision.Document.DocumentNumber + "/" + x.DocumentDecision.Document.DocumentDate.ToString("dd.MM.yyyy"),
                                    DocumentShortLable = x.DocumentDecision.Document.DocumentNumber + "/" + x.DocumentDecision.Document.DocumentDate.ToString("dd.MM.yyyy"),
                                    DocumentId = x.DocumentDecision.DocumentId
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
                    saved.CaseId = model.CaseId;
                    saved.Description = model.Description;
                    saved.DecisionRequestTypeId = model.DecisionRequestTypeId;
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
                logger.LogError(ex, $"Грешка при запис на DocumentDecisionCase Id={model.Id}");
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
                                        .AsSplitQuery()
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
                        Id = adr.AddressId.ToString(),
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
                                        .AsSplitQuery()
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
                    docPerson.PersonRoleLabel = person.PersonRole.Label;
                    docPerson.MilitaryRangId = person.MilitaryRangId;
                    docPerson.PersonMaturityId = person.PersonMaturityId;
                    foreach (var pAdr in person.Addresses)
                    {
                        var searchAdr = searchPerson.Addresses.FirstOrDefault(sp => sp.Id == pAdr.AddressId.ToString());
                        if (searchAdr != null)
                        {
                            var docPersonAddress = new DocumentPersonAddressVM()
                            {
                                PersonIndex = docPerson.Index,
                                Index = docPerson.Addresses.Count
                            };
                            docPersonAddress.Address.CopyFrom(pAdr.Address);
                            docPerson.Addresses.Add(docPersonAddress);
                        }
                    }
                    result.Add(docPerson);
                }
            }
            return result;
        }

        /// <summary>
        /// Метод извличащ данни за справка съпровождащи документи
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<DocumentCaseInfoSprVM> DocumentCaseInfoSpr_Select(DocumentCaseInfoSprFilterVM filter)
        {
            filter.DateFrom = filter.DateFrom.ForceStartDateWithAddYear(-100);
            filter.DateTo = filter.DateTo.ForceEndDateWithAddYear(100);

            DateTime dateNow = DateTime.Now;
            DateTime dateEnd = DateTime.Now.AddYears(100);

            Expression<Func<DocumentCaseInfo, bool>> iDocumentGroupIdWhere = i => true;
            Expression<Func<DocumentCaseInfo, bool>> iDocumentTypeIdWhere = i => true;
            Expression<Func<CaseSessionFastDocument, bool>> fSessionDocTypeIdWhere = i => true;

            if (filter.DocumentGroupId > 0)
                iDocumentGroupIdWhere = i => i.Document.DocumentGroupId == filter.DocumentGroupId;

            if (filter.DocumentTypeId > 0)
                iDocumentTypeIdWhere = i => i.Document.DocumentTypeId == filter.DocumentTypeId;

            if ((filter.DocumentGroupId > 0 || filter.DocumentTypeId > 0) && filter.SessionDocTypeId < 1)
                fSessionDocTypeIdWhere = i => i.SessionDocTypeId == filter.SessionDocTypeId;

            if (filter.SessionDocTypeId > 0)
                fSessionDocTypeIdWhere = i => i.SessionDocTypeId == filter.SessionDocTypeId;

            if (filter.SessionDocTypeId > 0 && filter.DocumentGroupId < 1 && filter.DocumentTypeId < 1)
            {
                iDocumentGroupIdWhere = i => i.Document.DocumentGroupId == filter.DocumentGroupId;
                iDocumentTypeIdWhere = i => i.Document.DocumentTypeId == filter.DocumentTypeId;
            }

            Expression<Func<DocumentCaseInfo, bool>> iCaseGroupIdWhere = i => true;
            if (filter.CaseGroupId > 0)
                iCaseGroupIdWhere = i => i.Case.CaseGroupId == filter.CaseGroupId;

            Expression<Func<DocumentCaseInfo, bool>> iCaseTypeIdWhere = i => true;
            if (filter.CaseTypeId > 0)
                iCaseTypeIdWhere = i => i.Case.CaseTypeId == filter.CaseTypeId;

            Expression<Func<DocumentCaseInfo, bool>> iCaseCodeIdWhere = i => true;
            if (filter.CaseCodeId > 0)
                iCaseCodeIdWhere = i => i.Case.CaseCodeId == filter.CaseCodeId;

            Expression<Func<CaseSessionFastDocument, bool>> fCaseGroupIdWhere = i => true;
            if (filter.CaseGroupId > 0)
                fCaseGroupIdWhere = i => i.Case.CaseGroupId == filter.CaseGroupId;

            Expression<Func<CaseSessionFastDocument, bool>> fCaseTypeIdWhere = i => true;
            if (filter.CaseTypeId > 0)
                fCaseTypeIdWhere = i => i.Case.CaseTypeId == filter.CaseTypeId;

            Expression<Func<CaseSessionFastDocument, bool>> fCaseCodeIdWhere = i => true;
            if (filter.CaseCodeId > 0)
                fCaseCodeIdWhere = i => i.Case.CaseCodeId == filter.CaseCodeId;

            Expression<Func<DocumentCaseInfo, bool>> iCaseCodeIdsWhere = x => true;
            Expression<Func<CaseSessionFastDocument, bool>> fCaseCodeIdsWhere = x => true;
            if (filter.CaseCodeIds != null && filter.CaseCodeIds.Any())
            {
                int[] caseCodeIds = filter.CaseCodeIds.Select(x => int.Parse(x)).ToArray();
                fCaseCodeIdsWhere = x => caseCodeIds.Contains(x.Case.CaseCodeId ?? 0);
                iCaseCodeIdsWhere = x => caseCodeIds.Contains(x.Case.CaseCodeId ?? 0);
            }

            var caseSessionDocQuerry = repo.AllReadonly<CaseSessionDoc>();

            var queryDCI = repo.AllReadonly<DocumentCaseInfo>()
                                .Where(i => i.CourtId == userContext.CourtId &&
                                            i.Document.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.CompliantDocument &&
                                            i.Document.DocumentDate >= filter.DateFrom &&
                                            i.Document.DocumentDate <= filter.DateTo)
                                .Where(iDocumentGroupIdWhere)
                                .Where(iDocumentTypeIdWhere)
                                .Where(iCaseGroupIdWhere)
                                .Where(iCaseTypeIdWhere)
                                .Where(iCaseCodeIdWhere)
                                .Where(iCaseCodeIdsWhere)
                                .Select(i => new DocumentCaseInfoSprVM()
                                {
                                    DocumentNumberYear = i.Document.DocumentNumber + "/" + i.Document.DocumentDate.Date.Year + "г.",
                                    DocumentDate = i.Document.DocumentDate,
                                    DocumentTypeLabel = i.Document.DocumentType.Label,
                                    CaseInfo = (i.CaseId != null) ? i.Case.CaseType.Code + " " + i.Case.RegNumber : string.Empty,
                                    CaseId = (i.CaseId != null) ? i.Case.Id : (int?)null,
                                    IsCase = (i.CaseId != null),
                                    CaseCodeLabel = (i.CaseId != null) ? i.Case.CaseCode.Code + " " + i.Case.CaseCode.Label : string.Empty,
                                    CaseDocumentInfo = (i.CaseId != null) ? i.Case.Document.DocumentType.Label + " " + i.Case.Document.DocumentNumber + "/" + i.Case.Document.DocumentDate.Date.Year + "г." : string.Empty,
                                    CaseSessionInfo = caseSessionDocQuerry.Where(d => d.DateExpired == null &&
                                                                                      d.DocumentId == i.DocumentId)
                                                                          .Select(d => d.CaseSession.SessionType.Label + " " + d.CaseSession.DateFrom.ToString("dd.MM.yyyy"))
                                                                          .FirstOrDefault(),
                                    JudgeReport = i.Case.CaseLawUnits.Where(a => a.CaseSessionId == null &&
                                                                                 (a.DateTo ?? dateEnd).Date >= dateNow.Date &&
                                                                                 a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                                     .Select(a => a.LawUnit.FullName)
                                                                     .FirstOrDefault()
                                })
                                .AsQueryable();

            var queryCFD = repo.AllReadonly<CaseSessionFastDocument>()
                                  .Where(f => f.CourtId == userContext.CourtId &&
                                              f.CaseSession.DateFrom >= filter.DateFrom && f.CaseSession.DateFrom <= filter.DateTo)
                                  .Where(fSessionDocTypeIdWhere)
                                  .Where(fCaseGroupIdWhere)
                                  .Where(fCaseTypeIdWhere)
                                  .Where(fCaseCodeIdWhere)
                                  .Where(fCaseCodeIdsWhere)
                                  .Select(f => new DocumentCaseInfoSprVM()
                                  {
                                      DocumentNumberYear = f.CaseSession.DateFrom.Year.ToString() + "г.",
                                      DocumentDate = f.CaseSession.DateFrom,
                                      DocumentTypeLabel = f.SessionDocType.Label,
                                      CaseId = f.Case.Id,
                                      IsCase = true,
                                      CaseInfo = f.Case.CaseType.Code + " " + f.Case.RegNumber,
                                      CaseCodeLabel = f.Case.CaseCode.Code + " " + f.Case.CaseCode.Label,
                                      CaseDocumentInfo = f.Case.Document.DocumentType.Label + " " + f.Case.Document.DocumentNumber + "/" + f.Case.Document.DocumentDate.Date.Year + "г.",
                                      CaseSessionInfo = f.CaseSession.SessionType.Label + " " + f.CaseSession.DateFrom.ToString("dd.MM.yyyy"),
                                      JudgeReport = f.CaseSession.CaseLawUnits.Where(l => l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter &&
                                                                                          (l.DateTo ?? dateNow.AddYears(100)) >= f.CaseSession.DateFrom)
                                                                              .OrderByDescending(l => l.DateFrom)
                                                                              .Select(l => l.LawUnit.FullName)
                                                                              .FirstOrDefault(),
                                  })
                                  .AsQueryable();

            return Enumerable.Concat(queryDCI, queryCFD).AsQueryable();
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

        public async Task<bool> DocumentExpire(ExpiredInfoVM model)
        {
            try
            {
                using (var transaction = repo.BeginTransaction())
                {
                    var saved = await this.ReadByIdAsync<Document>(model.LongId);

                    if (saved != null)
                    {
                        saved.DateExpired = DateTime.Now;
                        saved.UserExpiredId = userContext.UserId;
                        saved.DescriptionExpired = model.DescriptionExpired;

                        var docTasks = await repo.All<WorkTask>()
                                                 .Where(x => x.SourceType == SourceTypeSelectVM.Document && x.SourceId == model.LongId)
                                                 .ToListAsync();

                        if (docTasks.Any())
                        {
                            foreach (var task in docTasks)
                            {
                                task.TaskStateId = WorkTaskConstants.States.Deleted;
                            }
                        }

                        var documentKindId = await repo.GetPropByIdAsync<DocumentGroup, int>(x => x.Id == saved.DocumentGroupId, x => x.DocumentKindId);
                        if (documentKindId == DocumentConstants.DocumentKind.CompliantDocument)
                        {
                            var caseSessionDocs = await repo.All<CaseSessionDoc>()
                                                      .Where(x => x.DocumentId == model.LongId &&
                                                                  x.DateExpired == null)
                                                      .ToListAsync() ?? new List<CaseSessionDoc>();

                            foreach (var caseSessionDoc in caseSessionDocs)
                            {
                                caseSessionDoc.DateExpired = DateTime.Now;
                                caseSessionDoc.UserExpiredId = userContext.UserId;
                                caseSessionDoc.DescriptionExpired = model.DescriptionExpired;
                            }

                            var caseSessionActComplains = await repo.All<CaseSessionActComplain>()
                                                              .Where(x => x.ComplainDocumentId == model.LongId &&
                                                                          x.DateExpired == null)
                                                              .ToListAsync() ?? new List<CaseSessionActComplain>();

                            foreach (var caseSessionActComplain in caseSessionActComplains)
                            {
                                caseSessionActComplain.DateExpired = DateTime.Now;
                                caseSessionActComplain.UserExpiredId = userContext.UserId;
                                caseSessionActComplain.DescriptionExpired = model.DescriptionExpired;
                            }
                        }

                        //Ако документа е към темплейт се освобождава и от там
                        var docTemplate = await repo.All<DocumentTemplate>().Where(x => x.DocumentId == model.LongId).FirstOrDefaultAsync();
                        if (docTemplate != null)
                        {
                            docTemplate.DocumentId = null;

                            switch (docTemplate.SourceType)
                            {
                                case SourceTypeSelectVM.CaseMigration:
                                    var caseMigration = await ReadByIdAsync<CaseMigration>((int)docTemplate.SourceId);
                                    if (caseMigration != null)
                                    {
                                        caseMigration.OutDocumentId = null;
                                    }
                                    break;
                            }
                        }

                        await repo.SaveChangesAsync();

                        await epepService.AppendDocument(saved, EpepConstants.ServiceMethod.Delete);

                        if (NomenclatureConstants.DocumentGroup.N24.Contains(saved.DocumentGroupId))
                            await workNotificationService.TurnOfNotificationsForN24(saved.Id);

                        transaction.Commit();
                        return true;
                    }

                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при премахване на документ Id={model.LongId}");

            }
            return false;
        }

        /// <summary>
        /// Връща списък на всички деловодни регистратури, до които служителя има достъп
        /// </summary>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetDocumentRegistratures(bool appendallItem = false)
        {
            int[] userDocRegs = userContext.SubDocRegistry;

            if (userDocRegs == null || !userDocRegs.Any())
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

            var result = (await repo.AllReadonly<CourtOrganization>()
                            .Where(x => x.CourtId == userContext.CourtId)
                            .Where(x => x.DateFrom <= DateTime.Now && (x.DateTo ?? DateTime.MaxValue) >= DateTime.Now)
                            .Where(docRegSearch)
                            .Select(x => new
                            {
                                x.Id,
                                x.Label
                            }).ToListAsync())
                            .ToSelectList(x => x.Id, x => x.Label);

            if (appendallItem)
            {
                result = result.Prepend(new SelectListItem("Избери", "-1")).ToList();
            }
            return result;
        }


        public async Task<List<SelectListItem>> GetDDL_DocumentRequestTypes(bool appendallItem = false)
        {
            var result = (await repo.AllReadonly<DocumentRequestType>()
                            .Where(x => x.InitRequestCode == null)
                            .Select(x => new
                            {
                                x.Id,
                                x.Label
                            }).ToListAsync())
                            .ToSelectList(x => x.Id, x => x.Label);

            if (appendallItem)
            {
                result = result.Prepend(new SelectListItem("Избери", "-1")).ToList();
            }
            return result;
        }

        public async Task<bool> Reactivate(DocumentReactivateVM model)
        {
            if (model.Id == 0)
            {
                Expression<Func<Document, bool>> whereCourt = x => x.CourtId == userContext.CourtId;
                if (model.IsCRdocument)
                {
                    whereCourt = x => x.CreatedCourtId == userContext.CourtId && x.CourtId == NomenclatureConstants.Courts.RandomAssignment;
                }


                var info = await repo.AllReadonly<Document>()
                                    .Where(whereCourt)
                                    .Where(x => x.DocumentDirectionId == model.DocumentDirectionId)
                                    .Where(x => x.DocumentNumber == model.DocumentNumber && x.DocumentDate.Date == model.DocumentDate.Date)
                                    .Where(x => x.DateExpired != null)
                                    .Select(x => new
                                    {
                                        Id = x.Id,
                                        Info = $"{x.DocumentGroup.Label}\\{x.DocumentType.Label} {model.DocumentNumber} от {model.DocumentDate:dd.MM.yyyy}"
                                    }).FirstOrDefaultAsync();

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
                var expired = await repo.All<Document>()
                                        .Include(x => x.DocumentCaseInfo)
                                        .Include(x => x.DocumentPersons)
                                        .Where(x => x.Id == model.Id)
                                        .AsSplitQuery()
                                        .FirstOrDefaultAsync();

                if (expired != null)
                {
                    expired.DateExpired = null;
                    expired.UserExpiredId = null;
                    expired.DescriptionExpired = null;
                    await repo.SaveChangesAsync();

                    try
                    {
                        await epepService.AppendDocument(expired, EpepConstants.ServiceMethod.Add);
                    }
                    catch { }

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
                logger.LogError(ex, $"Грешка при запис на интервал по дело Id={model.Id}");
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
                                 InstitutionName = x.Institution.FullName,
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
                repo.SaveChanges();
                return true;
            }

            return false;
        }

        private void document_UpdateRegix(DocumentVM model, Document document)
        {
            //Ъпдейт на заявките към Regix
            if (string.IsNullOrEmpty(model.RegixRequestReason.RegixReasonGuid) == false)
            {
                document.RegixReports = repo.All<RegixReport>()
                                .Where(x => x.DocumentId == null &&
                                       x.RegixGuid == model.RegixRequestReason.RegixReasonGuid).ToList();
                foreach (var item in document.RegixReports)
                {
                    item.Description = null;
                }
                //repo.UpdateRange(document.RegixReports);
            }
        }

        public SaveResultVM CheckCanExpireDocument(long id)
        {
            SaveResultVM result = new SaveResultVM(true);

            if (repo.AllReadonly<DocumentResolution>().Where(x => x.DocumentId == id && x.DateExpired != null).Any())
            {
                result.Result = false;
                result.ErrorMessage = "По документа има издадено разпореждане";
                return result;
            }
            if (repo.AllReadonly<DocumentDecision>().Where(x => x.DocumentId == id).Any())
            {
                result.Result = false;
                result.ErrorMessage = "По документа има издадено решение";
                return result;
            }
            if (repo.AllReadonly<Case>().Where(x => x.DocumentId == id && x.RegNumber != null).Any())
            {
                result.Result = false;
                result.ErrorMessage = "По документа има образувано дело";
                return result;
            }
            if (repo.AllReadonly<Obligation>().Where(x => x.DocumentId == id && (x.IsActive ?? true) == true).Any())
            {
                result.Result = false;
                result.ErrorMessage = "По документа има активни суми";
                return result;
            }

            return result;
        }

        public IQueryable<DocumentInfoVM> DocumentsOtherFromSameCourtByCaseId_Select(int CaseId)
        {
            var listCaseIdByCaseInfo = repo.AllReadonly<Case>()
                                           .Where(x => x.Id == CaseId)
                                           .SelectMany(x => x.Document.DocumentCaseInfo.Where(b => (b.CaseId ?? 0) > 0).Select(b => b.CaseId))
                                           .ToList();

            var caseCase = repo.GetById<Case>(CaseId);

            if (listCaseIdByCaseInfo == null)
                return (new List<DocumentInfoVM>()).AsQueryable();

            if (!listCaseIdByCaseInfo.Any())
                return (new List<DocumentInfoVM>()).AsQueryable();

            int _court = userContext.CourtId;
            return repo.AllReadonly<DocumentCaseInfo>()
                       .Where(x => x.Document.CourtId == _court &&
                                   listCaseIdByCaseInfo.Contains(x.CaseId) &&
                                   x.Document.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.InitialDocument &&
                                   x.Document.DateExpired == null &&
                                   x.Document.Id != caseCase.DocumentId)
                       .Select(x => new DocumentInfoVM
                       {
                           Id = x.Document.Id,
                           Title = $"Вх.№ {x.Document.DocumentNumber}/{x.Document.DocumentDate:dd.MM.yyyy} {x.Document.DocumentType.Label}",
                           DirectionId = x.Document.DocumentDirectionId,
                           IsSecret = (x.Document.IsSecret ?? false),
                           IsRestriction = x.Document.IsRestictedAccess,
                           DocumentResolutions = x.Document.DocumentResolutions.Where(c => c.DateExpired == null && c.ResolutionStateId == NomenclatureConstants.ResolutionStates.Enforced).Select(c => new DocumentResolutionListVM() { Id = c.Id, ResolutionTypeLabel = c.ResolutionType.Label, Label = c.ResolutionType.Label + " " + c.RegNumber + "/" + (c.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy"), RegDate = c.RegDate, RegNumber = c.RegNumber }).ToList(),
                           DocumentDate = x.Document.DocumentDate
                       }).AsQueryable();
        }

        public IQueryable<DocumentInfoVM> DocumentsOtherFromDifferentCourtByCaseId_Select(int CaseId)
        {
            int _court = userContext.CourtId;
            return repo.AllReadonly<DocumentCaseInfo>()
                       .Where(x => x.Document.CourtId != _court &&
                                   x.CaseId == CaseId &&
                                   x.Document.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.InitialDocument &&
                                   x.Document.DateExpired == null)
                       .Select(x => new DocumentInfoVM
                       {
                           Id = x.Document.Id,
                           Title = $"Вх.№ {x.Document.DocumentNumber}/{x.Document.DocumentDate:dd.MM.yyyy} {x.Document.DocumentType.Label}",
                           DirectionId = x.Document.DocumentDirectionId,
                           IsSecret = (x.Document.IsSecret ?? false),
                           IsRestriction = x.Document.IsRestictedAccess,
                           DocumentResolutions = x.Document.DocumentResolutions.Where(c => c.DateExpired == null && c.ResolutionStateId == NomenclatureConstants.ResolutionStates.Enforced).Select(c => new DocumentResolutionListVM() { Id = c.Id, ResolutionTypeLabel = c.ResolutionType.Label, Label = c.ResolutionType.Label + " " + c.RegNumber + "/" + (c.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy"), RegDate = c.RegDate, RegNumber = c.RegNumber }).ToList(),
                           DocumentDate = x.Document.DocumentDate,
                           CourtId = x.Document.CourtId,
                           CourtLabel = x.Document.Court.Label
                       }).AsQueryable();
        }

        public List<SelectListItem> GetCompliantDocumentsByCaseId(int caseId, bool addInitDoc = false)
        {
            var result = new List<SelectListItem>();

            result.AddRange(repo.AllReadonly<DocumentCaseInfo>()
                             .Where(x => x.CaseId == caseId)
                             .Where(x => x.Document.DateExpired == null && x.Document.CourtId == userContext.CourtId)
                             .Where(x => x.Document.DocumentDirectionId == DocumentConstants.DocumentDirection.Incoming)
                             .Select(x => x.Document)
                             .OrderByDescending(x => x.Id)
                             .Select(x => new SelectListItem
                             {
                                 Value = x.Id.ToString(),
                                 Text = $"{x.DocumentType.Label} {x.DocumentNumber}/{x.DocumentDate:dd.MM.yyyy}"
                             }).ToList());

            if (addInitDoc)
            {
                result.AddRange(repo.AllReadonly<Case>()
                             .Where(x => x.Id == caseId)
                             .Select(x => x.Document)
                             .OrderByDescending(x => x.Id)
                             .Select(x => new SelectListItem
                             {
                                 Value = x.Id.ToString(),
                                 Text = $"{x.DocumentType.Label} {x.DocumentNumber}/{x.DocumentDate:dd.MM.yyyy}"
                             }));
            }
            return result;

        }

        public async Task<List<SelectListItem>> GetCompliantDocumentsByCaseIdAsync(int caseId, bool addInitDoc = false)
        {
            var result = new List<SelectListItem>();

            result.AddRange(await repo.AllReadonly<DocumentCaseInfo>()
                                      .Where(x => x.CaseId == caseId)
                                      .Where(x => x.Document.DateExpired == null && x.Document.CourtId == userContext.CourtId)
                                      .Where(x => x.Document.DocumentDirectionId == DocumentConstants.DocumentDirection.Incoming)
                                      .Select(x => x.Document)
                                      .OrderByDescending(x => x.Id)
                                      .Select(x => new SelectListItem
                                      {
                                          Value = x.Id.ToString(),
                                          Text = $"{x.DocumentType.Label} {x.DocumentNumber}/{x.DocumentDate:dd.MM.yyyy}"
                                      })
                                      .ToListAsync());

            if (addInitDoc)
            {
                result.AddRange(await repo.AllReadonly<Case>()
                                          .Where(x => x.Id == caseId)
                                          .Select(x => x.Document)
                                          .OrderByDescending(x => x.Id)
                                          .Select(x => new SelectListItem
                                          {
                                              Value = x.Id.ToString(),
                                              Text = $"{x.DocumentType.Label} {x.DocumentNumber}/{x.DocumentDate:dd.MM.yyyy}"
                                          })
                                          .ToListAsync());
            }

            return result;

        }

        public List<SelectListItem> GetDocumentPersonsByDocumentId(long documentId)
        {
            return repo.AllReadonly<DocumentPerson>()
                            .Where(x => x.DocumentId == documentId)
                            .OrderBy(x => x.FullName)
                            .Select(x => new SelectListItem
                            {
                                Value = x.Id.ToString(),
                                Text = $"{x.FullName} - {x.PersonRole.Label}"
                            }).ToList();
        }

        public List<SelectListItem> GetDocumentPersonsByDocumentIdWithIdName(long documentId)
        {
            var result = repo.AllReadonly<DocumentPerson>()
                            .Where(x => x.DocumentId == documentId)
                            .OrderBy(x => x.FullName)
                            .Select(x => new SelectListItem
                            {
                                Value = x.FullName,
                                Text = $"{x.FullName} - {x.PersonRole.Label}"
                            }).ToList();

            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });

            return result;
        }

        public IQueryable<ElectronicDocumentNewVM> GetElectronicDocumentNew()
        {

            var elDocs = repo.AllReadonly<ElectronicDocument>();

            var transactionRepo = repo.AllReadonly<MainGroup>()
                                    .Where(x => x.SourceType == SourceTypeSelectVM.ElectronicDocument)
                                    .Where(x => x.CourtId == userContext.CourtId)
                                    .Where(x => x.LastTransationId == null || NomenclatureConstants.MainTransactionTypes.NotFinished.Contains(x.LastTransation.OperationTypeId))
                                    .Select(x => new
                                    {
                                        x.SourceId,
                                        x.LastTransation.OperationTypeId,
                                        EditBy = (x.LastTransationId != null && x.LastTransation.UserId != null) ? x.LastTransation.User.LawUnit.FullName : (string)null
                                    });

            long[] docIds = transactionRepo.Select(x => x.SourceId).ToArray();

            return repo.AllReadonly<ElectronicDocument>()
                                    .Where(x => docIds.Contains(x.Id))
                                    .OrderBy(x => x.Id)
                                    .Select(x => new ElectronicDocumentNewVM
                                    {
                                        Id = x.Id,
                                        DocumentKind = x.DocumentGroup.DocumentKind.Label,
                                        DocumentGroup = x.DocumentGroup.Label,
                                        ApplyDate = x.ApplyDate,
                                        ApplyNumber = x.ApplyNumber,
                                        EpepUserName = x.EpepUser.FullName,
                                        EditBy = transactionRepo.Where(t => t.SourceId == x.Id).Select(t => t.EditBy).FirstOrDefault()
                                    });

            //var docIds = result.Select(x => x.Id).ToArray();

            //var mainGroups = await repo.AllReadonly<MainGroup>()
            //                        .Where(x => x.SourceType == SourceTypeSelectVM.ElectronicDocument && docIds.Contains(x.SourceId))
            //                        .Select(x => new
            //                        {
            //                            x.SourceId,
            //                            User = (x.LastTransationId > 0) ? x.LastTransation.User.LawUnit.FullName : (string)null
            //                        }).ToListAsync().ConfigureAwait(false);

            //return result.AsQueryable();
        }

        public async Task<long?> GetDocumentRequestTypeId(long documentId)
        {
            return await repo.AllReadonly<Document>()
                                .Where(x => x.Id == documentId)
                                .Select(x => x.DocumentRequestTypeId)
                                .FirstOrDefaultAsync();
        }

        public Task<List<long>> GetAssignedDocumentList(long documentId)
        {
            return repo.AllReadonly<Document>().Where(x => x.AssignmentDocumentId == documentId).Select(x => x.Id).ToListAsync();
        }

        public async Task<AssignedDocumentInfoVM> GetAssignedDocumentsInfo(long documentId)
        {
            var result = await repo.AllReadonly<Document>()
                                    .Where(x => x.Id == documentId)
                                    .Select(x => new AssignedDocumentInfoVM
                                    {
                                        CreateCourtId = x.CreatedCourtId,
                                        CreateCourtName = x.CreatedCourt.Label
                                    }).FirstOrDefaultAsync();
            result.Documents = await repo.AllReadonly<Document>()
                                        .Where(x => x.AssignmentDocumentId == documentId)
                                        .Select(x => new AssignedDocumentVM
                                        {
                                            DocumentId = x.Id,
                                            DocumentNumber = $"{x.DocumentNumber}/{x.DocumentDate:dd.MM.yyyy}",
                                            CourtName = x.Court.Label,
                                            CaseId = x.Cases.Select(c => c.Id).FirstOrDefault(),
                                            CaseNumber = x.Cases.Select(c => c.RegNumber).FirstOrDefault(),
                                        }).ToListAsync();
            return result;
        }
    }
}
