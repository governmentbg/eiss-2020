using Integration.Epep;
using IO.LogOperation.Models;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Extensions.HTML;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Epep;
using IOWebApplicationService.Infrastructure.Contracts;
using IOWebApplicationService.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using static IOWebApplication.Infrastructure.Constants.EpepConstants;

namespace IOWebApplicationService.Infrastructure.Services
{
    [Obsolete("Да се използва EpepRestService")]
    public class EpepService : BaseMQService, IEpepService
    {
        protected IEpepConnectionService connector;
        protected IeCaseServiceClient serviceClient;
        private readonly IDeliveryItemService deliveryItemService;
        /// <summary>
        /// Активиране на надградени функционалности
        /// </summary>
        private bool UPGRADE_EPEP = false;
        private bool UPGRADE_EPEP_LawyerVacations = false;
        private int UPGRADE_EPEP_SummonDaysWait = 7;
        private bool UPGRADE_EPEP_MIGRATEUSERS = false;
        private int UPGRADE_EPEP_MIGRATEUSERS_FETCH = 0;

        public EpepService(
            IRepository _repo,
            IEpepConnectionService _connector,
            ILogger<EpepService> _logger,
            IConfiguration configuration,
            ICdnService _cdnService,
            IDeliveryItemService _deliveryItemService)
        {
            repo = _repo;
            connector = _connector;
            cdnService = _cdnService;
            logger = _logger;
            deliveryItemService = _deliveryItemService;

            IntegrationTypeId = NomenclatureConstants.IntegrationTypes.EPEP;
            batchSave = false;
            UPGRADE_EPEP = configuration.GetValue<bool>("EPEP:UPGRADE_EPEP", false);
            UPGRADE_EPEP_LawyerVacations = configuration.GetValue<bool>("EPEP:UPGRADE_EPEP_LawyerVacations", false);
            UPGRADE_EPEP_SummonDaysWait = configuration.GetValue<int>("EPEP:UPGRADE_EPEP_SummonDaysWait", 8);
            UPGRADE_EPEP_MIGRATEUSERS = configuration.GetValue<bool>("EPEP:UPGRADE_EPEP_MIGRATEUSERS", false);
            UPGRADE_EPEP_MIGRATEUSERS_FETCH = configuration.GetValue<int>("EPEP:UPGRADE_EPEP_MIGRATEUSERS_FETCH", 2000);
            //this.mqID = 110953;
        }

        protected override async Task<bool> InitChanel()
        {
            serviceClient = await connector.Connect();

            return serviceClient != null;
        }

        protected override async Task CloseChanel()
        {
            if (serviceClient != null)
            {
                await Task.Run(() => serviceClient.Close());
            }
        }

        protected override async Task Reconnect()
        {
            if (serviceClient == null)
            {
                if (!await InitChanel())
                {
                    return;
                }
            }
            await connector.Reconnect(serviceClient);
        }

        protected override async Task<IEnumerable<MQEpep>> FetchHighPriorityItems(int fetchCount)
        {
            //return null;
            int[] highPriorityAddSourceTypes = { SourceTypeSelectVM.EpepUser };
            int[] highPriorityDeleteSourceTypes = { SourceTypeSelectVM.CaseSessionActDepersonalized, SourceTypeSelectVM.CaseSessionActMotiveDepersonalized };

            var addItems = await repo.All<MQEpep>()
                             .Where(x => x.IntegrationTypeId == IntegrationTypeId)
                             .Where(x => x.DateTransfered == null && x.IntegrationStateId == IntegrationStates.New)
                             .Where(x => (highPriorityAddSourceTypes.Contains(x.SourceType) && x.MethodName == EpepConstants.Methods.Add))
                             .OrderBy(x => x.Id)
                             .Take(fetchCount)
                             .ToListAsync();

            var deleteItems = await repo.All<MQEpep>()
                            .Where(x => x.IntegrationTypeId == IntegrationTypeId)
                            .Where(x => x.DateTransfered == null && x.IntegrationStateId == IntegrationStates.New)
                            .Where(x => (highPriorityDeleteSourceTypes.Contains(x.SourceType) && x.MethodName == EpepConstants.Methods.Delete))
                            .OrderBy(x => x.Id)
                            .Take(fetchCount)
                            .ToListAsync();

            return addItems.Union(deleteItems);
        }


        protected override async Task SendMQ(MQEpep mq)
        {
            //await UpdateUserRegistrations();

            DateTime lastDate = DateTime.Now;
            this.currentMqId = mq.Id;

            this.startTime = DateTime.Now;
            switch (mq.TargetClassName)
            {
                //Достъп до дела, ЕПЕП 2023
                case nameof(UserAssignment):
                    await Send_UserAssignment(mq);
                    break;

                //Регистрация на лица
                case nameof(PersonRegistration):
                    await Send_PersonRegistration(mq);
                    break;
                //Връзки лица по дела за лице
                case nameof(PersonAssignment):
                    await Send_PersonAssignment(mq);
                    break;

                //Регистрация на адвокат
                case nameof(LawyerRegistration):
                    await Send_LawyerRegistration(mq);
                    break;
                //Връзки лица по дела за адвокат
                case nameof(LawyerAssignment):
                    await Send_LawyerAssignment(mq);
                    break;

                //Входящи и изходящи доументи и файловете към тях
                case nameof(IncomingDocument):
                    await Send_IncomingDocument(mq);
                    break;
                case nameof(IncomingDocumentFile):
                    await Send_IncomingDocumentFile(mq);
                    break;
                case nameof(OutgoingDocument):
                    await Send_OutgoingDocument(mq);
                    break;
                case nameof(OutgoingDocumentFile):
                    await Send_OutgoingDocumentFile(mq);
                    break;

                //Протоколи за разпределяне и файловете към тях
                case nameof(Assignment):
                    await Send_Assignment(mq);
                    break;
                case nameof(AssignmentFile):
                    await Send_AssignmentFile(mq);
                    break;

                //Дела
                case nameof(Integration.Epep.Case):
                    await Send_Case(mq);
                    break;

                //Свързано предходно дело
                case nameof(Integration.Epep.ConnectedCase):
                    await Send_ConnectedCase(mq);
                    break;

                //Страни по делото
                case nameof(Integration.Epep.Side):
                    await Send_Side(mq);
                    break;

                //Съдия-докладчик
                case nameof(Integration.Epep.Reporter):
                    await Send_Reporter(mq);
                    break;

                //Заседания
                case nameof(Hearing):
                    await Send_Hearing(mq);
                    break;
                //Състав по Заседания
                case nameof(HearingParticipant):
                    await Send_HearingParticipant(mq);
                    break;

                //Документи, представени в заседания
                case nameof(HearingDocument):
                    await Send_HearingDocument(mq);
                    break;

                //Призовки
                case nameof(Summon):
                    await Send_Summon(mq);
                    break;
                //Призовки - файлове
                case nameof(SummonFile):
                    await Send_SummonFile(mq);
                    break;

                //Актове
                case nameof(Act):
                    await Send_Act(mq);
                    break;
                //Актове - Съдии
                case nameof(ActPreparator):
                    await send_ActPreparator((int)mq.SourceId);
                    break;
                //Актове - необезличен файл
                case nameof(PrivateActFile):
                    await Send_PrivateActFile(mq);
                    break;
                //Актове - обезличен файл
                case nameof(PublicActFile):
                    await Send_PublicActFile(mq);
                    break;
                //Мотиви къв акт  - необезличен файл
                case nameof(PrivateMotiveFile):
                    await Send_PrivateMotiveFile(mq);
                    break;
                //Мотиви къв акт  - обезличен файл
                case nameof(PublicMotiveFile):
                    await Send_PublicMotiveFile(mq);
                    break;
                //Обжалване на акт
                case nameof(Appeal):
                    await Send_Appeal(mq);
                    break;

                //Прикачени документи - особено мнение, обезличено особено мнение, документи в заседание
                case nameof(AttachedDocument):
                    await Send_AttachedDocument(mq);
                    break;

                //Изпращане на дело за обжалване към друг съд извън ЕИСС
                case nameof(CaseMigrationRegistration):
                    await send_CaseMigrationRegistration(mq);
                    break;
                default:
                    break;
            }
            //var elapsed = DateTime.Now - lastDate;
            //logger.LogWarning($"mq {mq.TargetClassName} {mq.MethodName} {mq.Id} {elapsed.TotalMilliseconds}");
        }

        private async Task Send_PersonRegistration(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<PersonRegistration>(Encoding.UTF8.GetString(mq.Content));
            epep.PersonRegistrationId = getKeyGuidNullable(SourceTypeSelectVM.EpepUser, mq.SourceId);
            if (!epep.PersonRegistrationId.IsEmpty() && mq.MethodName == EpepConstants.Methods.Add)
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }
            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    try
                    {
                        var existingReg = await serviceClient.SelectPersonRegistrationAsync(epep.EGN);
                        if (existingReg != null && !existingReg.PersonRegistrationId.IsEmpty())
                        {

                            //актуализиране данните за потребителя от съществуващите в ЕПЕП
                            var epepModel = repo.GetById<EpepUser>((int)mq.SourceId);
                            if (epepModel.Email != existingReg.Email
                                || epepModel.FullName != existingReg.Name
                                || epepModel.BirthDate != existingReg.BirthDate)
                            {
                                string correctionLog = "Данните на потребителя са актуализирани служебно на база съществуваща регистрация в ЕПЕП.";
                                correctionLog += "<br/>";
                                correctionLog += $"Въведено: {epepModel.Email}; Актуално: {existingReg.Email}";
                                correctionLog += "<br/>";
                                correctionLog += $"Въведено: {epepModel.FullName}; Актуално: {existingReg.Name}";
                                correctionLog += "<br/>";
                                correctionLog += $"Въведено: {epepModel.BirthDate:dd.MM.yyyy}; Актуално: {existingReg.BirthDate:dd.MM.yyyy}";
                                saveLogOperForEpepUser(epepModel.Id, correctionLog);
                                epepModel.Email = existingReg.Email;
                                epepModel.FullName = existingReg.Name;
                                epepModel.BirthDate = existingReg.BirthDate;
                                //repo.SaveChanges();
                            }

                            AddIntegrationKey(mq, existingReg.PersonRegistrationId, false);
                            return;
                        }
                    }
                    catch
                    {

                    }
                    var returnGuid = await serviceClient.InsertPersonRegistrationAsync(epep);
                    var regOK = false;
                    if (!returnGuid.IsEmpty())
                    {
                        try
                        {
                            var personREG = await serviceClient.GetPersonRegistrationByIdAsync(returnGuid.Value);
                            if (personREG != null)
                            {
                                regOK = true;
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        UpdateMQ(mq, false);
                        return;
                    }
                    if (regOK)
                    {
                        AddIntegrationKey(mq, returnGuid, false);
                    }
                    else
                    {
                        mq.ErrorDescription = $"Непълна регистрация за лице с email: {epep.Email}";
                        SetErrorToMQ(mq, IntegrationStates.DataContentError);
                    }
                    break;
                case EpepConstants.Methods.Update:
                    if (epep.PersonRegistrationId == Guid.Empty)
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, await serviceClient.UpdatePersonRegistrationAsync(epep));
                    break;
            }

        }

        private void saveLogOperForEpepUser(int epepUserId, string html, OperationTypes operType = OperationTypes.Patch)
        {
            var logOper = new LogOperation()
            {
                ActionName = "epepuser_edit",
                Controller = "epep",
                ObjectKey = epepUserId.ToString(),
                OperationTypeID = (int)operType,
                OperationDate = DateTime.Now,
                UserData = html,
                OperationUser = "ЕИСС"
            };
            repo.Add(logOper);
            if (!batchSave)
                repo.SaveChanges();
        }

