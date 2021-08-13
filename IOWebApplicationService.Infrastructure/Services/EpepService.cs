using Integration.Epep;
using IO.LogOperation.Models;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplicationService.Infrastructure.Contracts;
using IOWebApplicationService.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using static IOWebApplication.Infrastructure.Constants.EpepConstants;

namespace IOWebApplicationService.Infrastructure.Services
{
    public class EpepService : BaseMQService, IEpepService
    {
        protected IEpepConnectionService connector;
        protected IeCaseServiceClient serviceClient;


        public EpepService(
            IRepository _repo,
            IEpepConnectionService _connector,
            ILogger<EpepService> _logger,
            ICdnService _cdnService)
        {
            repo = _repo;
            connector = _connector;
            cdnService = _cdnService;
            logger = _logger;
            IntegrationTypeId = NomenclatureConstants.IntegrationTypes.EPEP;
            //this.mqID = 1581769;
        }
        protected override async Task<bool> InitChanel()
        {
            serviceClient = await connector.Connect();

            return serviceClient != null;
        }

        protected override async Task CloseChanel()
        {
            await ManageSummons(this.fetchCount);
            await serviceClient.CloseAsync();
        }

        protected override IEnumerable<MQEpep> FetchHighPriorityItems(int fetchCount)
        {
            //return null;
            int[] highPriorityAddSourceTypes = { SourceTypeSelectVM.EpepUser };
            int[] highPriorityDeleteSourceTypes = { SourceTypeSelectVM.CaseSessionActDepersonalized, SourceTypeSelectVM.CaseSessionActMotiveDepersonalized };
            
            var select = repo.All<MQEpep>()
                         .Where(x => x.IntegrationTypeId == IntegrationTypeId)
                         .Where(x => x.DateTransfered == null && x.IntegrationStateId == IntegrationStates.New);

            return select
                         .Where(x => (highPriorityAddSourceTypes.Contains(x.SourceType) && x.MethodName == EpepConstants.Methods.Add))
                         .Union(select
                         .Where(x => (highPriorityDeleteSourceTypes.Contains(x.SourceType) && x.MethodName == EpepConstants.Methods.Delete)))
                         .OrderBy(x => x.Id)
                         .Take(fetchCount)
                         .ToList();
        }


        protected override async Task SendMQ(MQEpep mq)
        {
            DateTime lastDate = DateTime.Now;

            this.startTime = DateTime.Now;
            switch (mq.TargetClassName)
            {
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
                    await send_ActPreparator((int)mq.SourceId, mq);
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

                    AddIntegrationKey(mq, await serviceClient.InsertPersonRegistrationAsync(epep), false);
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

        private void saveLogOperForEpepUser(int epepUserId, string html)
        {
            var logOper = new LogOperation()
            {
                ActionName = "epepuser_edit",
                Controller = "epep",
                ObjectKey = epepUserId.ToString(),
                OperationTypeID = (int)OperationTypes.Patch,
                OperationDate = DateTime.Now,
                UserData = html,
                OperationUser = "ЕИСС"
            };
            repo.Add(logOper);
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
                                repo.SaveChanges();
                            }
                            return;
                        }
                    }
                    catch (FaultException fex)
                    {

                    }

