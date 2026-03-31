// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Extensions;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.Integrations.EpepFastProcess;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Services;
using IOWebApplicationService.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using RestEpep = IOWebApplication.Infrastructure.Models.Integrations.EpepRest;

namespace IOWebApplicationService.Infrastructure.Services
{

    public class EpepDocumentService : BaseMQService, IEpepDocumentService
    {
        private readonly IEpepRestClient epepRestClient;
        private readonly IDocumentService documentService;
        private readonly ILazybleService<IDocumentRequestService> lazyDocumentRequestService;
        private readonly ICaseService caseService;
        private readonly INomenclatureService nomenclatureService;
        private readonly IWorkNotificationService workNotificationService;
        private readonly ICaseDeadlineService caseDeadlineService;
        private readonly IFastProcessSelectionCourtService fastProcessSelectionCourtService;
        private readonly IPdfCreatorService pdfCreatorService;
        private readonly ICourtStampCertificateService stamptService;
        private readonly IMQEpepService mqEpepService;
        private readonly IWorkingDaysService workingDays;

        public EpepDocumentService(IEpepRestClient _epepRestClient,
                                   IRepository _repo,
                                   IDocumentService _documentService,
                                   ILazybleService<IDocumentRequestService> _lazyDocumentRequestService,
                                   ICaseService _caseService,
                                   INomenclatureService _nomenclatureService,
                                   ICdnService _cdnService,
                                   IWorkNotificationService _workNotificationService,
                                   ICaseDeadlineService _caseDeadlineService,
                                   IFastProcessSelectionCourtService _fastProcessSelectionCourtService,
                                   IPdfCreatorService _pdfCreatorService,
                                   ICourtStampCertificateService _stamptService,
                                   IMQEpepService _mqEpepService,
                                   IWorkingDaysService _workingDays,
                                   ILogger<EpepDocumentService> _logger)
        {
            epepRestClient = _epepRestClient;
            repo = _repo;
            documentService = _documentService;
            lazyDocumentRequestService = _lazyDocumentRequestService;
            caseService = _caseService;
            nomenclatureService = _nomenclatureService;
            cdnService = _cdnService;
            workNotificationService = _workNotificationService;
            caseDeadlineService = _caseDeadlineService;
            fastProcessSelectionCourtService = _fastProcessSelectionCourtService;
            pdfCreatorService = _pdfCreatorService;
            stamptService = _stamptService;
            workingDays = _workingDays;
            mqEpepService = _mqEpepService;
            logger = _logger;
            IntegrationTypeId = NomenclatureConstants.IntegrationTypes.EPEP;
        }

        public async Task Test()
        {
            long documentId = 15398;

            var fastProcessModel = (FastProcessRequestVM)(await lazyDocumentRequestService.Service.GetDocumentRequestById(documentId, 0));
            var cityCode = await lazyDocumentRequestService.Service.GetCityCodeByCompetencyBases417(documentId, fastProcessModel.CompetencyBase417?.CompetencyBaseCode);
            var findCourtByCurrentAddresResult = await getCourtIdByCityCode(cityCode);
            if (findCourtByCurrentAddresResult.Result)
            {
                var court = await repo.GetByIdAsync<Court>((int)findCourtByCurrentAddresResult.ObjectId);
            }
        }

        /// <summary>
        /// Разпределя случайно в съд документ от Единната регистратура
        /// </summary>
        public async Task FastProcessRegisterInCourt(int fetchCount)
        {
            //Разпределението не трябва да работи в почивни дни и официални празници
            if (!workingDays.IsWorkingDay(NomenclatureConstants.Courts.RandomAssignment, DateTime.Now))
            {
                return;
            }


            var mqList = await repo.All<MQEpep>()
                       .Where(x => x.IntegrationTypeId == NomenclatureConstants.IntegrationTypes.EpepDocuments)
                       .Where(x => x.SourceType == SourceTypeSelectVM.Document)
                       //.Where(x => EpepConstants.EpepDocumentMethods.RandomAssignmentMethods.Contains(x.MethodName))
                       .Where(x => x.IntegrationStateId == EpepConstants.IntegrationStates.New)
                       .Where(x => x.DateTransfered == null)
                       .OrderBy(x => x.Id)
                       .Take(fetchCount)
                       .ToListAsync();

            if (mqList.Count == 0)
            {
                if (fetchCount == 25)
                {
                    logger.LogError("Няма документи за разпределяне");
                }
                return;
            }

            // Група Централизирано разпределение на дела Заповедни производства
            var courtGroupFastProcessId = await repo.AllReadonly<CourtGroup>()
                                                    .Where(x => x.GroupKind == NomenclatureConstants.CourtGroupKinds.FastProcessCentral)
                                                    .Where(x => x.CaseGroupId == NomenclatureConstants.CaseGroups.GrajdanskoDelo)
                                                    .Where(x => x.DateFrom < DateTime.Now)
                                                    .Where(x => x.DateTo == null)
                                                    .Select(x => x.Id)
                                                    .FirstOrDefaultAsync();

            // Група централизирано разпределение по подсъдност на дела Заповедни производства
            var courtGroupFastProcess417_3610Id = await repo.AllReadonly<CourtGroup>()
                                                    .Where(x => x.GroupKind == NomenclatureConstants.CourtGroupKinds.FastProcessCentralDistributionJurisdiction)
                                                    .Where(x => x.CaseGroupId == NomenclatureConstants.CaseGroups.GrajdanskoDelo)
                                                    .Where(x => x.DateFrom < DateTime.Now)
                                                    .Where(x => x.DateTo == null)
                                                    .Select(x => x.Id)
                                                    .FirstOrDefaultAsync();

            bool resInitMonth = await fastProcessSelectionCourtService.CreateInitializationSelectionForCourtByDate(DateTime.Now);
            bool resSelection = await fastProcessSelectionCourtService.CheckIsCreatedSelectionDay(DateTime.Now);
            if (fetchCount == 25)
            {
                logger.LogError($"CheckIsCreatedSelectionDay : {resSelection}");
            }
            if (fetchCount == 25)
            {
                logger.LogError($"AddAfterZeroCourtForSelectionDat : {resSelection}");
            }



            foreach (var mqItem in mqList)
            {
                try
                {
                    this.startTime = DateTime.Now;
                    mqItem.OperName = string.Empty;
                    bool result = await assignDocumentInCourt(mqItem, courtGroupFastProcessId, courtGroupFastProcess417_3610Id);
                    if (!result)
                    {
                        UpdateMQ(mqItem, false);
                    }
                }
                catch (Exception ex)
                {
                    mqItem.ErrorDescription = $"ГРЕШКА: {mqItem.OperName} - {ex.Message}";
                    UpdateMQ(mqItem, false);
                }
            }
        }