        private async Task Send_PersonAssignment(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<PersonAssignment>(Encoding.UTF8.GetString(mq.Content));
            epep.PersonAssignmentId = getKeyGuidNullable(SourceTypeSelectVM.EpepUserAssignment, mq.SourceId);
            epep.PersonRegistrationId = getKeyGuid(SourceTypeSelectVM.EpepUser, mq.ParentSourceId);
            if (epep.PersonRegistrationId == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                return;
            }
            if (!epep.PersonAssignmentId.IsEmpty() && mq.MethodName == EpepConstants.Methods.Add)
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }
            if (epep.SideId.IsEmpty())
            {
                var epepModel = repo.AllReadonly<EpepUserAssignment>().FirstOrDefault(x => x.Id == mq.SourceId);
                epep.SideId = getKeyGuidNullable(SourceTypeSelectVM.CasePerson, epepModel.CasePersonId);
                if (epep.SideId.IsEmpty())
                {
                    SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                    return;
                }
            }
            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, await serviceClient.InsertPersonAssignmentAsync(epep), false);
                    break;
                case EpepConstants.Methods.Update:
                    UpdateMQ(mq, await serviceClient.UpdatePersonAssignmentAsync(epep));
                    break;
                case EpepConstants.Methods.Delete:
                    if (!epep.PersonAssignmentId.HasValue)
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, await serviceClient.DeletePersonAssignmentAsync(epep.PersonAssignmentId.Value));
                    break;
            }

        }

        private async Task Send_LawyerRegistration(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<LawyerRegistration>(Encoding.UTF8.GetString(mq.Content));
            epep.LawyerRegistrationId = getKeyGuidNullable(SourceTypeSelectVM.EpepUser, mq.SourceId);

            var epepUser = repo.GetById<EpepUser>((int)mq.SourceId);
            try
            {
                var lawyerInfoFromEpep = await serviceClient.GetLawyerByNumberAsync(epepUser.LawyerNumber);
                if (lawyerInfoFromEpep != null)
                {
                    epep.LawyerId = lawyerInfoFromEpep.LawyerId ?? Guid.Empty;
                }
            }
            catch { }
            if (epep.LawyerId == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.MissingLawyerError, $"Ненамерен адвокат с номер {epepUser.LawyerNumber}");
                return;
            }
            if (!epep.LawyerRegistrationId.IsEmpty() && mq.MethodName == EpepConstants.Methods.Add)
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }
            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:

                    try
                    {
                        var lawyerIdentifiers = await serviceClient.GetLawyerRegistrationIdentifiersByLawyerIdAsync(epep.LawyerId);
                        if (lawyerIdentifiers.Any())
                        {
                            //Вече има регистрация в ЕПЕП
                            AddIntegrationKey(mq, lawyerIdentifiers.Last(), false);
                            var existingReg = await serviceClient.GetLawyerRegistrationByIdAsync(lawyerIdentifiers.Last());
                            //актуализиране данните за потребителя от съществуващите в ЕПЕП
                            var epepModel = repo.GetById<EpepUser>((int)mq.SourceId);
                            if (epepModel.Email != existingReg.Email
                                || epepModel.BirthDate != existingReg.BirthDate)
                            {
                                string correctionLog = "Данните на потребителя са актуализирани служебно на база съществуваща регистрация в ЕПЕП.";
                                correctionLog += "<br/>";
                                correctionLog += $"Въведено: {epepModel.Email}; Актуално: {existingReg.Email}";
                                correctionLog += "<br/>";
                                correctionLog += $"Въведено: {epepModel.BirthDate:dd.MM.yyyy}; Актуално: {existingReg.BirthDate:dd.MM.yyyy}";
                                saveLogOperForEpepUser(epepModel.Id, correctionLog);
                                epepModel.Email = existingReg.Email;
                                epepModel.BirthDate = existingReg.BirthDate;
                                if (!batchSave)
                                    repo.SaveChanges();
                            }
                            return;
                        }
                    }
                    catch (FaultException fex)
                    {

                    }


                    var returnGuid = await serviceClient.InsertLawyerRegistrationAsync(epep);
                    var regOK = false;
                    if (!returnGuid.IsEmpty())
                    {
                        try
                        {
                            var lawyerREG = await serviceClient.GetLawyerRegistrationByIdAsync(returnGuid.Value);
                            if (lawyerREG != null)
                            {
                                regOK = true;
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        UpdateMQ(mq, false);
                        return;
                    }

                    if (regOK)
                    {
                        AddIntegrationKey(mq, returnGuid, false);
                    }
                    else
                    {
                        mq.ErrorDescription = $"Непълна регистрация за адвокат с email: {epep.Email}";
                        SetErrorToMQ(mq, IntegrationStates.DataContentError);
                    }

                    break;
                case EpepConstants.Methods.Update:
                    if (epep.LawyerRegistrationId == Guid.Empty)
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, await serviceClient.UpdateLawyerRegistrationAsync(epep));
                    break;
            }
        }

        private async Task Send_UserAssignment(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<UserAssignment>(Encoding.UTF8.GetString(mq.Content));
            epep.UserAssignmentId = getKeyGuidNullable(SourceTypeSelectVM.EpepUserAssignment, mq.SourceId);
            epep.UserRegistrationId = getKeyGuid(SourceTypeSelectVM.EpepUser, mq.ParentSourceId);
            if (epep.UserRegistrationId == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                return;
            }
            if (!epep.UserAssignmentId.IsEmpty() && mq.MethodName == EpepConstants.Methods.Add)
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }
            if (epep.SideId == Guid.Empty)
            {
                var epepModel = repo.AllReadonly<EpepUserAssignment>().FirstOrDefault(x => x.Id == mq.SourceId);
                epep.SideId = getKeyGuid(SourceTypeSelectVM.CasePerson, epepModel.CasePersonId);
                if (epep.SideId == Guid.Empty)
                {
                    SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                    return;
                }
            }
            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, await serviceClient.InsertUserAssignmentAsync(epep), false);
                    break;
                case EpepConstants.Methods.Update:
                    UpdateMQ(mq, await serviceClient.UpdateUserAssignmentAsync(epep));
                    break;
                case EpepConstants.Methods.Delete:
                    if (!epep.UserAssignmentId.HasValue)
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, await serviceClient.DeleteUserAssignmentAsync(epep.UserAssignmentId.Value));
                    break;
            }

        }

        private async Task Send_LawyerAssignment(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<LawyerAssignment>(Encoding.UTF8.GetString(mq.Content));
            epep.LawyerAssignmentId = getKeyGuidNullable(SourceTypeSelectVM.EpepUserAssignment, mq.SourceId);
            if (!epep.LawyerAssignmentId.IsEmpty() && mq.MethodName == EpepConstants.Methods.Add)
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }
            epep.LawyerRegistrationId = getKeyGuid(SourceTypeSelectVM.EpepUser, mq.ParentSourceId);
            if (epep.LawyerRegistrationId == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                return;
            }
            if (epep.SideId.IsEmpty())
            {
                var epepModel = repo.AllReadonly<EpepUserAssignment>().FirstOrDefault(x => x.Id == mq.SourceId);
                epep.SideId = getKeyGuidNullable(SourceTypeSelectVM.CasePerson, epepModel.CasePersonId);
                if (epep.SideId.IsEmpty())
                {
                    SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                    return;
                }
            }
            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, await serviceClient.InsertLawyerAssignmentAsync(epep), false);
                    break;
                case EpepConstants.Methods.Update:
                    UpdateMQ(mq, await serviceClient.UpdateLawyerAssignmentAsync(epep));
                    break;
                case EpepConstants.Methods.Delete:
                    if (!epep.LawyerAssignmentId.HasValue)
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, await serviceClient.DeleteLawyerAssignmentAsync(epep.LawyerAssignmentId.Value));
                    break;
            }

        }

        private async Task Send_IncomingDocument(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<IncomingDocument>(Encoding.UTF8.GetString(mq.Content));
            if (string.IsNullOrEmpty(epep.IncomingDocumentTypeCode))
            {
                SetErrorToMQ(mq, IntegrationStates.MissingCodeError);
                return;
            }
            var doc = repo.AllReadonly<IOWebApplication.Infrastructure.Data.Models.Documents.Document>()
                                //.Include(x => x.DocumentGroup)
                                //.Include(x => x.DocumentCaseInfo)
                                .Where(x => x.Id == mq.SourceId)
                                .Select(x => new
                                {
                                    x.DocumentTypeId,
                                    x.DocumentGroup.DocumentKindId,
                                    caseId = x.DocumentCaseInfo.Select(c => c.CaseId).FirstOrDefault()
                                }).FirstOrDefault();

            if (doc != null)
            {
                switch (doc.DocumentKindId)
                {
                    case DocumentConstants.DocumentKind.InitialDocument:
                        //На иницииращите документи не подаваме свързано дело, защото грешно излизат в първия съд
                        epep.CaseId = null;
                        break;
                    case DocumentConstants.DocumentKind.CompliantDocument:
                        if (doc.caseId > 0)
                        {
                            epep.CaseId = getKeyGuid(SourceTypeSelectVM.Case, doc.caseId);
                            if (epep.CaseId.IsEmpty())
                            {
                                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на свързано дело");
                            }
                        }
                        break;
                    default:
                        break;
                }
                epep.IncomingDocumentTypeCode = GetNomValue(EpepConstants.Nomenclatures.IncommingDocumentTypes, doc.DocumentTypeId);
            }

            epep.IncomingDocumentId = getKeyGuidNullable(SourceTypeSelectVM.Document, mq.SourceId);
            if (mq.MethodName == EpepConstants.Methods.Add && !epep.IncomingDocumentId.IsEmpty())
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }
            if (mq.MethodName != EpepConstants.Methods.Add && epep.IncomingDocumentId.IsEmpty())
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                return;
            }
            decodeTexts(epep);
            epep.Person?.SanitizeNames();
            epep.Entity?.SanitizeNames();
            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    epep.IncomingDocumentId = null;
                    AddIntegrationKey(mq, await serviceClient.InsertIncomingDocumentAsync(epep), false);
                    break;
                case EpepConstants.Methods.Update:
                    UpdateMQ(mq, await serviceClient.UpdateIncomingDocumentAsync(epep));
                    break;
                case EpepConstants.Methods.Delete:
                    try
                    {
                        var _docFileGuid = await serviceClient.GetIncomingDocumentFileIdentifierByIncomingDocumentIdAsync(epep.IncomingDocumentId.Value);
                        if (!_docFileGuid.IsEmpty())
                        {
                            await serviceClient.DeleteIncomingDocumentFileAsync(_docFileGuid.Value);
                        }
                    }
                    catch (Exception ex) { }
                    UpdateMQ(mq, await serviceClient.DeleteIncomingDocumentAsync(epep.IncomingDocumentId.Value));
                    break;
            }
        }

        private void decodeTexts(IncomingDocument model)
        {
            if (model.Person != null)
            {
                model.Person.Firstname = model.Person.Firstname.Decode();
                model.Person.Secondname = model.Person.Secondname.Decode();
                model.Person.Lastname = model.Person.Lastname.Decode();
                model.Person.Address = model.Person.Address.Decode();
            }

            if (model.Entity != null)
            {
                model.Entity.Name = model.Entity.Name.Decode();
                model.Entity.Address = model.Entity.Address.Decode();
            }
        }

        private async Task Send_OutgoingDocument(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<OutgoingDocument>(Encoding.UTF8.GetString(mq.Content));
            var doc = repo.AllReadonly<IOWebApplication.Infrastructure.Data.Models.Documents.Document>()
                               //.Include(x => x.DocumentGroup)
                               //.Include(x => x.DocumentCaseInfo)
                               .Where(x => x.Id == mq.SourceId)
                               .Select(x => new
                               {
                                   x.DocumentTypeId,
                                   x.DocumentGroup.DocumentKindId,
                                   caseId = x.DocumentCaseInfo.Select(c => c.CaseId).FirstOrDefault()
                               }).FirstOrDefault();
            if (doc != null)
            {
                epep.OutgoingDocumentTypeCode = GetNomValue(EpepConstants.Nomenclatures.OutgoingDocumentTypes, doc.DocumentTypeId);
                if (doc.caseId > 0)
                {
                    epep.CaseId = getKeyGuid(SourceTypeSelectVM.Case, doc.caseId);
                    if (epep.CaseId.IsEmpty())
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на свързано дело");
                    }
                }
                else
                {
                    SetErrorToMQ(mq, IntegrationStates.DisabledByDelete, "Няма свързано дело");
                }
            }

            if (string.IsNullOrEmpty(epep.OutgoingDocumentTypeCode))
            {
                SetErrorToMQ(mq, IntegrationStates.MissingCodeError);
                return;
            }

            if (epep.Person != null)
            {
                epep.Person.Firstname = epep.Person.Firstname ?? " ";
                epep.Person.Lastname = epep.Person.Lastname ?? " ";
            }
            epep.OutgoingDocumentId = getKeyGuidNullable(SourceTypeSelectVM.Document, mq.SourceId);
            if (mq.MethodName == EpepConstants.Methods.Add && !epep.OutgoingDocumentId.IsEmpty())
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }
            if (mq.MethodName != EpepConstants.Methods.Add && epep.OutgoingDocumentId.IsEmpty())
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                return;
            }
            decodeTexts(epep);
            epep.Person?.SanitizeNames();
            epep.Entity?.SanitizeNames();
            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, await serviceClient.InsertOutgoingDocumentAsync(epep), false);
                    break;
                case EpepConstants.Methods.Update:
                    UpdateMQ(mq, await serviceClient.UpdateOutgoingDocumentAsync(epep));
                    break;
                case EpepConstants.Methods.Delete:
                    try
                    {
                        var _docFileGuid = await serviceClient.GetOutgoingDocumentFileIdentifierByOutgoingDocumentIdAsync(epep.OutgoingDocumentId.Value);
                        if (!_docFileGuid.IsEmpty())
                        {
                            await serviceClient.DeleteOutgoingDocumentFileAsync(_docFileGuid.Value);
                        }
                    }
                    catch (Exception ex) { }
                    UpdateMQ(mq, await serviceClient.DeleteOutgoingDocumentAsync(epep.OutgoingDocumentId.Value));
                    break;
            }

        }

        private void decodeTexts(OutgoingDocument model)
        {
            if (model.Person != null)
            {
                model.Person.Firstname = model.Person.Firstname.Decode();
                model.Person.Secondname = model.Person.Secondname.Decode();
                model.Person.Lastname = model.Person.Lastname.Decode();
                model.Person.Address = model.Person.Address.Decode();
            }

            if (model.Entity != null)
            {
                model.Entity.Name = model.Entity.Name.Decode();
                model.Entity.Address = model.Entity.Address.Decode();
            }
        }

        private async Task send_AttachedDocumentFromFile(MQEpep mq, int attachedDocumentType, Guid ParentId)
        {
            var epep = new AttachedDocument()
            {
                ParentId = ParentId,
                Type = attachedDocumentType
            };
            var fileId = getKeyGuidNullable(SourceTypeSelectVM.Files, mq.SourceId);
            var attachedDocumentFileId = getKeyGuidNullable(SourceTypeSelectVM.AttachedDocumentFiles, mq.SourceId);


            switch (mq.MethodName)
            {
                case Methods.Add:
                    {
                        if (!fileId.IsEmpty() || !attachedDocumentFileId.IsEmpty())
                        {
                            UpdateMQ(mq, true);
                            return;
                        }

                        var fileModel = await cdnService.MongoCdn_Download((int)mq.SourceId);
                        if (fileModel == null)
                        {
                            mq.ErrorDescription = $"Грешен файл с ID={mq.SourceId}";
                            SetErrorToMQ(mq, IntegrationStates.MissingObjectEISS);
                            return;
                        }

                        epep.FileDate = fileModel.DateUploaded;
                        epep.MimeType = fileModel.ContentType;
                        epep.FileTitle = fileModel.FileTitle;
                        epep.FileName = fileModel.FileName;
                        epep.FileContent = fileModel.GetBytes();

                        var epepGuid = await serviceClient.InsertAttachedDocumentAsync(epep);
                        if (epepGuid.HasValue)
                        {
                            mq.SourceType = SourceTypeSelectVM.AttachedDocumentFiles;
                        }
                        AddIntegrationKey(mq, epepGuid, false);
                    }
                    break;
                case Methods.Delete:
                    {
                        if (!fileId.IsEmpty())
                        {
                            switch (attachedDocumentType)
                            {
                                case AttachedDocumentTypes.IncommingDocument:
                                    UpdateMQ(mq, await serviceClient.DeleteIncomingDocumentFileAsync(epep.ParentId));
                                    break;
                                case AttachedDocumentTypes.OutgoingDocument:
                                    UpdateMQ(mq, await serviceClient.DeleteOutgoingDocumentFileAsync(epep.ParentId));
                                    break;
                            }
                        }
                        else
                        {
                            if (!attachedDocumentFileId.HasValue)
                            {
                                mq.ErrorDescription = $"Ненамерен файл с attachedDocumentFileId={mq.SourceId}";
                                SetErrorToMQ(mq, IntegrationStates.MissingObjectEISS);
                                return;
                            }
                            UpdateMQ(mq, await serviceClient.DeleteAttachedDocumentAsync(attachedDocumentFileId.Value));
                            break;
                        }
                    }
                    break;
                default:
                    mq.ErrorDescription = "Грешен метод";
                    UpdateMQ(mq, false);
                    break;
            }


        }

        private async Task Send_IncomingDocumentFile(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<IncomingDocumentFile>(Encoding.UTF8.GetString(mq.Content));


            epep.IncomingDocumentId = getKeyGuid(SourceTypeSelectVM.Document, mq.ParentSourceId);



            if (epep.IncomingDocumentId == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на документ");
                return;
            }


            await send_AttachedDocumentFromFile(mq, AttachedDocumentTypes.IncommingDocument, epep.IncomingDocumentId);
            return;


            //var fileModel = await cdnService.MongoCdn_Download(mq.SourceId);
            //if (fileModel == null)
            //{
            //    mq.ErrorDescription = $"Грешен файл с ID={mq.SourceId}";
            //    SetErrorToMQ(mq, IntegrationStates.MissingObjectEISS);
            //    return;
            //}
            //epep.IncomingDocumentMimeType = fileModel.ContentType;
            //epep.IncomingDocumentContent = fileModel.GetBytes();
            //epep.IncomingDocumentFileId = getKeyGuidNullable(SourceTypeSelectVM.Files, mq.SourceId);
            //if (mq.MethodName == EpepConstants.Methods.Add && !epep.IncomingDocumentFileId.IsEmpty())
            //{
            //    mq.MethodName = EpepConstants.Methods.Update;
            //}

            //if (mq.MethodName != EpepConstants.Methods.Add && epep.IncomingDocumentFileId.IsEmpty())
            //{
            //    SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
            //    return;
            //}

            //switch (mq.MethodName)
            //{
            //    case EpepConstants.Methods.Add:
            //        if (epep.IncomingDocumentFileId.IsEmpty())
            //        {
            //            epep.IncomingDocumentFileId = null;
            //        }
            //        AddIntegrationKey(mq, await serviceClient.InsertIncomingDocumentFileAsync(epep), false);
            //        break;
            //    case EpepConstants.Methods.Update:
            //        UpdateMQ(mq, await serviceClient.UpdateIncomingDocumentFileAsync(epep));
            //        break;
            //    case EpepConstants.Methods.Delete:
            //        UpdateMQ(mq, await serviceClient.DeleteIncomingDocumentFileAsync(epep.IncomingDocumentId));
            //        break;
            //}

        }
        private async Task Send_OutgoingDocumentFile(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<OutgoingDocumentFile>(Encoding.UTF8.GetString(mq.Content));

            var docGuid = getKey(SourceTypeSelectVM.Document, mq.ParentSourceId);

            if (string.IsNullOrEmpty(docGuid))
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на документ");
                return;
            }
            epep.OutgoingDocumentId = Guid.Parse(docGuid);


            await send_AttachedDocumentFromFile(mq, AttachedDocumentTypes.OutgoingDocument, epep.OutgoingDocumentId);
            return;


            //var fileModel = await cdnService.MongoCdn_Download(mq.SourceId, CdnFileSelect.PostProcess.Flatten);
            //if (fileModel == null)
            //{
            //    mq.ErrorDescription = $"Грешен файл с ID={mq.SourceId}";
            //    SetErrorToMQ(mq, IntegrationStates.MissingObjectEISS);
            //    return;
            //}
            //epep.OutgoingDocumentMimeType = fileModel.ContentType;
            //epep.OutgoingDocumentContent = fileModel.GetBytes();

            //epep.OutgoingDocumentFileId = getKeyGuidNullable(SourceTypeSelectVM.Files, mq.SourceId);
            //if (mq.MethodName == EpepConstants.Methods.Add && !epep.OutgoingDocumentFileId.IsEmpty())
            //{
            //    mq.MethodName = EpepConstants.Methods.Update;
            //}
            //if (mq.MethodName != EpepConstants.Methods.Add && epep.OutgoingDocumentFileId.IsEmpty())
            //{
            //    SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
            //    return;
            //}
            //switch (mq.MethodName)
            //{
            //    case EpepConstants.Methods.Add:
            //        if (epep.OutgoingDocumentFileId.IsEmpty())
            //        {
            //            epep.OutgoingDocumentFileId = null;
            //        }
            //        AddIntegrationKey(mq, await serviceClient.InsertOutgoingDocumentFileAsync(epep), false);
            //        break;
            //    case EpepConstants.Methods.Update:
            //        UpdateMQ(mq, await serviceClient.UpdateOutgoingDocumentFileAsync(epep));
            //        break;
            //    case EpepConstants.Methods.Delete:
            //        UpdateMQ(mq, await serviceClient.DeleteOutgoingDocumentFileAsync(epep.OutgoingDocumentId));
            //        break;
            //}

        }

        private async Task Send_Assignment(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<Assignment>(Encoding.UTF8.GetString(mq.Content));
            var info = repo.AllReadonly<CaseSelectionProtokol>()
                                   //.Include(x => x.Case)
                                   .Where(x => x.Id == mq.SourceId)
                                   .Select(x => new
                                   {
                                       CaseId = x.CaseId,
                                       DocumentId = x.Case.DocumentId
                                   }).FirstOrDefault();

            if (epep.IncomingDocumentId == Guid.Empty)
            {
                var docGuid = getKey(SourceTypeSelectVM.Document, info.DocumentId);

                if (string.IsNullOrEmpty(docGuid))
                {
                    SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на документ");
                    return;
                }
                epep.IncomingDocumentId = Guid.Parse(docGuid);
            }
            if (epep.CaseId == Guid.Empty)
            {
                var caseGuid = getKey(SourceTypeSelectVM.Case, info.CaseId);

                if (string.IsNullOrEmpty(caseGuid))
                {
                    SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на дело");
                    return;
                }
                epep.CaseId = Guid.Parse(caseGuid);
            }

            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, await serviceClient.InsertAssignmentAsync(epep), false);
                    break;
            }

        }

        private async Task Send_AssignmentFile(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<AssignmentFile>(Encoding.UTF8.GetString(mq.Content));
            epep.AssignmentId = getKeyGuid(SourceTypeSelectVM.CaseSelectionProtokol, mq.ParentSourceId);
            if (epep.AssignmentId == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на разпределение");
                return;
            }
            epep.AssignmentFileId = getKeyGuidNullable(SourceTypeSelectVM.CaseSelectionProtokolFile, mq.SourceId);
            if (!epep.AssignmentFileId.IsEmpty() && mq.MethodName == EpepConstants.Methods.Add)
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }

            var fileModel = await cdnService.MongoCdn_Download(new CdnFileSelect() { SourceType = SourceTypeSelectVM.CaseSelectionProtokol, SourceId = mq.SourceId.ToString() }, CdnFileSelect.PostProcess.Flatten);
            if (fileModel != null)
            {
                epep.ProtocolMimeType = fileModel.ContentType;
                epep.ProtocolContent = fileModel.GetBytes();
            }
            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, await serviceClient.InsertAssignmentFileAsync(epep), false);
                    break;
                case EpepConstants.Methods.Update:
                    UpdateMQ(mq, await serviceClient.UpdateAssignmentFileAsync(epep));
                    break;
            }

        }

        private async Task Send_Case(MQEpep mq)
        {
            if (mq.MethodName == EpepConstants.Methods.DataChange)
            {
                await send_CaseDataChange(mq);
                return;
            }

            var epep = JsonConvert.DeserializeObject<Integration.Epep.Case>(Encoding.UTF8.GetString(mq.Content));


            epep.IncomingDocumentId = getKeyGuid(SourceTypeSelectVM.Document, mq.ParentSourceId);

            if (epep.IncomingDocumentId == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на документ");
                return;
            }
            epep.CaseId = getKeyGuidNullable(SourceTypeSelectVM.Case, mq.SourceId);
            if (!epep.CaseId.IsEmpty() && mq.MethodName == EpepConstants.Methods.Add)
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }

            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    Guid? caseId;
                    caseId = await serviceClient.InsertCaseAsync(epep);
                    AddIntegrationKey(mq, caseId, false);

                    break;
                case EpepConstants.Methods.Update:
                    if (epep.CaseId.IsEmpty())
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, await serviceClient.UpdateCaseAsync(epep));
                    break;
                case EpepConstants.Methods.Delete:
                    if (epep.CaseId.IsEmpty())
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    try
                    {
                        var identifiers = await serviceClient.GetAssignmentIdentifiersByCaseIdAsync(epep.CaseId.Value);
                        foreach (var item in identifiers)
                        {
                            await serviceClient.DeleteAssignmentAsync(item);
                        }
                    }
                    catch (Exception ex) { }

                    try
                    {
                        var identifiers = await serviceClient.GetSideIdentifiersByCaseIdAsync(epep.CaseId.Value);
                        foreach (var item in identifiers)
                        {
                            await serviceClient.DeleteSideAsync(item);
                        }
                    }
                    catch { }
                    try
                    {
                        var identifiers = await serviceClient.GetHearingIdentifiersByCaseIdAsync(epep.CaseId.Value);
                        foreach (var item in identifiers)
                        {
                            try
                            {
                                var identifiersP = await serviceClient.GetHearingParticipantIdentifiersByHearingIdAsync(item);
                                foreach (var itemP in identifiersP)
                                {
                                    await serviceClient.DeleteHearingParticipantAsync(itemP);
                                }
                            }
                            catch { }
                            await serviceClient.DeleteHearingAsync(item);
                        }
                    }
                    catch { }

                    try
                    {
                        var identifiers = await serviceClient.GetReporterIdentifiersByCaseIdAsync(epep.CaseId.Value);
                        foreach (var item in identifiers)
                        {
                            await serviceClient.DeleteReporterAsync(item);
                        }
                    }
                    catch { }

                    UpdateMQ(mq, await serviceClient.DeleteCaseAsync(epep.CaseId.Value));
                    break;
            }

        }

        /// <summary>
        /// Променя данни за ограничено дело и актуален състав
        /// </summary>
        /// <param name="mq"></param>
        /// <returns></returns>
        private async Task send_CaseDataChange(MQEpep mq)
        {
            var caseGid = getKeyGuid(SourceTypeSelectVM.Case, mq.SourceId);
            if (caseGid == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.MissingCodeError, "Липсва код на дело");
                return;
            }
            int caseId = (int)mq.SourceId;

            var info = await repo.AllReadonly<IOWebApplication.Infrastructure.Data.Models.Cases.Case>()
                                 //.Include(x => x.Document)
                                 .Include(x => x.CaseState)
                                 .Include(x => x.CaseCode)
                                 .Where(x => x.Id == caseId)
                                 .Select(x => new
                                 {
                                     StateName = x.CaseState.Label,
                                     CaseCode = x.CaseCode.Code,
                                     IsRestricted = x.CaseClassifications.Any(c => c.DateTo == null && NomenclatureConstants.CaseClassifications.RestictedAccess.Contains(c.ClassificationId))
                                 }).FirstOrDefaultAsync();

            var courtDepartment = await repo.AllReadonly<CaseLawUnit>()
                                    .Include(x => x.CourtDepartment)
                                    .ThenInclude(x => x.ParentDepartment)
                                    .Where(x => x.CaseId == caseId && x.CaseSessionId == null)
                                    .Where(x => x.DateFrom <= DateTime.Now && (x.DateTo ?? DateTime.MaxValue) >= DateTime.Now)
                                    .Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                    .Select(x => x.CourtDepartment)
                                    .FirstOrDefaultAsync();

            Integration.Epep.Case epep = await serviceClient.GetCaseByIdAsync(caseGid);
            if (epep != null)
            {

                if (courtDepartment != null)
                {
                    epep.PanelName = courtDepartment.Label;
                    if (courtDepartment.ParentDepartment != null)
                    {
                        epep.DepartmentName = courtDepartment.ParentDepartment.Label;

                    }
                }

                epep.RestrictedAccess = info.IsRestricted;
            }

            UpdateMQ(mq, await serviceClient.UpdateCaseAsync(epep));
        }

        private async Task Send_Side(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<Integration.Epep.Side>(Encoding.UTF8.GetString(mq.Content));

            epep.CaseId = getKeyGuid(SourceTypeSelectVM.Case, mq.ParentSourceId);
            if (epep.CaseId == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на дело");
                return;
            }
            epep.SideId = getKeyGuidNullable(SourceTypeSelectVM.CasePerson, mq.SourceId);
            if (!epep.SideId.IsEmpty() && mq.MethodName == EpepConstants.Methods.Add)
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }
            decodeTexts(epep);
            epep.Person?.SanitizeNames();
            epep.Entity?.SanitizeNames();

            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, await serviceClient.InsertSideAsync(epep), false);
                    break;
                case EpepConstants.Methods.Update:
                    if (epep.SideId.IsEmpty())
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, await serviceClient.UpdateSideAsync(epep));
                    break;
                case EpepConstants.Methods.Delete:
                    if (epep.SideId.IsEmpty())
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, await serviceClient.DeleteSideAsync(epep.SideId.Value));
                    break;
            }

        }

        private void decodeTexts(Integration.Epep.Side model)
        {
            if (model.Person != null)
            {
                model.Person.Firstname = model.Person.Firstname.Decode();
                model.Person.Secondname = model.Person.Secondname.Decode();
                model.Person.Lastname = model.Person.Lastname.Decode();
                model.Person.Address = model.Person.Address.Decode();
            }

            if (model.Entity != null)
            {
                model.Entity.Name = model.Entity.Name.Decode();
                model.Entity.Address = model.Entity.Address.Decode();
            }
        }
        private async Task Send_Reporter(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<Integration.Epep.Reporter>(Encoding.UTF8.GetString(mq.Content));

            epep.CaseId = getKeyGuid(SourceTypeSelectVM.Case, mq.ParentSourceId);
            if (epep.CaseId == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на дело");
                return;
            }
            epep.ReporterId = getKeyGuidNullable(SourceTypeSelectVM.CaseReporter, mq.SourceId);
            if (!epep.ReporterId.IsEmpty() && mq.MethodName == EpepConstants.Methods.Add)
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }

            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, await serviceClient.InsertReporterAsync(epep), false);
                    break;
                case EpepConstants.Methods.Update:
                    if (epep.ReporterId.IsEmpty())
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, await serviceClient.UpdateReporterAsync(epep));
                    break;
                case EpepConstants.Methods.Delete:
                    if (epep.ReporterId.IsEmpty())
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, await serviceClient.DeleteReporterAsync(epep.ReporterId ?? Guid.Empty));
                    break;
            }

        }

        private async Task Send_Hearing(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<Integration.Epep.Hearing>(Encoding.UTF8.GetString(mq.Content));

            epep.CaseId = getKeyGuid(SourceTypeSelectVM.Case, mq.ParentSourceId);
            if (epep.CaseId == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на дело");
                return;
            }

            epep.HearingId = getKeyGuidNullable(SourceTypeSelectVM.CaseSession, mq.SourceId);
            if (!epep.HearingId.IsEmpty() && mq.MethodName == EpepConstants.Methods.Add)
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }

            var caseSession = repo.AllReadonly<CaseSession>().Where(x => x.Id == (int)mq.SourceId).FirstOrDefault();

            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, await serviceClient.InsertHearingAsync(epep), false);
                    if (caseSession.SessionStateId == NomenclatureConstants.SessionState.Provedeno)
                    {
                        await send_HearingParticipants(caseSession);
                    }
                    break;
                case EpepConstants.Methods.Update:
                    if (epep.HearingId.IsEmpty())
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    var res = await serviceClient.UpdateHearingAsync(epep);
                    UpdateMQ(mq, res);

                    if (caseSession.SessionStateId == NomenclatureConstants.SessionState.Provedeno)
                    {
                        await send_HearingParticipants(caseSession);
                    }
                    break;
                case EpepConstants.Methods.Delete:
                    if (epep.HearingId.IsEmpty())
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    try
                    {
                        var _hp = await serviceClient.GetHearingParticipantIdentifiersByHearingIdAsync(epep.HearingId.Value);
                        foreach (var item in _hp)
                        {
                            await serviceClient.DeleteHearingParticipantAsync(item);
                            RemoveIntegrationKeys(item.ToString(), SourceTypeSelectVM.CaseLawUnit);
                        }
                    }
                    catch (Exception ex) { }
                    UpdateMQ(mq, await serviceClient.DeleteHearingAsync(epep.HearingId.Value));
                    break;
            }

        }
        private async Task send_HearingParticipants(CaseSession caseSession)
        {
            var caseLawUnits = repo.AllReadonly<CaseLawUnit>()
                                        .Where(x => x.CaseId == caseSession.CaseId && x.CaseSessionId == caseSession.Id)
                                        .Where(x => NomenclatureConstants.JudgeRole.JudgeRolesListMain.Contains(x.JudgeRoleId))
                                        .OrderBy(x => x.JudgeDepartmentRoleId)
                                        .ThenBy(x => x.JudgeRoleId)
                                        .ThenBy(x => x.DateFrom)
                                        .Select(x => new
                                        {
                                            Id = x.Id,
                                            FullName = x.LawUnit.FullName,
                                            JudgeRole = x.JudgeRole.Label,
                                            JudgeDepartmentRole = (x.JudgeDepartmentRole != null) ? x.JudgeDepartmentRole.Label : "Член",
                                            DateTo = x.DateTo ?? DateTime.MaxValue
                                        }).ToList();
            var HearingId = getKeyGuid(SourceTypeSelectVM.CaseSession, caseSession.Id);
            if (HearingId == Guid.Empty)
            {
                return;
            }
            try
            {
                var participantsIds = await serviceClient.GetHearingParticipantIdentifiersByHearingIdAsync(HearingId);
                foreach (var item in participantsIds)
                {
                    await serviceClient.DeleteHearingParticipantAsync(item);
                    RemoveIntegrationKeys(item.ToString(), SourceTypeSelectVM.CaseLawUnit);
                }
            }
            catch (Exception ex) { }
            foreach (var lawUnit in caseLawUnits)
            {
                if (lawUnit.DateTo < caseSession.DateTo)
                {
                    continue;
                }

                HearingParticipant epep = new HearingParticipant()
                {
                    HearingId = HearingId,
                    JudgeName = lawUnit.FullName,
                    Role = lawUnit.JudgeDepartmentRole
                };
                var returnGuid = await serviceClient.InsertHearingParticipantAsync(epep);
                if (returnGuid.HasValue)
                {
                    AddIntegrationKey(SourceTypeSelectVM.CaseLawUnit, lawUnit.Id, returnGuid.Value.ToString(), false);
                }
            }
        }

        private async Task Send_HearingDocument(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<Integration.Epep.HearingDocument>(Encoding.UTF8.GetString(mq.Content));

            epep.HearingId = getKeyGuid(SourceTypeSelectVM.CaseSession, mq.ParentSourceId);
            if (epep.HearingId == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на заседание");
                return;
            }

            epep.HearingDocumentId = getKeyGuidNullable(SourceTypeSelectVM.CaseSessionFastDocument, mq.SourceId);
            if (!epep.HearingDocumentId.IsEmpty() && mq.MethodName == EpepConstants.Methods.Add)
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }
            var info = repo.AllReadonly<CaseSessionFastDocument>()
                                .Where(x => x.Id == (int)mq.SourceId)
                                .Select(x => new
                                {
                                    SessionPersonId = x.CasePersonId
                                }).FirstOrDefault();

            var casePersonId = getCasePersonIdFromCase(info.SessionPersonId, false);
            epep.SideId = getKeyGuid(SourceTypeSelectVM.CasePerson, casePersonId);
            if (epep.SideId == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на страна");
                return;
            }

            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, await serviceClient.InsertHearingDocumentAsync(epep), false);

                    break;
                case EpepConstants.Methods.Update:
                    if (epep.HearingDocumentId.IsEmpty())
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    var res = await serviceClient.UpdateHearingDocumentAsync(epep);
                    UpdateMQ(mq, res);

                    break;
                case EpepConstants.Methods.Delete:
                    if (epep.HearingDocumentId.IsEmpty())
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }

                    UpdateMQ(mq, await serviceClient.DeleteHearingDocumentAsync(epep.HearingDocumentId.Value));
                    break;
            }

        }

        private async Task Send_HearingParticipant(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<HearingParticipant>(Encoding.UTF8.GetString(mq.Content));

            epep.HearingId = getKeyGuid(SourceTypeSelectVM.CaseSession, mq.ParentSourceId);
            if (epep.HearingId == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на заседание");
                return;
            }

            epep.HearingParticipantId = getKeyGuidNullable(SourceTypeSelectVM.CaseLawUnit, mq.SourceId);
            if (!epep.HearingParticipantId.IsEmpty() && mq.MethodName == EpepConstants.Methods.Add)
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }
            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, await serviceClient.InsertHearingParticipantAsync(epep), false);
                    break;
                case EpepConstants.Methods.Update:
                    if (epep.HearingParticipantId.IsEmpty())
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, await serviceClient.UpdateHearingParticipantAsync(epep));
                    break;
                case EpepConstants.Methods.Delete:
                    if (epep.HearingParticipantId.IsEmpty())
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, await serviceClient.DeleteHearingParticipantAsync(epep.HearingParticipantId.Value));
                    break;
            }

        }
        private async Task Send_Summon(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<Summon>(Encoding.UTF8.GetString(mq.Content));

            var _notification = repo.GetById<CaseNotification>((int)mq.SourceId);
            var epepInfo = getEpepSummonInfo(_notification);

            if (epepInfo == null)
            {
                SetErrorToMQ(mq, IntegrationStates.DataContentError);
                return;
            }

            if (string.IsNullOrEmpty(epep.Addressee))
            {
                epep.Addressee = repo.GetPropById<CasePerson, string>(x => x.Id == _notification.CasePersonId, x => x.FullName).Decode();
            }

            epep.SideId = getKeyGuid(SourceTypeSelectVM.CasePerson, epepInfo.CasePersonId);

            if (epep.ParentId == Guid.Empty)
            {
                switch (epep.SummonTypeCode)
                {
                    case SummonTypeCode_CasesessionAct:
                        if (_notification.CaseSessionActId != null)
                        {
                            epep.ParentId = getKeyGuid(SourceTypeSelectVM.CaseSessionAct, _notification.CaseSessionActId);
                        }
                        break;
                    case SummonTypeCode_CaseSession:
                        if (_notification.CaseSessionId != null)
                        {
                            epep.ParentId = getKeyGuid(SourceTypeSelectVM.CaseSession, _notification.CaseSessionId);
                        }
                        break;
                }
            }

            var epepUser = getKeyGuid(SourceTypeSelectVM.EpepUser, epepInfo.EpepUserId);
            if (epep.ParentId == Guid.Empty || epep.SideId == Guid.Empty || epepUser == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "SideId");
                return;
            }

            epep.SummonId = getKeyGuidNullable(SourceTypeSelectVM.CaseNotification, mq.SourceId);
            if (!epep.SummonId.IsEmpty() && mq.MethodName == EpepConstants.Methods.Add)
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }

            if (_notification.CaseSessionActComplainId > 0 && (epep.IncommingDocumentId == null || epep.IncommingDocumentId == Guid.Empty))
            {
                var complainDocumentId = await repo.GetPropByIdAsync<CaseSessionActComplain, long>(x => x.Id == (int)_notification.CaseSessionActComplainId, x => x.ComplainDocumentId);
                epep.IncommingDocumentId = getKeyGuid(SourceTypeSelectVM.Document, complainDocumentId);
                if (epep.IncommingDocumentId == Guid.Empty)
                {
                    SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "IncommingDocumentId");
                    return;
                }
            }

            //if (string.IsNullOrEmpty(epep.Subject))
            //{
            //    epep.Subject = epep.SummonKind;
            //}

            epep.Subject = epep.SummonKind;

            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, await serviceClient.InsertSummonAsync(epep, epepUser), false);

                    if (mq.IntegrationStateId == IntegrationStates.TransferOK)
                    {
                        //Актуализира статуса на призовката на За Връчване
                        var caseNotification = repo.GetById<CaseNotification>((int)mq.SourceId);
                        caseNotification.NotificationStateId = NomenclatureConstants.NotificationState.ForDelivery;
                        caseNotification.DateSend = DateTime.Now;
                        await deliveryItemService.CreateDeliveryItem(caseNotification, true);
                        if (!batchSave)
                            repo.SaveChanges();
                    }
                    break;
                case EpepConstants.Methods.Update:
                    if (epep.SummonId.IsEmpty())
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, await serviceClient.UpdateSummonAsync(epep));
                    break;
                case EpepConstants.Methods.Delete:
                    if (epep.SummonId.IsEmpty())
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, await serviceClient.DeleteSummonAsync(epep.SummonId.Value));
                    break;
            }

        }

        private async Task Send_SummonFile(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<SummonFile>(Encoding.UTF8.GetString(mq.Content));

            epep.SummonId = getKeyGuid(SourceTypeSelectVM.CaseNotification, mq.ParentSourceId);


            if (epep.SummonId == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на призовка");
                return;
            }

            epep.SummonFileId = getKeyGuidNullable(SourceTypeSelectVM.CaseNotificationPrint, mq.SourceId);
            if (!epep.SummonFileId.IsEmpty() && mq.MethodName == EpepConstants.Methods.Add)
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }

            CdnItemVM aFile = cdnService.Select(SourceTypeSelectVM.CaseNotificationPrint, mq.SourceId.ToString()).Where(x => x.FileName.EndsWith(".pdf")).FirstOrDefault();
            if (aFile != null)
            {
                var fileModel = await cdnService.MongoCdn_Download(aFile);
                if (fileModel == null || string.IsNullOrEmpty(fileModel?.FileContentBase64))
                {
                    SetErrorToMQ(mq, IntegrationStates.MissingObjectEISS, "Грешен или липсващ файл.");
                    return;
                }
                epep.MimeType = fileModel.ContentType;
                epep.Content = fileModel.GetBytes();
            }
            else
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                return;
            }

            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, await serviceClient.InsertSummonFileAsync(epep), false);
                    break;
                case EpepConstants.Methods.Update:
                    if (epep.SummonFileId.IsEmpty())
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, await serviceClient.UpdateSummonFileAsync(epep));
                    break;
                case EpepConstants.Methods.Delete:
                    if (epep.SummonId == Guid.Empty)
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, await serviceClient.DeleteSummonFileAsync(epep.SummonId));
                    break;
            }

        }

        private async Task Send_ConnectedCase(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<Integration.Epep.ConnectedCase>(Encoding.UTF8.GetString(mq.Content));

            var caseMigration = repo.GetById<CaseMigration>((int)mq.SourceId);

            epep.CaseId = getKeyGuid(SourceTypeSelectVM.Case, caseMigration.CaseId);
            epep.PredecessorCaseId = getKeyGuid(SourceTypeSelectVM.Case, caseMigration.PriorCaseId);
            if (epep.CaseId == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на дело");
                return;
            }
            if (epep.PredecessorCaseId == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на предходно дело");
                return;
            }
            epep.ConnectedCaseTypeCode = "3000";//Свързано дело

            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, await serviceClient.InsertConnectedCaseAsync(epep), false);
                    break;
                case EpepConstants.Methods.Update:
                    if (epep.CaseId == Guid.Empty)
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    var res = await serviceClient.UpdateConnectedCaseAsync(epep);
                    UpdateMQ(mq, res);
                    break;
                case EpepConstants.Methods.Delete:
                    if (epep.CaseId == Guid.Empty)
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }

                    UpdateMQ(mq, await serviceClient.DeleteConnectedCaseAsync(epep.CaseId));
                    break;
            }

        }

        /// <summary>
        /// Изпращане на изходящо движение към друг съд
        /// </summary>
        /// <param name="mq"></param>
        /// <returns></returns>
        private async Task send_CaseMigrationRegistration(MQEpep mq)
        {
            var epep = new CaseMigrationRegistration();

            var caseMigration = repo.GetById<CaseMigration>((int)mq.SourceId);
            epep.FromCourt = GetNomValue(EpepConstants.Nomenclatures.Courts, caseMigration.CourtId);
            epep.ToCourt = GetNomValue(EpepConstants.Nomenclatures.Courts, caseMigration.SendToCourtId);

            epep.CaseId = getKeyGuid(SourceTypeSelectVM.Case, caseMigration.CaseId);
            if (epep.CaseId == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на дело");
                return;
            }
            epep.MigrationType = GetNomValue(EpepConstants.Nomenclatures.CaseMigrationType, caseMigration.CaseMigrationTypeId);
            if (string.IsNullOrEmpty(epep.MigrationType))
            {
                SetErrorToMQ(mq, IntegrationStates.DataContentError, $"Невалиден вид движение {caseMigration.CaseMigrationTypeId}");
                return;
            }
            if (caseMigration.CaseSessionActId > 0)
            {
                epep.ActId = getKeyGuid(SourceTypeSelectVM.CaseSessionAct, caseMigration.CaseSessionActId);
                if (epep.ActId == Guid.Empty)
                {
                    SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на акт");
                    return;
                }
                List<Guid> complainDocumentList = new List<Guid>();
                var complainDocuments = await repo.AllReadonly<CaseSessionActComplain>()
                                    .Where(x => x.CaseSessionActId == caseMigration.CaseSessionActId)
                                    .Select(x => x.ComplainDocumentId)
                                    .ToArrayAsync();
                foreach (var item in complainDocuments)
                {
                    var docGid = getKeyGuid(SourceTypeSelectVM.Document, item);
                    if (docGid == Guid.Empty)
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на документ за обжалване");
                        return;
                    }
                    complainDocumentList.Add(docGid);
                }

                epep.IncomingDocuments = complainDocumentList.ToArray();
            }
            if (caseMigration.OutDocumentId > 0)
            {
                epep.OutgoingDocumentId = getKeyGuid(SourceTypeSelectVM.Document, caseMigration.OutDocumentId);
            }
            if (epep.OutgoingDocumentId == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на изходящ документ");
                return;
            }
            if (AddIntegrationKey(mq, await serviceClient.InsertCaseMigrationAsync(epep), false))
            {
                caseMigration.MigrationKind = NomenclatureConstants.CaseMigrationKinds.EpepInMigration;
                await repo.SaveChangesAsync();
            }
        }

        private async Task Send_Act(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<Act>(Encoding.UTF8.GetString(mq.Content));
            if (epep.CaseId == Guid.Empty)
            {
                var caseSessionAct = repo.GetById<CaseSessionAct>((int)mq.SourceId);
                epep.CaseId = getKeyGuid(SourceTypeSelectVM.Case, caseSessionAct.CaseId);
            }
            if (epep.CaseId == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на дело");
                return;
            }

            epep.ActId = getKeyGuidNullable(SourceTypeSelectVM.CaseSessionAct, mq.SourceId);
            if (!epep.ActId.IsEmpty() && mq.MethodName == EpepConstants.Methods.Add)
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }
            epep.HearingId = getKeyGuidNullable(SourceTypeSelectVM.CaseSession, mq.ParentSourceId ?? 0);
            if (epep.HearingId.IsEmpty())
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на заседание");
                return;
            }


            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, await serviceClient.InsertActAsync(epep), true);
                    if (mq.IntegrationStateId == EpepConstants.IntegrationStates.TransferOK)
                    {
                        await send_ActPreparator((int)mq.SourceId);
                    }
                    break;
                case EpepConstants.Methods.Update:
                    if (epep.ActId == Guid.Empty)
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, await serviceClient.UpdateActAsync(epep));
                    await send_ActPreparator((int)mq.SourceId);
                    break;
                case EpepConstants.Methods.Delete:
                    if (epep.ActId == Guid.Empty)
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, await serviceClient.DeleteActAsync(epep.ActId.Value));
                    break;
            }
        }
        private async Task send_ActPreparator(int actId)
        {
            var EpepActId = getKeyGuid(SourceTypeSelectVM.CaseSessionAct, actId);
            if (EpepActId == Guid.Empty)
            {
                return;
            }
            var act = repo.GetById<CaseSessionAct>(actId);
            var sessionLawUnits = await repo.AllReadonly<CaseLawUnit>()
                                    .Where(x => x.CaseId == act.CaseId && x.CaseSessionId == act.CaseSessionId)
                                    .Where(x => NomenclatureConstants.JudgeRole.JudgeRolesListMain.Contains(x.JudgeRoleId))
                                    .Where(x => (x.DateTo ?? DateTime.MaxValue) >= act.RegDate)
                                    .OrderBy(x => x.JudgeRoleId)
                                    .ThenBy(x => x.DateFrom)
                                    .Select(x => new
                                    {
                                        Id = x.Id,
                                        FullName = x.LawUnit.FullName,
                                        JudgeRoleId = x.JudgeRoleId,
                                        JudgeRole = x.JudgeDepartmentRole.Label,
                                        SubstituteFor = (x.LawUnitSubstitution != null) ? x.LawUnitSubstitution.LawUnit.FullName : "",
                                        SubstituteReason = (x.LawUnitSubstitution != null) ? x.LawUnitSubstitution.Description : "",
                                    }).ToListAsync();

            try
            {
                var pIds = await serviceClient.GetActPreparatorIdentifiersByActIdAsync(EpepActId);
                foreach (var item in pIds)
                {
                    await serviceClient.DeleteActPreparatorAsync(item);
                    RemoveIntegrationKeys(item.ToString(), SourceTypeSelectVM.CaseSessionActPreparatorByAct, actId);
                }
            }
            catch { }

            foreach (var lawUnit in sessionLawUnits)
            {
                ActPreparator epep = new ActPreparator()
                {
                    ActId = EpepActId,
                    JudgeName = lawUnit.FullName,
                    //Ако не се изпрати точно този стринг ЕПЕП не го визуализира като Съдия докладчик в списъка на актовете по делото и заседанието
                    Role = (lawUnit.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter) ? "Съдия докладчик" : lawUnit.JudgeRole
                };

                if (!string.IsNullOrEmpty(lawUnit.SubstituteFor))
                {
                    epep.SubstituteFor = lawUnit.SubstituteFor.TrimLength(200);
                    epep.SubstituteReason = lawUnit.SubstituteReason.TrimLength(200);
                }

                await serviceClient.InsertActPreparatorAsync(epep);
            }
        }

        private async Task Send_PrivateActFile(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<PrivateActFile>(Encoding.UTF8.GetString(mq.Content));

            epep.ActId = getKeyGuid(SourceTypeSelectVM.CaseSessionAct, mq.SourceId);
            if (epep.ActId == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на акт");
                return;
            }

            epep.PrivateActFileId = getKeyGuidNullable(SourceTypeSelectVM.CaseSessionActPdf, mq.SourceId);
            if (!epep.PrivateActFileId.IsEmpty())
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }

            var fileModel = await cdnService.MongoCdn_Download(new CdnFileSelect() { SourceType = mq.SourceType, SourceId = mq.SourceId.ToString() }, CdnFileSelect.PostProcess.Flatten);
            if (fileModel == null || string.IsNullOrEmpty(fileModel?.FileContentBase64))
            {
                SetErrorToMQ(mq, IntegrationStates.MissingObjectEISS, "Грешен или липсващ файл.");
                return;
            }

            epep.PrivateActMimeType = fileModel.ContentType;
            epep.PrivateActContent = fileModel.GetBytes();


            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, await serviceClient.InsertPrivateActFileAsync(epep), false);
                    break;
                case EpepConstants.Methods.Update:
                    if (epep.PrivateActFileId == Guid.Empty)
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, await serviceClient.UpdatePrivateActFileAsync(epep));
                    break;

            }
        }

        private async Task Send_PublicActFile(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<PublicActFile>(Encoding.UTF8.GetString(mq.Content));

            epep.ActId = getKeyGuid(SourceTypeSelectVM.CaseSessionAct, mq.SourceId);
            if (epep.ActId == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на акт");
                return;
            }

            epep.PublicActFileId = getKeyGuidNullable(SourceTypeSelectVM.CaseSessionActDepersonalized, mq.SourceId);
            if (mq.MethodName == EpepConstants.Methods.Add && !epep.PublicActFileId.IsEmpty())
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }
            if (mq.MethodName != EpepConstants.Methods.Delete)
            {
                var fileModel = await cdnService.MongoCdn_Download(new CdnFileSelect() { SourceType = mq.SourceType, SourceId = mq.SourceId.ToString() });
                if (fileModel == null || string.IsNullOrEmpty(fileModel?.FileContentBase64))
                {
                    SetErrorToMQ(mq, IntegrationStates.MissingObjectEISS, "Грешен или липсващ файл.");
                    return;
                }
                epep.PublicActMimeType = fileModel.ContentType;
                epep.PublicActContent = fileModel.GetBytes();
            }
            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, await serviceClient.InsertPublicActFileAsync(epep), false);
                    break;
                case EpepConstants.Methods.Update:
                    if (epep.PublicActFileId == Guid.Empty)
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, await serviceClient.UpdatePublicActFileAsync(epep));
                    break;
                case EpepConstants.Methods.Delete:
                    RemoveUnfinishedTasksBeforeDelete(mq);
                    if (epep.PublicActFileId == Guid.Empty)
                    {
                        UpdateMQ(mq, true);
                        return;
                    }
                    UpdateMQ(mq, await serviceClient.DeletePublicActFileAsync(epep.ActId));
                    RemoveIntegrationKeys(mq);
                    break;
            }

        }



        private async Task Send_PrivateMotiveFile(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<PrivateMotiveFile>(Encoding.UTF8.GetString(mq.Content));

            epep.ActId = getKeyGuid(SourceTypeSelectVM.CaseSessionAct, mq.SourceId);
            if (epep.ActId == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на акт");
                return;
            }

            epep.PrivateMotiveFileId = getKeyGuidNullable(SourceTypeSelectVM.CaseSessionActMotivePdf, mq.SourceId);
            if (!epep.PrivateMotiveFileId.IsEmpty())
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }
            var fileModel = await cdnService.MongoCdn_Download(new CdnFileSelect() { SourceType = mq.SourceType, SourceId = mq.SourceId.ToString() }, CdnFileSelect.PostProcess.Flatten);
            if (fileModel == null || string.IsNullOrEmpty(fileModel?.FileContentBase64))
            {
                SetErrorToMQ(mq, IntegrationStates.MissingObjectEISS, "Грешен или липсващ файл.");
                return;
            }

            epep.PrivateMotiveMimeType = fileModel.ContentType;
            epep.PrivateMotiveContent = fileModel.GetBytes();
            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, await serviceClient.InsertPrivateMotiveFileAsync(epep), false);
                    break;
                case EpepConstants.Methods.Update:
                    if (epep.PrivateMotiveFileId == Guid.Empty)
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, await serviceClient.UpdatePrivateMotiveFileAsync(epep));
                    break;
            }
        }

        private async Task Send_PublicMotiveFile(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<PublicMotiveFile>(Encoding.UTF8.GetString(mq.Content));

            epep.ActId = getKeyGuid(SourceTypeSelectVM.CaseSessionAct, mq.SourceId);
            if (epep.ActId == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на акт");
                return;
            }

            epep.PublicMotiveFileId = getKeyGuidNullable(SourceTypeSelectVM.CaseSessionActMotiveDepersonalized, mq.SourceId);
            if (mq.MethodName == EpepConstants.Methods.Add && !epep.PublicMotiveFileId.IsEmpty())
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }
            if (mq.MethodName != EpepConstants.Methods.Delete)
            {
                var fileModel = await cdnService.MongoCdn_Download(new CdnFileSelect() { SourceType = mq.SourceType, SourceId = mq.SourceId.ToString() });
                if (fileModel == null || string.IsNullOrEmpty(fileModel?.FileContentBase64))
                {
                    SetErrorToMQ(mq, IntegrationStates.MissingObjectEISS, "Грешен или липсващ файл.");
                    return;
                }
                epep.PublicMotiveMimeType = fileModel.ContentType;
                epep.PublicMotiveContent = fileModel.GetBytes();
            }

            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, await serviceClient.InsertPublicMotiveFileAsync(epep), false);
                    break;
                case EpepConstants.Methods.Update:
                    if (epep.PublicMotiveFileId == Guid.Empty)
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, await serviceClient.UpdatePublicMotiveFileAsync(epep));
                    break;
                case EpepConstants.Methods.Delete:
                    RemoveUnfinishedTasksBeforeDelete(mq);
                    if (epep.PublicMotiveFileId == Guid.Empty)
                    {
                        UpdateMQ(mq, true);
                        return;
                    }

                    UpdateMQ(mq, await serviceClient.DeletePublicMotiveFileAsync(epep.ActId));
                    RemoveIntegrationKeys(mq);
                    break;
            }

        }

        private async Task Send_Appeal(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<Appeal>(Encoding.UTF8.GetString(mq.Content));
            epep.ActId = getKeyGuid(SourceTypeSelectVM.CaseSessionAct, mq.ParentSourceId);
            if (epep.ActId == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на акт");
                return;
            }
            if (epep.SideId == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на страна по дело");
                return;
            }
            epep.AppealId = getKeyGuidNullable(SourceTypeSelectVM.CaseSessionActComplain, mq.SourceId);
            if (!epep.AppealId.IsEmpty() && mq.MethodName == EpepConstants.Methods.Add)
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }


            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, await serviceClient.InsertAppealAsync(epep), false);
                    break;
                case EpepConstants.Methods.Update:
                    if (epep.ActId == Guid.Empty)
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, await serviceClient.UpdateAppealAsync(epep));
                    break;
                case EpepConstants.Methods.Delete:
                    if (epep.AppealId.IsEmpty())
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, await serviceClient.DeleteAppealAsync(epep.AppealId.Value));
                    break;
            }

        }


        public override async Task<bool> FetchResult()
        {
            try
            {
                if (!await InitChanel())
                {
                    return false;
                }
                try
                {
                    await ManageSummons();
                }
                catch (Exception ex) { }
                if (UPGRADE_EPEP_MIGRATEUSERS)
                {
                    try
                    {
                        await UpdateUserRegistrations();
                    }
                    catch (Exception ex) { }
                    try
                    {
                        await FetchElectronicDocuments();
                    }
                    catch (Exception ex) { }
                }
            }
            finally
            {
                await CloseChanel();
            }
            return true;
        }



        public async Task ManageSummons()
        {

            //извлича всички изпратени, неизтрити призовки, с начин на доставка през ЕПЕП
            var epepNotifications = await repo.All<CaseNotification>()
                                        .Include(x => x.CaseSession)
                                        .Where(x => x.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.ByEPEP)
                                        .Where(x => x.DeliveryDate == null && x.ReturnDate == null)
                                        .Where(x => x.DatePrint != null)
                                        .Where(x => x.DateSend != null)
                                        .Where(x => x.DateExpired == null)
                                        .OrderBy(x => x.Id)
                                        .ToListAsync();

            foreach (var epepNotification in epepNotifications)
            {
                try
                {



                    //Взема кода към ЕПЕП на призовката, ако има
                    var epepKey = getKeyGuid(SourceTypeSelectVM.CaseNotification, epepNotification.Id);

                    if (epepKey == Guid.Empty)
                    {
                        continue;
                    }

                    //Взема датата на връчване от портала
                    DateTime deliveryDate = DateTime.MinValue;
                    DateTime checkFromDate = epepNotification.DateSend.Value;

                    if (UPGRADE_EPEP)
                    {
                        //Нов метод, връща резултат дали е прочетена призовката и дата до на отсъствие, в случай на адвокат
                        var summonRes = await serviceClient.GetSummonsReadTimestampV3Async(epepKey);
                        if (summonRes.IsRead && summonRes.ReadDate.HasValue)
                        {
                            deliveryDate = summonRes.ReadDate.Value;
                        }
                        else
                        {
                            //С последните промени в ГПК, тези отсъствия на адвокатите, които са ни по задание не трябва да се отчитат при призоваването
                            if (UPGRADE_EPEP_LawyerVacations && summonRes.VacationEndDate.HasValue)
                            {
                                if (summonRes.VacationEndDate.Value > checkFromDate)
                                {
                                    checkFromDate = summonRes.VacationEndDate.Value;
                                }
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            deliveryDate = await serviceClient.GetSummonsServedTimestampAsync(epepKey);
                        }
                        catch { }
                    }

                    //ПО ГПК срока е 7 дни+1
                    if (checkFromDate.AddDays(UPGRADE_EPEP_SummonDaysWait).Date <= DateTime.Now
                        //|| epepNotification.DatePrint.Value.AddDays(10).Date <= DateTime.Now
                        )
                    {
                        epepNotification.NotificationStateId = NomenclatureConstants.NotificationState.Delivered;
                        epepNotification.DeliveryDate = DateTime.Now;
                        epepNotification.ReturnDate = DateTime.Now;
                        epepNotification.ReturnInfo = "Автоматично отразяване след изтичане на 7 дневен срок от датата на изпращане.";
                        await deliveryItemService.CreateDeliveryItem(epepNotification, true);
                        await repo.SaveChangesAsync();

                        try
                        {
                            await serviceClient.MarkSummonAsCourtReadAsync(epepKey, epepNotification.DeliveryDate.Value, epepNotification.ReturnInfo);
                        }
                        catch (Exception ex) { }

                        continue;
                    }

                    if (deliveryDate > epepNotification.DateSend)
                    {
                        //успешно призоваване
                        epepNotification.NotificationStateId = NomenclatureConstants.NotificationState.Delivered;
                        epepNotification.DeliveryDate = deliveryDate;
                        epepNotification.ReturnDate = deliveryDate;
                        await deliveryItemService.CreateDeliveryItem(epepNotification, true);
                        await repo.SaveChangesAsync();

                        //Маркира призовката като прочетена
                        await serviceClient.MarkSummonAsReadAsync(epepKey, DateTime.Now);

                        try
                        {
                            var summonReportFile = await serviceClient.GetSummonReportDocumentAsync(epepKey);
                            if (summonReportFile != null)
                            {
                                var uploadRequest = new CdnUploadRequest()
                                {
                                    FileContentBase64 = Convert.ToBase64String(summonReportFile),
                                    SourceType = SourceTypeSelectVM.CaseNotificationReturn,
                                    SourceId = epepNotification.Id.ToString(),
                                    FileName = $"epepReport{epepNotification.RegNumber}.pdf",
                                    ContentType = "application/pdf"

                                };
                                await cdnService.MongoCdn_AppendUpdate(uploadRequest);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, $"Error GetSummonReportDocument. CaseNotificationId = {epepNotification.Id}; Gid={epepKey}");
                        }
                    }
                }
                catch (FaultException fex)
                {
                    var _error = fex.Message;
                    //logger.LogError(fex, $"Error managing summon. FaultException {fex.Message}");
                    //SetErrorToMQ(mq, IntegrationStates.DataContentError, _error);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error managing summon. CaseNotificationId = {epepNotification.Id}");
                }
            }

        }

        public async Task<bool> Correction()
        {
            if (!(await InitChanel()))
            {
                return false;
            }

            var result = false;

            var model = repo.All<MQEpep>()
                            .Where(x => x.TargetClassName == "Case")
                            .Where(x => x.IntegrationTypeId == 2)
                            .Where(x => x.MethodName == "add")
                            .Where(x => x.DateTransfered == null)
                            .Where(x => x.IntegrationStateId == 18)
                            .ToList();

            foreach (var mq in model)
            {

                var epep = JsonConvert.DeserializeObject<Integration.Epep.Case>(Encoding.UTF8.GetString(mq.Content));

                epep.IncomingDocumentId = getKeyGuid(SourceTypeSelectVM.Document, mq.ParentSourceId);

                try
                {
                    Guid? caseId;
                    caseId = await serviceClient.InsertCaseAsync(epep);
                    AddIntegrationKey(mq, caseId, false);
                }
                catch (FaultException fex)
                {
                    var _error = fex.GetMessageFault();
                    SetErrorToMQ(mq, IntegrationStates.DataContentError, _error);
                }
                catch (Exception ex)
                {
                    if (logger != null)
                    {
                        logger.LogError(ex, ex.Message);
                    }
                    SetErrorToMQ(mq, IntegrationStates.TransferError, $"Exception: {ex.Message}");
                }
                //var doc = repo.AllReadonly<IOWebApplication.Infrastructure.Data.Models.Cases.Case>()
                //                        .Include(x => x.Document)
                //                        .Where(x => x.Id == mq.SourceId)
                //                        .Select(x => x.Document)
                //                        .FirstOrDefault();

                //try
                //{
                //    var caseId = await serviceClient.GetCaseIdAsync(doc.DocumentNumberValue.Value, doc.DocumentDate.Year, epep.CourtCode);
                //    if (caseId.HasValue)
                //    {
                //        AddIntegrationKey(mq, caseId);
                //        mq.DateTransfered = DateTime.Now;
                //        mq.IntegrationStateId = IntegrationStates.TransferOK;
                //        mq.ErrorDescription = null;
                //        mq.ErrorCount = 0;
                //        repo.Update(mq);
                //        repo.SaveChanges();
                //        result = true;
                //    }
                //}
                //catch (Exception ex) { }

            }
            ;

            await CloseChanel();

            return result;
        }


        private EpepSummonInfoVM getEpepSummonInfo(CaseNotification model)
        {

            int? casePersonId = null;
            if (model.CasePersonL3Id > 0)
            {
                casePersonId = getCasePersonIdFromCase(model.CasePersonL3Id ?? 0);
            }

            if (!casePersonId.HasValue && model.CasePersonL2Id > 0)
            {
                casePersonId = getCasePersonIdFromCase(model.CasePersonL2Id ?? 0);
            }

            if (!casePersonId.HasValue && model.CasePersonL1Id > 0)
            {
                casePersonId = getCasePersonIdFromCase(model.CasePersonL1Id ?? 0);
            }

            if (!casePersonId.HasValue && model.CasePersonId > 0)
            {
                casePersonId = getCasePersonIdFromCase(model.CasePersonId ?? 0);
            }

            if (!casePersonId.HasValue)
            {
                return null;
            }

            var result = new EpepSummonInfoVM()
            {
                CasePersonId = casePersonId.Value,
                EpepUserId = repo.AllReadonly<EpepUserAssignment>()
                                    .Where(x => x.CasePersonId == casePersonId && x.DateExpired == null)
                                    .Where(x => x.CanSummon == true)
                                    .Select(x => x.EpepUserId)
                                    .FirstOrDefault()
            };

            return result;
        }

        private int? getCasePersonIdFromCase(int id, bool forSummon = true)
        {
            var _model = repo.AllReadonly<CasePerson>()
                        .Where(x => x.Id == id)
                        .Select(x => new { x.CaseSessionId, x.CasePersonIdentificator })
                        .FirstOrDefault();

            if (_model == null)
            {
                return null;
            }

            int casePersonId = 0;

            if (_model.CaseSessionId == null)
            {
                casePersonId = id;
            }
            else
            {
                casePersonId = repo.GetPropById<CasePerson, int>(x => x.CasePersonIdentificator == _model.CasePersonIdentificator && x.CaseSessionId == null, x => x.Id);
            }

            if (!forSummon)
            {
                return casePersonId;
            }

            if (repo.AllReadonly<EpepUserAssignment>().Where(x => x.CasePersonId == casePersonId && x.DateExpired == null && x.CanSummon == true).Any())
            {
                return casePersonId;
            }

            return null;
        }

        /// <summary>
        /// Прикачени документи - особено мнение, обезличено особено мнение, документи в заседание
        /// </summary>
        /// <param name="mq"></param>
        /// <returns></returns>
        private async Task Send_AttachedDocument(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<AttachedDocument>(Encoding.UTF8.GetString(mq.Content));
            string fileId = null;
            if (mq.MethodName == EpepConstants.Methods.Add)
                switch (mq.SourceType)
                {
                    case SourceTypeSelectVM.Files:
                        {
                            var fileInfo = repo.AllReadonly<MongoFile>()
                                                    .Where(x => x.Id == (int)mq.SourceId)
                                                    .Select(x => new { x.SourceType, x.FileId })
                                                    .FirstOrDefault();
                            fileId = fileInfo.FileId;
                            switch (fileInfo.SourceType)
                            {
                                case SourceTypeSelectVM.CaseSessionFastDocument:
                                    epep.Type = EpepConstants.AttachedDocumentTypes.SessionFastDocument;
                                    epep.ParentId = getKeyGuid(SourceTypeSelectVM.CaseSessionFastDocument, mq.ParentSourceId);
                                    if (epep.ParentId == Guid.Empty)
                                    {
                                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на документ в заседание");
                                        return;
                                    }
                                    break;
                                case SourceTypeSelectVM.CaseSessionActManualUpload:
                                    epep.Type = EpepConstants.AttachedDocumentTypes.ActManualFile;
                                    epep.ParentId = getKeyGuid(SourceTypeSelectVM.CaseSessionAct, mq.ParentSourceId);
                                    if (epep.ParentId == Guid.Empty)
                                    {
                                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на акт");
                                        return;
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    //само особеното мнение се прикачва по sourcetype/sourceid на обекта, останалите са файлове към обекти
                    case SourceTypeSelectVM.CaseSessionActCoordinationPdf:
                    case SourceTypeSelectVM.CaseSessionActCoordinationDepersonalizedPdf:
                        epep.ParentId = getKeyGuid(SourceTypeSelectVM.CaseSessionAct, mq.ParentSourceId);
                        if (epep.ParentId == Guid.Empty)
                        {
                            SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на акт");
                            return;
                        }
                        break;
                    default:
                        break;
                }

            if (epep.Type == 0)
                switch (mq.SourceType)
                {
                    case SourceTypeSelectVM.CaseSessionActCoordinationPdf:
                        epep.Type = EpepConstants.AttachedDocumentTypes.ActCoordination;
                        break;
                    case SourceTypeSelectVM.CaseSessionActCoordinationDepersonalizedPdf:
                        epep.Type = EpepConstants.AttachedDocumentTypes.ActCoordinationPublic;
                        break;
                }

            epep.AttachedDocumentId = getKeyGuidNullable(mq.SourceType, mq.SourceId);
            if (mq.MethodName == EpepConstants.Methods.Add)
            {


                var fileModel = await cdnService.MongoCdn_Download(new CdnFileSelect() { FileId = fileId, SourceType = mq.SourceType, SourceId = mq.SourceId.ToString() }, CdnFileSelect.PostProcess.Flatten);
                if (fileModel == null || string.IsNullOrEmpty(fileModel?.FileContentBase64))
                {
                    SetErrorToMQ(mq, IntegrationStates.MissingObjectEISS, "Грешен или липсващ файл.");
                    return;
                }

                epep.FileTitle = fileModel.FileTitle;
                epep.FileName = fileModel.FileName;
                epep.FileDate = fileModel.DateUploaded;

                epep.MimeType = fileModel.ContentType;
                epep.FileContent = fileModel.GetBytes();
            }

            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    if (!epep.AttachedDocumentId.IsEmpty())
                    {
                        if (await serviceClient.DeleteAttachedDocumentAsync(epep.AttachedDocumentId.Value))
                        {
                            RemoveIntegrationKeys(mq);
                            epep.AttachedDocumentId = Guid.Empty;
                        }
                        else
                        {
                            SetErrorToMQ(mq, IntegrationStates.DataContentError, "Грешка при подмяна на файл");
                            return;
                        }
                    }
                    AddIntegrationKey(mq, await serviceClient.InsertAttachedDocumentAsync(epep), false);
                    break;
                case EpepConstants.Methods.Delete:
                    if (epep.AttachedDocumentId.IsEmpty())
                    {
                        if (mq.ErrorCount > IntegrationMaxErrorCount / 2)
                        {
                            UpdateMQ(mq, true);
                            return;
                        }

                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, await serviceClient.DeleteAttachedDocumentAsync(epep.AttachedDocumentId.Value));
                    if (mq.IntegrationStateId == IntegrationStates.TransferOK)
                    {
                        RemoveIntegrationKeys(mq);
                    }
                    break;

            }
        }

        public async Task UpdateUserRegistrations()
        {
            if (!UPGRADE_EPEP)
            {
                return;
            }
            string formatTS = "yyyy-MM-dd@HH:mm:ss:fff";
            DateTime dtMinDate = new DateTime(1900, 1, 1);
            var lastSyncDateTxt = getKey(SourceTypeSelectVM.EpepLastUpdate, 1);
            if (string.IsNullOrEmpty(lastSyncDateTxt))
            {
                lastSyncDateTxt = dtMinDate.ToString(formatTS);
            }

            DateTime fromDate = Utils.SafeParseDate(lastSyncDateTxt, formatTS) ?? dtMinDate;

            UserRegistration[] userRegistrations = null;
            try
            {
                userRegistrations = (await serviceClient.GetUserRegistrationsAsync(fromDate)).Take(UPGRADE_EPEP_MIGRATEUSERS_FETCH).ToArray();
            }
            catch (Exception ex)
            {
                logger.LogError($"UpdateUserRegistrations - {ex.Message};{ex.InnerException?.Message}");
                return;
            }

            if (userRegistrations == null || userRegistrations.Length == 0)
            {
                return;
            }

            foreach (var _user in userRegistrations)
            {
                string changeLog = "";
                var savedEpep = repo.All<EpepUser>().FirstOrDefault(x => x.EpepId == _user.UserRegistrationId);
                if (savedEpep != null)
                {
                    //Редакция
                    if (savedEpep.FullName != _user.Name)
                    {
                        changeLog += $"Имена: {_user.Name} ({savedEpep.FullName});";
                        savedEpep.FullName = _user.Name;
                    }
                    if (!string.IsNullOrEmpty(_user.LawyerNumber))
                    {
                        if (savedEpep.EpepUserTypeId == EpepConstants.UserTypes.Person)
                        {
                            savedEpep.Description = $"АН: {_user.LawyerNumber}";
                        }
                        if (savedEpep.Uic != _user.UIC)
                        {
                            changeLog += $"ЕГН: * (*);";
                            savedEpep.Uic = _user.UIC;
                        }
                        if (savedEpep.LawyerNumber != _user.LawyerNumber)
                        {
                            changeLog += $"АН: {_user.LawyerNumber} ({savedEpep.LawyerNumber});";
                            savedEpep.LawyerNumber = _user.LawyerNumber;
                        }
                    }
                    else
                    {
                        if (savedEpep.Uic != _user.UIC)
                        {
                            changeLog += $"Идентификатор: {_user.UIC} ({savedEpep.Uic});";
                            savedEpep.Uic = _user.UIC;
                        }
                    }
                    if (savedEpep.Email != _user.Email)
                    {
                        changeLog += $"Имейл: {_user.Email} ({savedEpep.Email});";
                        savedEpep.Email = _user.Email;
                    }
                    if (!string.IsNullOrEmpty(changeLog))
                    {
                        changeLog = "Автоматична промяна на профил от ЕПЕП: " + changeLog;
                        saveLogOperForEpepUser(savedEpep.Id, changeLog, OperationTypes.Update);
                    }
                }
                else
                {
                    var newUser = new EpepUser()
                    {
                        Uic = _user.UIC,
                        LawyerNumber = _user.LawyerNumber,
                        EpepId = _user.UserRegistrationId,
                        FullName = _user.Name,
                        EpepUserTypeId = _user.UserType,
                        Email = _user.Email,
                        DateWrt = _user.ModifyDate
                    };
                    repo.Add(newUser);
                    await repo.SaveChangesAsync();
                    AddIntegrationKey(SourceTypeSelectVM.EpepUser, newUser.Id, _user.UserRegistrationId.ToString());
                    changeLog = "Автоматично добавяне на профил от ЕПЕП: ";
                    var userTypeName = repo.GetPropById<EpepUserType, string>(x => x.Id == _user.UserType, x => x.Label);
                    changeLog += $"{userTypeName}: {_user.Name} {_user.UIC}, {_user.Email};";
                    saveLogOperForEpepUser(newUser.Id, changeLog, OperationTypes.Insert);
                }
            }
            var maxModifyDate = userRegistrations.Select(x => x.ModifyDate).Max();
            AppendUpdateIntegrationKey(SourceTypeSelectVM.EpepLastUpdate, 1, maxModifyDate.ToString(formatTS));
        }


        public async Task FetchElectronicDocuments()
        {
            if (!UPGRADE_EPEP)
            {
                return;
            }
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
                item.Code = epepCourtMap.Where(x => x.InnerCode == item.Id.ToString()).Select(x => x.OuterCode).FirstOrDefault();
            }

            List<Guid> documentIdentifiers = new List<Guid>();
            foreach (var court in courtsSupported)
            {
                Guid[] documentIdentifiersForCourt = await serviceClient.GetNewElectronicDocumentIdentifiersAsync(court.Code);
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

                Integration.Epep.ElectronicDocument epepDocData = null;
                try
                {
                    epepDocData = await serviceClient.GetElectronicDocumentAsync(docId);
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

                var mappedDocument = mapElectronicDocumentToEiss(forLog, epepDocData, courtsSupported, docKindList, docGroupList, feeTypes);

                if (mappedDocument != null && string.IsNullOrEmpty(mappedDocument.MapErrorDescription))
                {
                    using (var ts = repo.BeginTransaction())
                    {
                        repo.Add(mappedDocument);
                        await repo.SaveChangesAsync();
                        if (epepDocData.Files != null)
                        {
                            foreach (var file in epepDocData.Files)
                            {
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

                        var group = new MainGroup(mappedDocument.CourtId, SourceTypeSelectVM.ElectronicDocument, mappedDocument.Id);
                        repo.Add(group);
                        await repo.SaveChangesAsync();
                        group.LastTransation = new MainTransaction(group.Id, NomenclatureConstants.MainTransactionTypes.Waiting);
                        await repo.SaveChangesAsync();

                        if (await serviceClient.UpdateElectronicDocumentSetDateCourtAcceptAsync(docId, mappedDocument.DateCourtAccept.Value))
                        {
                            ts.Commit();
                        }
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

        IOWebApplication.Infrastructure.Data.Models.Documents.ElectronicDocument mapElectronicDocumentToEiss(
            bool forLog,
            Integration.Epep.ElectronicDocument electronicDocument,
            List<BaseCommonNomenclature> courtList,
            List<BaseCommonNomenclature> docKindList,
            List<DocumentGroup> docGroupList,
            List<BaseCommonNomenclature> feeTypes
            )
        {


            var doc = new IOWebApplication.Infrastructure.Data.Models.Documents.ElectronicDocument()
            {
                CourtId = courtList.Where(x => x.Code == electronicDocument.CourtCode).Select(x => x.Id).FirstOrDefault(),
                //DocumentKindId = docKindList.Where(x => x.Code == electronicDocument.DocumentKind).Select(x => x.Id).FirstOrDefault(),
                //DocumentGroupId = docGroupList.Where(x => x.Code == electronicDocument.DocumentType).Select(x => x.Id).FirstOrDefault(),
                EpepId = electronicDocument.ElectronicDocumentId,
                EpepUserId = repo.AllReadonly<EpepUser>().Where(x => x.EpepId == electronicDocument.UserRegistrationId).Select(x => x.Id).FirstOrDefault(),
                ApplyNumber = electronicDocument.NumberApply,
                ApplyDate = electronicDocument.DateApply,
                Description = electronicDocument.Description,
                CurrencyCode = electronicDocument.CurrencyCode,
                PaidDate = electronicDocument.DatePaid,
                DateCourtAccept = DateTime.Now
            };

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
                var docPerson = new IOWebApplication.Infrastructure.Data.Models.Documents.ElectronicDocumentPerson();
                docPerson.PersonRoleId = GetNomIdByOuterCodeInt(EpepConstants.Nomenclatures.PersonRolesFromEPEP, side.SideInvolvementKind);
                if (docPerson.PersonRoleId == 0)
                {
                    docPerson.PersonRoleId = GetNomIdByOuterCodeInt(EpepConstants.Nomenclatures.PersonRoles, side.SideInvolvementKind);
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
                doc.Persons.Add(docPerson);
            }
            if (electronicDocument.Files != null)
            {
                foreach (var file in electronicDocument.Files)
                {
                    if (file.FileSize != file.Content.Length)
                    {
                        doc.FileError = $"File size error! {file.FileName}. Size:{file.FileSize}, Content:{file.Content.Length}";
                    }
                }
            }

            return doc;
        }

        public Task TestFiles()
        {
            throw new NotImplementedException();
        }

        public Task TestSummmon()
        {
            throw new NotImplementedException();
        }

        public Task UpdateEkStreets()
        {
            throw new NotImplementedException();
        }

        public Task SummonRecover_Report()
        {
            throw new NotImplementedException();
        }
    }
}