                    AddIntegrationKey(mq, await serviceClient.InsertLawyerRegistrationAsync(epep), false);
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
            var doc = repo.AllReadonly<Document>()
                                .Include(x => x.DocumentGroup)
                                .Include(x => x.DocumentCaseInfo)
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
                                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
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
            if (epep.Person != null)
            {
                epep.Person.Firstname = epep.Person.Firstname ?? " ";
                epep.Person.Lastname = epep.Person.Lastname ?? ".";
            }
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
        private async Task Send_OutgoingDocument(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<OutgoingDocument>(Encoding.UTF8.GetString(mq.Content));
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

        private async Task Send_IncomingDocumentFile(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<IncomingDocumentFile>(Encoding.UTF8.GetString(mq.Content));


            epep.IncomingDocumentId = getKeyGuid(SourceTypeSelectVM.Document, mq.ParentSourceId);

            if (epep.IncomingDocumentId == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                return;
            }

            var fileModel = await cdnService.MongoCdn_Download(mq.SourceId);
            if (fileModel == null)
            {
                mq.ErrorDescription = $"Грешен файл с ID={mq.SourceId}";
                SetErrorToMQ(mq, IntegrationStates.MissingObjectEISS);
                return;
            }
            epep.IncomingDocumentMimeType = fileModel.ContentType;
            epep.IncomingDocumentContent = fileModel.GetBytes();
            epep.IncomingDocumentFileId = getKeyGuidNullable(SourceTypeSelectVM.Files, mq.SourceId);
            if (mq.MethodName == EpepConstants.Methods.Add && !epep.IncomingDocumentFileId.IsEmpty())
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }

            if (mq.MethodName != EpepConstants.Methods.Add && epep.IncomingDocumentFileId.IsEmpty())
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                return;
            }

            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    if (epep.IncomingDocumentFileId.IsEmpty())
                    {
                        epep.IncomingDocumentFileId = null;
                    }
                    AddIntegrationKey(mq, await serviceClient.InsertIncomingDocumentFileAsync(epep), false);
                    break;
                case EpepConstants.Methods.Update:
                    UpdateMQ(mq, await serviceClient.UpdateIncomingDocumentFileAsync(epep));
                    break;
                case EpepConstants.Methods.Delete:
                    UpdateMQ(mq, await serviceClient.DeleteIncomingDocumentFileAsync(epep.IncomingDocumentId));
                    break;
            }

        }
        private async Task Send_OutgoingDocumentFile(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<OutgoingDocumentFile>(Encoding.UTF8.GetString(mq.Content));

            var docGuid = getKey(SourceTypeSelectVM.Document, mq.ParentSourceId);

            if (string.IsNullOrEmpty(docGuid))
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                return;
            }
            epep.OutgoingDocumentId = Guid.Parse(docGuid);

            var fileModel = await cdnService.MongoCdn_Download(mq.SourceId);
            if (fileModel == null)
            {
                mq.ErrorDescription = $"Грешен файл с ID={mq.SourceId}";
                SetErrorToMQ(mq, IntegrationStates.MissingObjectEISS);
                return;
            }
            epep.OutgoingDocumentMimeType = fileModel.ContentType;
            epep.OutgoingDocumentContent = fileModel.GetBytes();

            epep.OutgoingDocumentFileId = getKeyGuidNullable(SourceTypeSelectVM.Files, mq.SourceId);
            if (mq.MethodName == EpepConstants.Methods.Add && !epep.OutgoingDocumentFileId.IsEmpty())
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }
            if (mq.MethodName != EpepConstants.Methods.Add && epep.OutgoingDocumentFileId.IsEmpty())
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                return;
            }
            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    if (epep.OutgoingDocumentFileId.IsEmpty())
                    {
                        epep.OutgoingDocumentFileId = null;
                    }
                    AddIntegrationKey(mq, await serviceClient.InsertOutgoingDocumentFileAsync(epep), false);
                    break;
                case EpepConstants.Methods.Update:
                    UpdateMQ(mq, await serviceClient.UpdateOutgoingDocumentFileAsync(epep));
                    break;
                case EpepConstants.Methods.Delete:
                    UpdateMQ(mq, await serviceClient.DeleteOutgoingDocumentFileAsync(epep.OutgoingDocumentId));
                    break;
            }

        }

        private async Task Send_Assignment(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<Assignment>(Encoding.UTF8.GetString(mq.Content));
            var info = repo.AllReadonly<CaseSelectionProtokol>()
                                   .Include(x => x.Case)
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
                    SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                    return;
                }
                epep.IncomingDocumentId = Guid.Parse(docGuid);
            }
            if (epep.CaseId == Guid.Empty)
            {
                var caseGuid = getKey(SourceTypeSelectVM.Case, info.CaseId);

                if (string.IsNullOrEmpty(caseGuid))
                {
                    SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
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
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
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
            var epep = JsonConvert.DeserializeObject<Integration.Epep.Case>(Encoding.UTF8.GetString(mq.Content));


            epep.IncomingDocumentId = getKeyGuid(SourceTypeSelectVM.Document, mq.ParentSourceId);

            if (epep.IncomingDocumentId == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
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
                    if (epep.CaseId == Guid.Empty)
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, await serviceClient.UpdateCaseAsync(epep));
                    break;
                case EpepConstants.Methods.Delete:
                    if (epep.CaseId == Guid.Empty)
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

        private async Task Send_Side(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<Integration.Epep.Side>(Encoding.UTF8.GetString(mq.Content));

            epep.CaseId = getKeyGuid(SourceTypeSelectVM.Case, mq.ParentSourceId);
            if (epep.CaseId == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                return;
            }
            epep.SideId = getKeyGuidNullable(SourceTypeSelectVM.CasePerson, mq.SourceId);
            if (!epep.SideId.IsEmpty() && mq.MethodName == EpepConstants.Methods.Add)
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }
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
        private async Task Send_Reporter(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<Integration.Epep.Reporter>(Encoding.UTF8.GetString(mq.Content));

            epep.CaseId = getKeyGuid(SourceTypeSelectVM.Case, mq.ParentSourceId);
            if (epep.CaseId == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                return;
            }
            epep.ReporterId = getKeyGuidNullable(SourceTypeSelectVM.CaseLawUnit, mq.SourceId);
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
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
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
                                        .Include(x => x.JudgeDepartmentRole)
                                        .Include(x => x.JudgeRole)
                                        .Include(x => x.LawUnit)
                                        .Where(x => x.CaseId == caseSession.CaseId && x.CaseSessionId == caseSession.Id)
                                        .Where(x => NomenclatureConstants.JudgeRole.JudgeRolesListMain.Contains(x.JudgeRoleId))
                                        .OrderBy(x => x.JudgeDepartmentRoleId)
                                        .ThenBy(x => x.JudgeRoleId)
                                        .ThenBy(x => x.DateFrom)
                                        //.Where(x => (x.DateTo ?? DateTime.MaxValue) >= caseSession.DateFrom)
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
                }
            }
            catch (Exception ex) { }
            foreach (var lawUnit in caseLawUnits)
            {
                var luID = getKeyGuidNullable(SourceTypeSelectVM.CaseLawUnit, lawUnit.Id);
                string luMethod = Methods.Add;
                if (!luID.IsEmpty())
                {
                    luMethod = Methods.Update;
                    if (lawUnit.DateTo < caseSession.DateTo)
                    {
                        luMethod = Methods.Delete;
                    }
                }

                switch (luMethod)
                {
                    case Methods.Add:
                        {
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
                        break;

                    case Methods.Delete:
                        {
                            await serviceClient.DeleteHearingParticipantAsync(luID.Value);
                        }
                        break;
                }
            }
        }

        private async Task Send_HearingParticipant(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<HearingParticipant>(Encoding.UTF8.GetString(mq.Content));

            epep.HearingId = getKeyGuid(SourceTypeSelectVM.CaseSession, mq.ParentSourceId);
            if (epep.HearingId == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
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

            var notificationInfo = repo.AllReadonly<CaseNotification>()
                                            .Include(x => x.CasePerson)
                                            .Where(x => x.Id == mq.SourceId)
                                            .Select(x => new
                                            {
                                                CaseId = x.CaseId,
                                                CaseSessionId = x.CaseSessionId,
                                                CasePersonIdentificator = x.CasePerson.CasePersonIdentificator
                                            }).FirstOrDefault();

            var casePersonId = repo.AllReadonly<CasePerson>()
                             .Where(x => x.CasePersonIdentificator == notificationInfo.CasePersonIdentificator && x.CaseSessionId == null)
                             .Select(x => x.Id)
                             .FirstOrDefault();

            epep.SideId = getKeyGuid(SourceTypeSelectVM.CasePerson, casePersonId);
            if (notificationInfo.CaseSessionId != null)
            {
                epep.ParentId = getKeyGuid(SourceTypeSelectVM.CaseSession, notificationInfo.CaseSessionId);
            }
            else
            {
                epep.ParentId = getKeyGuid(SourceTypeSelectVM.Case, notificationInfo.CaseId);
            }
            var epepUser = getKeyGuid(SourceTypeSelectVM.EpepUser, mq.ParentSourceId);
            if (epep.ParentId == Guid.Empty || epep.SideId == Guid.Empty || epepUser == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                return;
            }

            epep.SummonId = getKeyGuidNullable(SourceTypeSelectVM.CaseNotification, mq.SourceId);
            if (!epep.SummonId.IsEmpty() && mq.MethodName == EpepConstants.Methods.Add)
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }

            if (string.IsNullOrEmpty(epep.Subject))
            {
                epep.Subject = epep.SummonKind;
            }

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
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
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
                if (fileModel == null)
                {
                    mq.ErrorDescription = $"Грешен файл с ID={mq.SourceId}";
                    SetErrorToMQ(mq, IntegrationStates.MissingObjectEISS);
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
                    await send_ActPreparator((int)mq.SourceId);
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
        private async Task send_ActPreparator(int actId, MQEpep mq = null)
        {
            var EpepActId = getKeyGuid(SourceTypeSelectVM.CaseSessionAct, actId);
            if (EpepActId == Guid.Empty)
            {
                if (mq != null)
                {
                    SetErrorToMQ(mq, EpepConstants.IntegrationStates.WaitForParentIdError, "Изчаква id на акт.");
                }
                return;
            }
            var act = repo.GetById<CaseSessionAct>(actId);
            var sessionLawUnits = repo.AllReadonly<CaseLawUnit>()
                                    .Include(x => x.JudgeDepartmentRole)
                                    .Include(x => x.LawUnit)
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
                                        JudgeRole = x.JudgeDepartmentRole.Label
                                    }).ToList();

            var preparatorsKey = getKey(SourceTypeSelectVM.CaseSessionActPreparatorByAct, actId);
            if (!string.IsNullOrEmpty(preparatorsKey))
            {
                try
                {
                    var pIds = await serviceClient.GetActPreparatorIdentifiersByActIdAsync(EpepActId);
                    if (pIds.Count() == sessionLawUnits.Count())
                    {
                        //Има същите бройки хора
                        if (mq != null)
                            UpdateMQ(mq, true);

                        return;
                    }
                    foreach (var item in pIds)
                    {
                        await serviceClient.DeleteActPreparatorAsync(item);
                    }
                }
                catch { }
            }

            preparatorsKey = null;
            foreach (var lawUnit in sessionLawUnits)
            {
                ActPreparator epep = new ActPreparator()
                {
                    ActId = EpepActId,
                    JudgeName = lawUnit.FullName,
                    //Ако не се изпрати точно този стринг ЕПЕП не го визуализира като Съдия докладчик в списъка на актовете по делото и заседанието
                    Role = (lawUnit.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter) ? "Съдия докладчик" : lawUnit.JudgeRole
                };

                var _pKey = await serviceClient.InsertActPreparatorAsync(epep);

                if (_pKey.HasValue)
                {
                    preparatorsKey = _pKey.Value.ToString() + ",";
                }
            }
            if (!string.IsNullOrEmpty(preparatorsKey))
            {
                AddIntegrationKey(SourceTypeSelectVM.CaseSessionActPreparatorByAct, actId, preparatorsKey, false);
                if (mq != null)
                    UpdateMQ(mq, true);
            }
        }



        private async Task send_ActPreparatorOld(int actId)
        {
            var ActId = getKeyGuid(SourceTypeSelectVM.CaseSessionAct, actId);
            if (ActId == Guid.Empty)
            {
                return;
            }
            var act = repo.GetById<CaseSessionAct>(actId);
            var sessionLawUnits = repo.AllReadonly<CaseLawUnit>()
                                    .Include(x => x.JudgeDepartmentRole)
                                    .Include(x => x.LawUnit)
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
                                        JudgeRole = x.JudgeDepartmentRole.Label
                                    }).ToList();


            foreach (var lawUnit in sessionLawUnits)
            {

                var preparatorId = getKeyGuidNullable(SourceTypeSelectVM.CaseSessionActPreparator, lawUnit.Id);
                var apMethod = Methods.Add;
                if (preparatorId.HasValue)
                {
                    apMethod = Methods.Update;
                }

                ActPreparator epep = new ActPreparator()
                {
                    ActId = ActId,
                    ActPreparatorId = preparatorId,
                    JudgeName = lawUnit.FullName,
                    //Ако не се изпрати точно този стринг ЕПЕП не го визуализира като Съдия докладчик в списъка на актовете по делото и заседанието
                    Role = (lawUnit.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter) ? "Съдия докладчик" : lawUnit.JudgeRole
                };

                switch (apMethod)
                {
                    case Methods.Add:
                        var apId = await serviceClient.InsertActPreparatorAsync(epep);
                        if (!apId.IsEmpty())
                        {
                            AddIntegrationKey(SourceTypeSelectVM.CaseSessionActPreparator, lawUnit.Id, apId.Value.ToString(), false);
                        }
                        break;
                    case Methods.Update:
                        await serviceClient.UpdateActPreparatorAsync(epep);
                        break;
                }
            }
        }

        private async Task Send_PrivateActFile(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<PrivateActFile>(Encoding.UTF8.GetString(mq.Content));

            epep.ActId = getKeyGuid(SourceTypeSelectVM.CaseSessionAct, mq.SourceId);
            if (epep.ActId == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                return;
            }

            epep.PrivateActFileId = getKeyGuidNullable(SourceTypeSelectVM.CaseSessionActPdf, mq.SourceId);
            if (!epep.PrivateActFileId.IsEmpty())
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }

            var fileModel = await cdnService.MongoCdn_Download(new CdnFileSelect() { SourceType = mq.SourceType, SourceId = mq.SourceId.ToString() }, CdnFileSelect.PostProcess.Flatten);
            if (fileModel == null)
            {
                mq.ErrorDescription = $"Грешен файл с ID={mq.SourceId}";
                SetErrorToMQ(mq, IntegrationStates.MissingObjectEISS);
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
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
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
                if (fileModel == null)
                {
                    SetErrorToMQ(mq, IntegrationStates.MissingObjectEISS);
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
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                return;
            }

            epep.PrivateMotiveFileId = getKeyGuidNullable(SourceTypeSelectVM.CaseSessionActMotivePdf, mq.SourceId);
            if (!epep.PrivateMotiveFileId.IsEmpty())
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }
            var fileModel = await cdnService.MongoCdn_Download(new CdnFileSelect() { SourceType = mq.SourceType, SourceId = mq.SourceId.ToString() }, CdnFileSelect.PostProcess.Flatten);
            if (fileModel == null)
            {
                mq.ErrorDescription = $"Грешен файл с ID={mq.SourceId}";
                SetErrorToMQ(mq, IntegrationStates.MissingObjectEISS);
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
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
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
                if (fileModel == null)
                {
                    mq.ErrorDescription = $"Грешен файл с ID={mq.SourceId}";
                    SetErrorToMQ(mq, IntegrationStates.MissingObjectEISS);
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
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                return;
            }
            if (epep.SideId == Guid.Empty)
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
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

        public async Task ManageSummons(int fetchCount)
        {
            var dtNow = DateTime.Now;
            //Да не проверява непрекъснато за призовки, а само на всеки три часа
            if (dtNow.Hour % 3 == 0 && dtNow.Minute < 10)
            {
                return;
            }

            //извлича всички изпратени, неизтрити призовки, с начин на доставка през ЕПЕП
            var epepNotifications = repo.All<CaseNotification>()
                                        .Where(x => x.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.ByEPEP)
                                        .Where(x => x.DeliveryDate == null && x.ReturnDate == null)
                                        .Where(x => x.DateSend != null)
                                        .Where(x => x.DateExpired == null)
                                        .OrderBy(x => x.Id)
                                        .Take(fetchCount)
                                        .ToList();

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
                    var deliveryDate = await serviceClient.GetSummonsServedTimestampAsync(epepKey);
                    if (deliveryDate > epepNotification.DateSend)
                    {
                        //успешно призоваване
                        epepNotification.NotificationStateId = NomenclatureConstants.NotificationState.Delivered;
                        epepNotification.DeliveryDate = deliveryDate;
                        epepNotification.ReturnDate = deliveryDate;
                        //                        repo.Update(epepNotification);
                        repo.SaveChanges();

                        //Маркира призовката като прочетена
                        await serviceClient.MarkSummonAsReadAsync(epepKey, DateTime.Now);
                    }
                }
                catch (FaultException fex)
                {
                    var _error = fex.Message;
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

            };

            await CloseChanel();

            return result;
        }
    }
}