        public async Task TestAssign(int assignCount)
        {
            var mqItem = await repo.AllReadonly<MQEpep>()
                       .Where(x => x.Id == 134458)
                       .FirstOrDefaultAsync();


            // Група Централизирано разпределение на дела Заповедни производства
            var courtGroupFastProcessId = await repo.AllReadonly<CourtGroup>()
                                                    .Where(x => x.GroupKind == NomenclatureConstants.CourtGroupKinds.FastProcessCentral)
                                                    .Where(x => x.CaseGroupId == NomenclatureConstants.CaseGroups.GrajdanskoDelo)
                                                    .Where(x => x.DateFrom < DateTime.Now)
                                                    .Where(x => x.DateTo == null)
                                                    .Select(x => x.Id)
                                                    .FirstOrDefaultAsync();

            // Група централизирано разпределение по подсъдност на дела Заповедни производства
            var courtGroupFastProcess417_3610Id = await repo.AllReadonly<CourtGroup>()
                                                    .Where(x => x.GroupKind == NomenclatureConstants.CourtGroupKinds.FastProcessCentralDistributionJurisdiction)
                                                    .Where(x => x.CaseGroupId == NomenclatureConstants.CaseGroups.GrajdanskoDelo)
                                                    .Where(x => x.DateFrom < DateTime.Now)
                                                    .Where(x => x.DateTo == null)
                                                    .Select(x => x.Id)
                                                    .FirstOrDefaultAsync();

            bool resInitMonth = await fastProcessSelectionCourtService.CreateInitializationSelectionForCourtByDate(DateTime.Now);
            bool resSelection = await fastProcessSelectionCourtService.CheckIsCreatedSelectionDay(DateTime.Now);



            for (int i=0;i<assignCount;i++)
            {
                try
                {
                    this.startTime = DateTime.Now;
                    mqItem.OperName = string.Empty;
                    bool result = await assignDocumentInCourt(mqItem, courtGroupFastProcessId, courtGroupFastProcess417_3610Id);
                    
                }
                catch (Exception ex)
                {
                    
                }
            }
        }


        public async Task FetchElectronicDocumentPayments()
        {
            epepRestClient.InitClient();

            var courtsSupported = await repo.AllReadonly<Court>()
                                            .Where(x => x.IsActive && (x.EpepHasElectronicDocuments == true))
                                            .Select(x => new BaseCommonNomenclature
                                            {
                                                Id = x.Id,
                                                Code = x.Code,
                                                Label = x.Label
                                            }).ToListAsync();

            var epepCourtMap = await repo.AllReadonly<CodeMapping>()
                                            .Where(x => x.Alias == EpepConstants.Nomenclatures.Courts)
                                            .Select(x => new
                                            {
                                                x.InnerCode,
                                                x.OuterCode
                                            }).ToListAsync();

            foreach (var item in courtsSupported)
            {
                item.Code = epepCourtMap.Where(x => x.InnerCode == item.Id.ToString()).Select(x => x.OuterCode).FirstOrDefault() ?? item.Code;
            }

            var documentPayments = new List<RestEpep.ElectronicDocumentPayment>();
            foreach (var court in courtsSupported)
            {
                var documentIdentifiersForCourt = await epepRestClient.GetElectronicDocumentPayments(court.Code);
                documentPayments.AddRange(documentIdentifiersForCourt);
            }

            if (documentPayments.Count == 0)
            {
                return;
            }

            var paymentTypeList = await repo.AllReadonly<PaymentType>()
                                            .Select(x => new BaseCommonNomenclature
                                            {
                                                Id = x.Id,
                                                Code = x.Code,
                                                Label = x.Label
                                            }).ToListAsync();

            foreach (var docPayment in documentPayments)
            {

                await saveElectronicDocumentPayments(docPayment, courtsSupported, paymentTypeList);
            }
        }

        async Task saveElectronicDocumentPayments(RestEpep.ElectronicDocumentPayment docPayment, List<BaseCommonNomenclature> courtList, List<BaseCommonNomenclature> paymentTypeList)
        {
            using (var ts = repo.BeginTransaction())
            {
                var elDoc = await repo.All<ElectronicDocument>()
                                      .Where(x => x.EpepId == docPayment.ElectronicDocumentId)
                                      .FirstOrDefaultAsync();

                if (elDoc == null)
                {
                    return;
                }

                if (elDoc.PaidDate != null)
                {
                    var updatePaidDocument = await epepRestClient.UpdateElectronicDocumentSetMoneyAccept(docPayment.ElectronicDocumentId, DateTime.Now);
                    return;
                }

                var paymentCourtId = courtList.Where(x => x.Code == docPayment.CourtCode).Select(x => x.Id).FirstOrDefault();
                var paymentTypeId = paymentTypeList.Where(x => x.Code == docPayment.PaymentKind.ToString()).Select(x => x.Id).FirstOrDefault();

                var documentInfo = await repo.AllReadonly<Document>().Where(x => x.ElectronicDocumentId == elDoc.Id).Select(x => new
                {
                    x.Id,
                    x.CourtId
                }).FirstOrDefaultAsync();

                if (documentInfo == null)
                {
                    //Все още няма документ по този електронен документ
                    //Не трябва да има такъв случай - Да има плащане по съд за който ЕИСС не знае
                    elDoc.TaxAmount = (decimal)docPayment.PaymentAmount / 100;
                    elDoc.CurrencyCode = docPayment.CurrencyCode;
                    elDoc.PaymentTypeId = paymentTypeId;
                    await repo.SaveChangesAsync();
                    ts.Commit();
                    var updBeforeDocument = await epepRestClient.UpdateElectronicDocumentSetMoneyAccept(docPayment.ElectronicDocumentId, DateTime.Now);
                    return;
                }

                if (documentInfo.CourtId != paymentCourtId)
                {
                    documentInfo = await repo.AllReadonly<Document>()
                                            .Where(x => x.AssignmentDocumentId == documentInfo.Id && x.CourtId == paymentCourtId)
                                            .OrderBy(x => x.Id)
                                            .Select(x => new
                                            {
                                                x.Id,
                                                x.CourtId
                                            }).FirstOrDefaultAsync();
                }

                if (documentInfo == null)
                {
                    //Все още няма определен съд
                    //Не трябва да има такъв случай - Да има плащане по съд за който ЕИСС не знае
                    return;
                }

                elDoc.TaxAmount = (decimal)docPayment.PaymentAmount / 100;
                elDoc.CurrencyCode = docPayment.CurrencyCode;
                elDoc.PaymentTypeId = paymentTypeId;
                elDoc.PaidDate = docPayment.DatePaid;
                await repo.SaveChangesAsync();

                var docModel = await repo.AllReadonly<Document>()
                                        //.Include(x => x.DocumentPersons)
                                        .Include(x => x.Cases)
                                        .Where(x => x.Id == documentInfo.Id)
                                        .AsSplitQuery()
                                        .FirstOrDefaultAsync();

                docModel.DocumentPersons = await repo.AllReadonly<DocumentPerson>()
                                                    .Where(x => x.DocumentId == documentInfo.Id)
                                                    .Where(x => x.PersonRole.RoleKindId == NomenclatureConstants.RoleKind.LeftSide)
                                                    .OrderBy(x => x.Id)
                                                    .Take(1).ToListAsync();

                var moneyResult = await documentService.FinishElectronicDocumentSaveMoney(elDoc.Id, docModel, docModel.DocumentPersons.FirstOrDefault());

                ts.Commit();
                var updResult = await epepRestClient.UpdateElectronicDocumentSetMoneyAccept(docPayment.ElectronicDocumentId, DateTime.Now);
            }
        }

        public async Task FetchExecProcessNewExecListDeliveryDate()
        {
            try
            {
                epepRestClient.InitClient();
                var newDates = await epepRestClient.SelectNewExecListDeliveryDates();
                foreach (var date in newDates)
                {
                    var integrationKey = await repo.AllReadonly<IntegrationKey>()
                                                    .Where(x => x.IntegrationTypeId == NomenclatureConstants.IntegrationTypes.EPEP)
                                                    .Where(x => x.OuterCode == date.ExecProcessGid.ToString())
                                                    .FirstOrDefaultAsync();

                    if (integrationKey != null)
                    {
                        int actSourceType = 0;
                        switch (integrationKey.SourceType)
                        {
                            case SourceTypeSelectVM.ExecProcessCaseSessionAct:
                                actSourceType = SourceTypeSelectVM.CaseSessionAct;
                                break;
                            case SourceTypeSelectVM.ExecProcessExecList:
                                actSourceType = SourceTypeSelectVM.ExecList;
                                break;
                            default:
                                break;
                        }
                        var updateResult = await epepRestClient.SetCourtAcceptListDeliveryDate(date.ExecCaseGid);
                        if (updateResult)
                        {
                            string description = $"{date.CaseNumber} ({date.CaseUser})";
                            await workNotificationService.SaveNotificationsForDeliveredЕxecutiveListFastProcess(actSourceType, integrationKey.SourceId, date.ListDeliveryDate, description);
                            //TODO: WorkNotification add here
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, nameof(FetchExecProcessNewExecListDeliveryDate));
            }
        }

        async Task<SaveResultVM> getCourtIdByCityCode(string cityCode)
        {
            if (string.IsNullOrEmpty(cityCode))
            {
                return new SaveResultVM(false);
            }

            var ekMunicipality = await repo.GetPropByIdAsync<EkEkatte, string>(x => x.Ekatte == cityCode, x => x.Munincipality.Municipality);

            var courtRegionId = await repo.AllReadonly<CourtRegionArea>()
                                          .Where(x => x.MunicipalityCode == ekMunicipality)
                                          .Select(x => x.CourtRegionId)
                                          .FirstOrDefaultAsync();

            var courtId = await repo.AllReadonly<Court>()
                                    .Where(x => x.CourtRegionId == courtRegionId)
                                    .Where(x => x.CourtTypeId == NomenclatureConstants.CourtType.RegionalCourt)
                                    .Select(x => x.Id)
                                    .FirstOrDefaultAsync();

            if (courtId > 0)
            {
                return new SaveResultVM(true)
                {
                    ObjectId = courtId
                };
            }
            else
            {
                return new SaveResultVM(false);
            }
        }

        /// <summary>
        /// Метод създаващ нотификейшъни за новообразувано дело
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task<SaveResultVM> initNotificationsForNewCaseFastProcess(int caseId)
        {
            try
            {
                List<WorkNotification> workNotifications = await workNotificationService.GetNotificationsForN1(caseId);

                if (workNotifications.Count > 0)
                {
                    repo.AddRange(workNotifications);
                    await repo.SaveChangesAsync();
                }

                return new(true);
            }
            catch (Exception ex)
            {
                return new(false, "Проблем при създаване на нотификация за новообразувано дело по чл. 410 ГПК или чл. 417 ГПК");
            }
        }

        /// <summary>
        /// Метод за създаване на срок за предприемане на действия по ново образувано дело по чл. 410 ГПК или чл. 417 ГПК
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task<SaveResultVM> initStartTakingActionFastProcess(int caseId)
        {
            try
            {
                bool result = await caseDeadlineService.StartTakingActionFastProcess(caseId);
                if (result)
                    await repo.SaveChangesAsync();

                return new(true);
            }
            catch (Exception ex)
            {
                return new(false, "Проблем при създаване на срок за Предприемане на действия по ново образувано дело по чл. 410 ГПК или чл. 417 ГПК");
            }
        }

        /// <summary>
        /// Разпределя един документ от Централна регистратура на избран съд/съдия
        /// </summary>
        /// <param name="documentId">Document.Id Идентификатор на документ от Регистратура Централизирано разпределение</param>
        /// <param name="courtGroupFastProcessId">Идентификатор на група Централизирано разпределение ГД</param>
        /// <returns></returns>
        async Task<bool> assignDocumentInCourt(MQEpep mqItem, int courtGroupFastProcessId, int courtGroupFastProcess417_3610Id)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            string times = "";

            long documentId = mqItem.SourceId;
            mqItem.OperName = "Document_GetById";
            var documentInCourt = await documentService.Document_GetById(documentId);
            int newCaseGroupId = courtGroupFastProcessId;
            int newCaseGroupKind = NomenclatureConstants.CourtGroupKinds.FastProcessCentral;

            using (var ts = repo.BeginTransaction())
            {
                int? caseMigrationId = null;
                CaseMigration caseMigrationAssign = null;

                if (EpepConstants.EpepDocumentMethods.RandomAssignmentMethods.Contains(mqItem.MethodName) && mqItem.ParentSourceId > 0)
                {
                    //При преразпределение на дело от движение, насрещния съд на движението се променя
                    caseMigrationId = (int)mqItem.ParentSourceId;
                    caseMigrationAssign = await repo.GetByIdAsync<CaseMigration>(caseMigrationId);
                }
                int preferedCourt = 0;


                mqItem.OperName = "Определяне на компетенция";
                bool getPreferedCourtFromRequest = false;
                //Ако разпределението е по движение - компетенцията се определя от вида движение
                if (caseMigrationAssign != null)
                {
                    switch (caseMigrationAssign.CaseMigrationTypeId)
                    {
                        //Разпределяне по компетенция сменя шифъра на новия документ/дело на 417 ал. 1, т 3, 6 и 10
                        case NomenclatureConstants.CaseMigrationTypes.SendCase_FromAssignmentByAddress:
                            //Само 417-ки при това движение си сменят групата по компетенция
                            if (documentInCourt.CaseCodeId == NomenclatureConstants.CaseCode.FP417
                                || documentInCourt.CaseCodeId == NomenclatureConstants.CaseCode.FP417_t3610)
                            {
                                preferedCourt = caseMigrationAssign.SendToCourtId ?? 0;
                                documentInCourt.CaseCodeId = NomenclatureConstants.CaseCode.FP417_t3610;
                                newCaseGroupKind = NomenclatureConstants.CourtGroupKinds.FastProcessCentralDistributionJurisdiction;
                                newCaseGroupId = courtGroupFastProcess417_3610Id;
                            }
                            break;
                        //Разпределяне по общата група 
                        case NomenclatureConstants.CaseMigrationTypes.SendCase_FromRandomAssignment:
                            if (documentInCourt.CaseCodeId == NomenclatureConstants.CaseCode.FP417_t3610)
                            {
                                documentInCourt.CaseCodeId = NomenclatureConstants.CaseCode.FP417;
                            }
                            newCaseGroupKind = NomenclatureConstants.CourtGroupKinds.FastProcessCentral;
                            newCaseGroupId = courtGroupFastProcessId;
                            break;
                        default:
                            break;
                    }

                }
                else
                {
                    getPreferedCourtFromRequest = true;
                }
                if (getPreferedCourtFromRequest)
                {
                    var requestService = lazyDocumentRequestService.Service;
                    requestService.InitDbEuro(this.DbEuroConfig);
                    var fastProcessModel = (FastProcessRequestVM)(await requestService.GetDocumentRequestById(documentId, 0, true));
                    //if (fastProcessModel == null)
                    //{
                    //    mqItem.ErrorDescription = $"Липсва заявление DocumentId:{documentId}";
                    //    UpdateMQ(mqItem, false);
                    //    return false;
                    //}
                    //При лица по чл51-52 се разрешава записване без данни
                    if (fastProcessModel != null)
                    {
                        if (fastProcessModel.RequestTypeCode == FastProcessRequestVM.FastProcess417)
                        {
                            times += ";417:";
                            if (fastProcessModel.CompetencyBase417 != null && fastProcessModel.CompetencyBase417.ForCompetencyBase)
                            {
                                var cityCode = await requestService.GetCityCodeByCompetencyBases417(documentId, fastProcessModel.CompetencyBase417?.CompetencyBaseCode);
                                var findCourtByCurrentAddresResult = await getCourtIdByCityCode(cityCode);
                                if (findCourtByCurrentAddresResult.Result)
                                {
                                    preferedCourt = (int)findCourtByCurrentAddresResult.ObjectId;
                                    documentInCourt.CaseCodeId = NomenclatureConstants.CaseCode.FP417_t3610;
                                    newCaseGroupKind = NomenclatureConstants.CourtGroupKinds.FastProcessCentralDistributionJurisdiction;
                                    newCaseGroupId = courtGroupFastProcess417_3610Id;
                                }
                                else
                                {
                                    times += $";417:nocourt-city:{cityCode}";
                                }
                            }
                        }
                    }
                    else
                    {
                        times += $";NO_REQUEST!!!;";
                    }
                }

                stopwatch.Stop();
                times += $";GetCourt:{stopwatch.ElapsedMilliseconds}ms";
                stopwatch.Restart();




                mqItem.OperName = "Определяне на съд";
                //Изтегля случайно разпределен съд, или подадения, ако е възможен
                int newCourtId = await fastProcessSelectionCourtService.GetSelectedAvailableCourtForFastSelection(preferedCourt, newCaseGroupKind);

                //TODO: Ако върне 0 - ко праим?
                if (newCourtId == 0)
                {
                    string courtName = await repo.GetPropByIdAsync<Court, string>(x => x.Id == preferedCourt, x => x.Label);
                    mqItem.ErrorDescription = (preferedCourt > 0) ? $"Невъзможно разпределение в съд по адрес CourtId:{preferedCourt}, {courtName}" : "Няма възможен съд";
                    return false;
                }

                if (caseMigrationAssign != null && caseMigrationAssign.CaseMigrationTypeId != NomenclatureConstants.CaseMigrationTypes.SendCase_FromAssignmentByAddress)
                {
                    caseMigrationAssign.SendToCourtId = newCourtId;
                }


                //Всички последващи документи в различните съдилища имат общ AssignmentDocumentId Document.Id на общия документ в Централна регистратура
                documentInCourt.AssignmentDocumentId = documentInCourt.Id;

                documentInCourt.Id = 0;
                foreach (var docPerson in documentInCourt.DocumentPersons)
                {
                    docPerson.Id = 0;
                    foreach (var addr in docPerson.Addresses)
                    {
                        addr.Id = 0;

                        addr.Address.Id = 0;
                    }
                }
                documentInCourt.CourtId = newCourtId;
                documentInCourt.DisableTransaction = true;


                mqItem.OperName = $"Запис на документ {documentId} в съд id {documentInCourt.CourtId}";
                var documentSaveResult = await documentService.Document_SaveData(documentInCourt);
                if (!documentSaveResult)
                {
                    mqItem.ErrorDescription = "Грешка при запис на документ в съд";
                    return false;
                }


                stopwatch.Stop();
                times += $";Doc:{stopwatch.ElapsedMilliseconds}ms";
                stopwatch.Restart();



                if (!documentInCourt.CaseId.HasValue)
                {
                    mqItem.ErrorDescription = "Грешка при запис на дело към документ в съд";
                    return false;
                }

                //TODO: Така ли ще остане?
                var firstLoadGroupId = nomenclatureService.GetDDL_LoadGroupLink(NomenclatureConstants.CourtType.RegionalCourt, documentInCourt.CaseTypeId.Value, documentInCourt.CaseCodeId.Value)
                                        .Where(x => x.Value != NomenclatureConstants.NullVal.ToString())
                                        .Select(x => x.Value)
                                        .FirstOrDefault();

                int loadGroupId = 0;

                if (!string.IsNullOrEmpty(firstLoadGroupId))
                {
                    loadGroupId = int.Parse(firstLoadGroupId);
                }
                if (loadGroupId == 0)
                {
                    mqItem.ErrorDescription = "Липсва група по натовареност";
                    return false;
                }

                var caseModel = await caseService.Case_SelectForEdit(documentInCourt.CaseId.Value);

                if (caseModel.CaseCharacterId <= 0)
                    caseModel.CaseCharacterId = NomenclatureConstants.CaseCharacters.PyrvaInstanciaGrajdanskoDelo;

                caseModel.CourtGroupId = newCaseGroupId;
                caseModel.LoadGroupLinkId = loadGroupId;
                caseModel.CaseStateId = NomenclatureConstants.CaseState.New;
                caseModel.ProcessPriorityId = DocumentConstants.ProcessPriority.ShortNoticeCase;

                caseModel.DisableTransaction = true;
                caseModel.RandomAssignmentOutCaseMigrationId = caseMigrationId;
                caseModel.CaseTypeUnitId = await repo.AllReadonly<CaseTypeUnit>()
                                                    .Where(x => x.CaseTypeId == caseModel.CaseTypeId)
                                                    .OrderBy(x => x.OrderNumber)
                                                    .Select(x => x.Id)
                                                    .FirstOrDefaultAsync();

                if (caseModel.CaseTypeUnitId == 0)
                {
                    caseModel.CaseTypeUnitId = null;
                }



                mqItem.OperName = $"Образуване на дело в съд {caseModel.CourtId}";
                var caseResult = await caseService.Case_SaveData(caseModel);
                if (!caseResult.Result)
                {
                    mqItem.ErrorDescription = "Грешка образуване на разпределено дело";
                    //TODO: Proper error handling
                    return false;
                }
                stopwatch.Stop();
                times += $";Case:{stopwatch.ElapsedMilliseconds}ms";
                stopwatch.Restart();

                caseModel = await caseService.Case_SelectForEdit(documentInCourt.CaseId.Value);

                mqItem.OperName = $"Разпределение на съдия в съд {caseModel.CourtId}";

                var assignmentSaveResult = await initCaseAssignment(caseModel);
                if (!assignmentSaveResult.Result)
                {
                    mqItem.ErrorDescription = assignmentSaveResult.ErrorMessage;
                    //TODO: Proper error handling
                    return false;
                }

                stopwatch.Stop();
                times += $";Assign:{stopwatch.ElapsedMilliseconds}ms|{assignmentSaveResult.Content}";
                stopwatch.Restart();

                mqItem.OperName = "Известия ново дело";
                // Метод създаващ нотификейшъни за новообразувано дело по чл. 410 ГПК или чл. 417 ГПК
                SaveResultVM resultNotifications = await initNotificationsForNewCaseFastProcess(caseModel.Id);
                if (!resultNotifications.Result)
                {
                    mqItem.ErrorDescription = resultNotifications.ErrorMessage;
                    return false;
                }
                mqItem.OperName = "Стартиране на срокове";
                // Създаване на срок за предприемане на действия по ново образувано дело по чл. 410 ГПК или чл. 417 ГПК
                SaveResultVM resultStartTakingActionFastProcess = await initStartTakingActionFastProcess(caseModel.Id);
                if (!resultStartTakingActionFastProcess.Result)
                {
                    mqItem.ErrorDescription = resultStartTakingActionFastProcess.ErrorMessage;
                    return false;
                }
                stopwatch.Stop();
                times += $";Other:{stopwatch.ElapsedMilliseconds}ms";
                mqItem.ErrorDescription = times;

                UpdateMQ(mqItem, true);
                ts.Commit();
            }
            return true;
        }

        private async Task<DocumentVM> initDocumentFromElectronic(long electronicDocumentId)
        {
            var documentModel = await documentService.Document_Init(DocumentConstants.DocumentDirection.Incoming, 0, electronicDocumentId);
            var electronicDocument = await repo.GetByIdAsync<ElectronicDocument>(electronicDocumentId);

            var initResult = await documentService.InitializeDocumentVMFromRequest(documentModel, electronicDocument.RequestTypeCode);

            if (!initResult)
            {
                return null;
            }

            return documentModel;
        }

        private async Task<SaveResultVM> initCaseAssignment(CaseEditVM caseModel)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            string times = "";

            //Избира съдия от избрания съд и създава протокол за разпределение
            var protocolResult = await fastProcessSelectionCourtService.FastProcessCreateSelectionProtocol(caseModel.Id);

            if (!protocolResult.Result)
            {
                return new SaveResultVM(false, "Грешка при създаване на протокол за разпределение");
            }

            int protocolId = protocolResult.ProtocolId;

            //Данни за протокол и създаване на pdf файл
            var protocolModel = await fastProcessSelectionCourtService.FastProcessSelectionProtokol_Preview(protocolId);
            if (protocolModel == null || !protocolModel.SelectedLawUnitId.HasValue)
            {
                return new SaveResultVM(false, "Грешка при генериране на протокол за разпределение");
            }

            stopwatch.Stop();
            times += $";Prot:{stopwatch.ElapsedMilliseconds}ms";
            stopwatch.Restart();

            try
            {
                byte[] protocolPdf = await pdfCreatorService.RenderViewAsPDF("~/Views/FastProcessSelectionCourt/Preview.cshtml", protocolModel);

                stopwatch.Stop();
                times += $";Pdf:{stopwatch.ElapsedMilliseconds}ms";
                stopwatch.Restart();


                //Добавяне на печат на съда
                var stampResult = await stamptService.Stamp(new CourtStampRequestVM()
                {
                    CourtId = caseModel.CourtId,
                    PdfContent = protocolPdf,
                    StampContent = "Случайно разпределение",
                    SourceType = SourceTypeSelectVM.CaseSelectionProtokol,
                    BlankMode = 0,
                    WideStamp = false
                });

                if (!stampResult.Result)
                {
                    return new SaveResultVM(false, stampResult.StampError);
                }
                stopwatch.Stop();
                times += $";Stamper:{stopwatch.ElapsedMilliseconds}ms";
                stopwatch.Restart();

                //Запис на подпечатан съд
                var pdfRequest = new CdnUploadRequest()
                {
                    SourceId = protocolId.ToString(),
                    SourceType = SourceTypeSelectVM.CaseSelectionProtokol,
                    ContentType = System.Net.Mime.MediaTypeNames.Application.Pdf,
                    FileContent = stampResult.StampedPdfContent,
                    FileContentBase64 = Convert.ToBase64String(stampResult.StampedPdfContent),
                    FileName = "selectionProtokol.pdf",
                    Title = $"Протокол за разпределение {protocolModel.SelectedLawUnitName} ({protocolModel.JudgeRoleName})"
                };
                if (!await cdnService.MongoCdn_AppendUpdate(pdfRequest))
                {
                    return new SaveResultVM(false, "Грешка при запис на pdf протокол");
                }

                //Изпращане ан протокола за случайно разпределение към ЕПЕП и ЦСРД
                var savedProtocol = await repo.GetByIdAsync<CaseSelectionProtokol>(protocolId);
                if (savedProtocol.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                {
                    var judgeCaseLawunitId = await repo.AllReadonly<CaseLawUnit>()
                                                        .Where(x => x.CaseId == caseModel.Id && x.CaseSelectionProtokolId == protocolId)
                                                        .Select(x => x.Id)
                                                        .FirstOrDefaultAsync();
                    mqEpepService.AppendJudgeReporter(judgeCaseLawunitId, EpepConstants.ServiceMethod.Add);

                }
                mqEpepService.AppendCaseSelectionProtocol(savedProtocol, EpepConstants.ServiceMethod.Add, "ЕИСС - Централно Разпределяне");

                stopwatch.Stop();
                times += $";Final:{stopwatch.ElapsedMilliseconds}ms";

                await workNotificationService.CreateNotificationsAddCaseLawUnit(protocolResult.CaseLawunitId);

                return new SaveResultVM(true)
                {
                    Content = times
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Проблем при създаване на pdf протокол: {protocolId}");
                return new SaveResultVM(false, $"Грешка при създаване на протокол - {ex.Message}");
            }



        }


        public async Task FetchElectronicDocuments()
        {

            epepRestClient.InitClient();

            //var test = await epepRestClient.GetElectronicDocument(Guid.Parse("B66518B5-0670-4D8E-AB53-8D40F2167611"));
            //return;

            var courtsSupported = await repo.AllReadonly<Court>()
                                            .Where(x => x.IsActive && (x.EpepHasElectronicDocuments == true))
                                            .Select(x => new BaseCommonNomenclature
                                            {
                                                Id = x.Id,
                                                Code = x.Code,
                                                Label = x.Label
                                            }).ToListAsync();

            var epepCourtMap = await repo.AllReadonly<CodeMapping>()
                                            .Where(x => x.Alias == EpepConstants.Nomenclatures.Courts)
                                            .Select(x => new
                                            {
                                                x.InnerCode,
                                                x.OuterCode
                                            }).ToListAsync();

            foreach (var item in courtsSupported)
            {
                item.Code = epepCourtMap.Where(x => x.InnerCode == item.Id.ToString()).Select(x => x.OuterCode).FirstOrDefault() ?? item.Code;
            }

            List<Guid> documentIdentifiers = new List<Guid>();
            foreach (var court in courtsSupported)
            {
                var documentIdentifiersForCourt = await epepRestClient.GetNewElectronicDocumentIdentifiers(court.Code);
                documentIdentifiers.AddRange(documentIdentifiersForCourt);
            }

            if (documentIdentifiers.Count == 0)
            {
                return;
            }

            List<BaseCommonNomenclature> docKindList = new List<BaseCommonNomenclature>()
            {
                new BaseCommonNomenclature(){ Id = DocumentConstants.DocumentKind.InitialDocument,Code="1"},
                new BaseCommonNomenclature(){ Id = DocumentConstants.DocumentKind.CompliantDocument,Code="2"},
                new BaseCommonNomenclature(){ Id = DocumentConstants.DocumentKind.InAdministrationDocument,Code="3"}
            };
            var docGroupList = await repo.AllReadonly<DocumentGroup>().Where(x => x.Code != null).ToListAsync();

            var feeTypes = await repo.AllReadonly<MoneyFeeType>()
                                           .Where(x => x.IsActive)
                                           .Select(x => new BaseCommonNomenclature
                                           {
                                               Id = x.Id,
                                               Code = x.Code,
                                               Label = x.Label
                                           }).ToListAsync();

            foreach (var docId in documentIdentifiers)
            {

                var forLog = false;//docId.ToString().ToUpper() == "21C1DC7E-2D4E-4768-AD9C-10E516E347C3";

                RestEpep.ElectronicDocument epepDocData = null;
                try
                {

                    var savedElDoc = await repo.AllReadonly<ElectronicDocument>()
                                                .Where(x => x.EpepId == docId)
                                                .Where(x => x.DateCourtAccept != null)
                                                .Select(x => new { x.Id, x.DateCourtAccept })
                                                .FirstOrDefaultAsync();
                    if (savedElDoc != null)
                    {
                        //Ако изчетения документ вече е записан в electronic_document - само се отбелязва като прочетен в ЕПЕП
                        await epepRestClient.UpdateElectronicDocumentSetDateCourtAccept(docId, savedElDoc.DateCourtAccept.Value);

                        logger.LogError($"FetchElectronicDocument: ELDOC {docId} exists !!!");
                        continue;

                    }

                    epepDocData = await epepRestClient.GetElectronicDocument(docId);
                }
                catch (Exception ex)
                {
                    if (forLog)
                        logger.LogError("ELDOC.get; " + ex.Message);
                }
                if (epepDocData == null)
                {
                    if (forLog)
                        logger.LogError("ELDOC.get=null; ");
                    continue;
                }

                ElectronicDocument mappedDocument = await mapElectronicDocumentToEiss(forLog, epepDocData, courtsSupported, docKindList, docGroupList, feeTypes);

                if (mappedDocument != null && string.IsNullOrEmpty(mappedDocument.MapErrorDescription))
                {
                    try
                    {
                        using (var ts = repo.BeginTransaction())
                        {
                            if (epepDocData.Files != null)
                            {
                                foreach (var file in epepDocData.Files)
                                {
                                    await correctComplainRequestDocumentCaseGids(file, mappedDocument);
                                }
                            }
                            await repo.AddAsync(mappedDocument);
                            await repo.SaveChangesAsync();
                            if (epepDocData.Files != null)
                            {
                                foreach (var file in epepDocData.Files)
                                {
                                    if (!file.FileIsLoaded)
                                    {
                                        file.Content = await epepRestClient.DownloadFileContent(file.FileId);
                                    }
                                    await cdnService.MongoCdn_UploadFile(new CdnUploadRequest()
                                    {
                                        SourceType = SourceTypeSelectVM.InitFromEpepAttachedType(file.AttachmentType),
                                        SourceId = mappedDocument.Id.ToString(),
                                        FileName = file.FileName,
                                        Title = file.Title,
                                        FileContentBase64 = Convert.ToBase64String(file.Content)
                                    });
                                }
                            }
                            if (mappedDocument.CourtId == NomenclatureConstants.Courts.RandomAssignment)
                            {
                                //Създава нов документ в съд Случайно разпределение
                                var documentForAssignment = await initDocumentFromElectronic(mappedDocument.Id);
                                if (documentForAssignment.DocumentTypeId > 0)
                                {
                                    documentForAssignment.DisableTransaction = true;
                                    var documentForAssignmentResult = await documentService.Document_SaveData(documentForAssignment);
                                    if (documentForAssignmentResult)
                                    {
                                        //При успешен запис се добавя задача за определяне на съд

                                        //Добавя се нова Задача за разпределяне на документ
                                        //SourceId = Document.Id от регистратура Случайно разпределение
                                        //MqId = ElectronicDocument.Id
                                        //След регистриране в общата регистратура SourceId се променя на Document.Id

                                        await appendMQ(documentForAssignment.Id, mappedDocument.Id);
                                    }
                                }
                            }
                            else
                            {
                                var group = new MainGroup(mappedDocument.CourtId, SourceTypeSelectVM.ElectronicDocument, mappedDocument.Id);
                                repo.Add(group);
                                await repo.SaveChangesAsync();
                                group.LastTransation = new MainTransaction(group.Id, NomenclatureConstants.MainTransactionTypes.Waiting);
                            }
                            await repo.SaveChangesAsync();
                            ts.Commit();                            
                        }
                        await epepRestClient.UpdateElectronicDocumentSetDateCourtAccept(docId, mappedDocument.DateCourtAccept.Value);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Epep ElectronicDocument Map Error {docId}: NULL");
                    }
                }
                else
                {
                    if (mappedDocument == null)
                    {
                        logger.LogError($"Epep ElectronicDocument Map Error {docId}: NULL");
                    }
                    else
                    {
                        logger.LogError($"Epep ElectronicDocument Map Error {docId}: {mappedDocument.MapErrorDescription}");
                    }
                }
            }
        }

        //Променя данните за заявлението - подадените Gid-ове на лица в разпределение са Case.Side.Gid от ЕПЕП.
        //През IntegrationKeys се вземат реалните id-та на case.case_person.id и от там case.case_person.person_code
        async Task correctComplainRequestDocumentCaseGids(RestEpep.ElectronicDocumentFile file, ElectronicDocument elDoc)
        {
            if ((file.AttachmentType != EpepConstants.AttachedDocumentTypes.ElectronicDocumentRequest))
            {
                return;
            }
            if (!FastProcessRequestVM.ComplainRequests.Contains(elDoc.RequestTypeCode))
            {
                return;
            }
            if (elDoc.CaseId == null)
            {
                return;
            }
            FastProcessRequestVM request = null;
            try
            {
                string jsonData = System.Text.Encoding.UTF8.GetString(file.Content);
                request = JsonConvert.DeserializeObject<FastProcessRequestVM>(jsonData);
            }
            catch (Exception ex)
            {
            }
            if (request == null)
            {
                return;
            }
            if (request.DebtDistributions == null)
            {
                return;
            }

            bool fixedGid = false;
            foreach (var debt in request.DebtDistributions)
            {
                int casePersonId = (int)getSourceIdByOuterCode(SourceTypeSelectVM.CasePerson, debt.PersonCode);
                if (casePersonId > 0)
                {
                    var casePersonGid = await repo.GetPropByIdAsync<CasePerson, string>(x => x.Id == casePersonId, x => x.PersonGid);
                    if (!string.IsNullOrEmpty(casePersonGid))
                    {
                        string caseGid = casePersonGid.Trim().ToLower();
                        foreach (var person in elDoc.Persons)
                        {
                            if (person.PersonGid == debt.PersonCode)
                            {
                                person.PersonGid = caseGid;
                            }
                            if (person.RepresentsGid == debt.PersonCode)
                            {
                                person.RepresentsGid = caseGid;
                            }
                        }

                        debt.PersonCode = caseGid;
                        fixedGid = true;
                    }
                }
            }
            if (fixedGid)
            {
                string jsonData = JsonConvert.SerializeObject(request);
                file.Content = System.Text.Encoding.UTF8.GetBytes(jsonData);
            }
        }

        string getAssignMethod(RestEpep.ElectronicDocument elDoc)
        {
            string result = EpepConstants.EpepDocumentMethods.ForAssignment;
            if (elDoc.DocumentRequestType != FastProcessRequestVM.FastProcess417)
            {
                return result;
            }

            var requestFileContent = elDoc.Files
                                   .Where(f => f.AttachmentType == EpepConstants.AttachedDocumentTypes.ElectronicDocumentRequest)
                                   .Select(f => f.Content)
                                   .FirstOrDefault();
            if (requestFileContent == null)
            {
                return result;
            }

            try
            {
                var request417 = Newtonsoft.Json.JsonConvert.DeserializeObject<FastProcessRequestVM>(System.Text.Encoding.UTF8.GetString(requestFileContent));
                if (request417 != null)
                {
                    //Разпределението е по настоящ адрес
                    if (request417.CompetencyBase417.ForCompetencyBase)
                    {
                        result = EpepConstants.EpepDocumentMethods.ForAssignmentAddress;
                    }
                }
            }
            catch (Exception) { }
            return result;
        }

        async Task<ElectronicDocument> mapElectronicDocumentToEiss(
            bool forLog,
            RestEpep.ElectronicDocument electronicDocument,
            List<BaseCommonNomenclature> courtList,
            List<BaseCommonNomenclature> docKindList,
            List<DocumentGroup> docGroupList,
            List<BaseCommonNomenclature> feeTypes
            )
        {


            var doc = new ElectronicDocument()
            {
                CourtId = courtList.Where(x => x.Code == electronicDocument.CourtCode).Select(x => x.Id).FirstOrDefault(),
                //DocumentKindId = docKindList.Where(x => x.Code == electronicDocument.DocumentKind).Select(x => x.Id).FirstOrDefault(),
                //DocumentGroupId = docGroupList.Where(x => x.Code == electronicDocument.DocumentType).Select(x => x.Id).FirstOrDefault(),
                EpepId = electronicDocument.ElectronicDocumentId,
                EpepUserId = await repo.AllReadonly<EpepUser>().Where(x => x.EpepId == electronicDocument.UserRegistrationId).Select(x => x.Id).FirstOrDefaultAsync(),
                ApplyNumber = electronicDocument.NumberApply,
                ApplyDate = electronicDocument.DateApply,
                Description = electronicDocument.Description,
                CurrencyCode = electronicDocument.CurrencyCode,
                PaidDate = electronicDocument.DatePaid,
                DateCourtAccept = DateTime.Now,
                RequestTypeCode = electronicDocument.DocumentRequestType,
                FromAPI = electronicDocument.FromApi,
                CreateUserName = electronicDocument.CreateUserName
            };

            if (doc.PaidDate.HasValue)
            {
                switch (electronicDocument.PaymentKind)
                {
                    case 1:
                        doc.PaymentTypeId = NomenclatureConstants.PaymentType.EPEP; break;
                    case 2:
                        doc.PaymentTypeId = NomenclatureConstants.PaymentType.Bank; break;
                }
                if (!string.IsNullOrEmpty(electronicDocument.PaidInCourtCode))
                {
                    doc.VPOSPaidInCourtId = courtList.Where(x => x.Code == electronicDocument.PaidInCourtCode).Select(x => x.Id).FirstOrDefault();
                    if (doc.VPOSPaidInCourtId == 0)
                    {
                        doc.VPOSPaidInCourtId = null;
                    }
                }
            }

            var docGroup = docGroupList.Where(x => x.Code == electronicDocument.DocumentType).FirstOrDefault();
            if (docGroup != null)
            {
                doc.DocumentGroupId = docGroup.Id;
                doc.DocumentKindId = docGroup.DocumentKindId;
            }


            if (electronicDocument.BaseAmount > 0)
            {
                doc.BaseAmount = (decimal?)electronicDocument.BaseAmount / 100M;
            }
            else
            {
                doc.BaseAmount = null;
            }
            if (electronicDocument.TaxAmount > 0)
            {
                doc.TaxAmount = (decimal?)electronicDocument.TaxAmount / 100M;
                doc.MoneyFeeTypeId = feeTypes.Where(x => x.Code == electronicDocument.PricelistCode).Select(x => x.Id).FirstOrDefault();
                if (doc.MoneyFeeTypeId == 0)
                {
                    doc.MoneyFeeTypeId = null;
                }
            }
            else
            {
                doc.TaxAmount = 0M;
            }

            if (doc.CurrencyCode == NomenclatureConstants.CurrencyCode.BGN && this.DbEuroConfig.IsInEuro)
            {
                if (doc.BaseAmount > 0M)
                {
                    //Ако има материален интерес, тя е в лева и вече е влязло еврото
                    doc.BaseAmount = this.DbEuroConfig.GetEURFromBGN(doc.BaseAmount.Value);
                }

                if (doc.TaxAmount > 0M)
                {
                    //Ако има такса, тя е в лева и вече е влязло еврото
                    doc.TaxAmount = this.DbEuroConfig.GetEURFromBGN(doc.TaxAmount.Value);
                }
                doc.CurrencyCode = NomenclatureConstants.CurrencyCode.EUR;
            }

            if (electronicDocument.CaseId.HasValue)
            {
                doc.CaseId = (int)getSourceIdByOuterCode(SourceTypeSelectVM.Case, electronicDocument.CaseId.Value.ToString());
                if (doc.CaseId == 0)
                {
                    if (forLog)
                        logger.LogError("ELDOC.Map;CaseId=0; ");

                    doc.CaseId = null;
                }
                if (electronicDocument.SideId.HasValue)
                {
                    doc.CasePersonId = (int)getSourceIdByOuterCode(SourceTypeSelectVM.CasePerson, electronicDocument.SideId.Value.ToString());

                    if (doc.CasePersonId == 0)
                    {
                        if (forLog)
                            logger.LogError("ELDOC.Map;CasePersonId=0; ");
                        doc.CasePersonId = null;
                    }
                }
            }

            foreach (var side in electronicDocument.Sides)
            {
                var docPerson = new ElectronicDocumentPerson();
                docPerson.PersonRoleId = GetNomIdByOuterCodeInt(EpepConstants.Nomenclatures.PersonRolesFromEPEP, side.SideInvolvementKind);
                if (docPerson.PersonRoleId == 0)
                {
                    docPerson.PersonRoleId = GetNomIdByOuterCodeInt(EpepConstants.Nomenclatures.PersonRoles, side.SideInvolvementKind);
                }

                docPerson.PersonGid = side.ElectronicDocumentSideId.ToString();
                docPerson.RepresentsGid = side.RepresentsSideId?.ToString();
                if (!string.IsNullOrEmpty(side.CitizenshipCode))
                {
                    if (side.CitizenshipCode == NomenclatureConstants.CountryBG)
                    {
                        docPerson.CitizenshipId = NomenclatureConstants.CountryBGID;
                    }
                    else
                    {
                        docPerson.CitizenshipId = await repo.AllReadonly<EkCountry>().Where(x => x.Code == side.CitizenshipCode).Select(x => x.CountryId).FirstOrDefaultAsync();
                    }
                }

                if (side.Person != null)
                {
                    docPerson.UicTypeId = NomenclatureConstants.UicTypes.EGN;
                    docPerson.Uic = side.Person.EGN;
                    docPerson.FirstName = side.Person.Firstname;
                    docPerson.MiddleName = side.Person.Secondname;
                    docPerson.FamilyName = side.Person.Lastname;
                }
                if (side.Entity != null)
                {
                    docPerson.UicTypeId = NomenclatureConstants.UicTypes.EIK;
                    docPerson.Uic = side.Entity.Bulstat;
                    docPerson.FirstName = side.Entity.Name;
                    docPerson.FullName = side.Entity.Name;
                }
                docPerson.FullName = docPerson.MakeFullName();
                if (docPerson.PersonRoleId == 0)
                {
                    logger.LogError($"mapElectronicDocumentToEiss, PersonRole missing: {docPerson.FullName} {side.SideInvolvementKind}");
                }
                if (side.Addresses != null)
                {
                    List<ElectronicDocumentPersonAddress> personAddresses = new List<ElectronicDocumentPersonAddress>();
                    foreach (var address in side.Addresses)
                    {
                        var addressTypeId = GetNomIdByOuterCodeInt(EpepConstants.Nomenclatures.AddressTypesFromEPEP, address.AddressTypeCode);
                        if (addressTypeId == 0)
                        {
                            continue;
                        }
                        var newAddress = new ElectronicDocumentPersonAddress()
                        {
                            Address = new Address()
                            {
                                AddressTypeId = addressTypeId,
                                CountryCode = address.CountryCode,
                                ForeignAddress = address.ForeignAddress,
                                CityCode = address.CityCode,
                                ResidentionAreaCode = address.ResidentionAreaCode,
                                StreetCode = address.StreetCode,
                                Block = address.Block,
                                SubBlock = address.SubBlock,
                                StreetNumber = address.StreetNumber,
                                SubNumber = address.SubNumber,
                                Entrance = address.Entrance,
                                Floor = address.Floor,
                                Appartment = address.Apartment,
                                Email = address.Email,
                                Phone = address.Phone,
                                Fax = address.Fax,
                                Description = address.Description,
                                FullAddress = address.FullAddress
                            }
                        };
                        personAddresses.Add(newAddress);
                    }
                    docPerson.Addresses = personAddresses;
                }

                doc.Persons.Add(docPerson);
            }
            if (electronicDocument.Files != null)
            {
                foreach (var file in electronicDocument.Files)
                {
                    if (file.FileIsLoaded)
                        if (file.FileSize != file.Content.Length)
                        {
                            doc.FileError = $"File size error! {file.FileName}. Size:{file.FileSize}, Content:{file.Content.Length}";
                        }
                }
            }

            return doc;
        }

        Task appendMQ(long assignmentDocumentId, long? electronicDocumentId = null)
        {
            var mq = new MQEpep()
            {
                IntegrationTypeId = NomenclatureConstants.IntegrationTypes.EpepDocuments,
                SourceType = SourceTypeSelectVM.Document,
                SourceId = assignmentDocumentId,
                MethodName = EpepConstants.EpepDocumentMethods.InitAssignment,
                MQId = $"elDoc = {electronicDocumentId}",
                ErrorCount = 0,
                DateWrt = DateTime.Now,
                IntegrationStateId = EpepConstants.IntegrationStates.New
            };
            repo.Add(mq);
            return repo.SaveChangesAsync();
        }


    }
}
