// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Integration.Epep;
using IO.SignTools.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
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
            ICdnService _cdnService,
            IIOSignToolsService _signTools)
        {
            repo = _repo;
            connector = _connector;
            cdnService = _cdnService;
            logger = _logger;
            IntegrationTypeId = NomenclatureConstants.IntegrationTypes.EPEP;
            signTools = _signTools;
            //this.mqID = 349487;
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
            int[] highPriorityAddSourceTypes = { SourceTypeSelectVM.EpepUser };
            int[] highPriorityDeleteSourceTypes = { SourceTypeSelectVM.CaseSessionActDepersonalized, SourceTypeSelectVM.CaseSessionActMotiveDepersonalized };
            IEnumerable<MQEpep> select = repo.All<MQEpep>()
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
            switch (mq.TargetClassName)
            {
                //Регистрация на лица
                case nameof(PersonRegistration):
                    Send_PersonRegistration(mq);
                    break;
                //Връзки лица по дела за лице
                case nameof(PersonAssignment):
                    Send_PersonAssignment(mq);
                    break;

                //Регистрация на адвокат
                case nameof(LawyerRegistration):
                    Send_LawyerRegistration(mq);
                    break;
                //Връзки лица по дела за адвокат
                case nameof(LawyerAssignment):
                    Send_LawyerAssignment(mq);
                    break;

                //Входящи и изходящи доументи и файловете към тях
                case nameof(IncomingDocument):
                    Send_IncomingDocument(mq);
                    break;
                case nameof(IncomingDocumentFile):
                    await Send_IncomingDocumentFile(mq);
                    break;
                case nameof(OutgoingDocument):
                    Send_OutgoingDocument(mq);
                    break;
                case nameof(OutgoingDocumentFile):
                    await Send_OutgoingDocumentFile(mq);
                    break;

                //Протоколи за разпределяне и файловете към тях
                case nameof(Assignment):
                    Send_Assignment(mq);
                    break;
                case nameof(AssignmentFile):
                    await Send_AssignmentFile(mq);
                    break;

                //Дела
                case nameof(Integration.Epep.Case):
                    Send_Case(mq);
                    break;

                //Страни по делото
                case nameof(Integration.Epep.Side):
                    Send_Side(mq);
                    break;

                //Съдия-докладчик
                case nameof(Integration.Epep.Reporter):
                    Send_Reporter(mq);
                    break;

                //Заседания
                case nameof(Hearing):
                    Send_Hearing(mq);
                    break;
                //Състав по Заседания
                case nameof(HearingParticipant):
                    Send_HearingParticipant(mq);
                    break;

                //Призовки
                case nameof(Summon):
                    await Send_Summon(mq);
                    break;

                //Актове
                case nameof(Act):
                    Send_Act(mq);
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
                    Send_Appeal(mq);
                    break;
                default:
                    break;

            }
        }

        private void Send_PersonRegistration(MQEpep mq)
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
                        var existingReg = serviceClient.SelectPersonRegistration(epep.EGN);
                        if (existingReg != null && !existingReg.PersonRegistrationId.IsEmpty())
                        {
                            AddIntegrationKey(mq, existingReg.PersonRegistrationId);
                            return;
                        }
                    }
                    catch (FaultException fex)
                    {

                    }

                    AddIntegrationKey(mq, serviceClient.InsertPersonRegistration(epep));
                    break;
                case EpepConstants.Methods.Update:
                    if (epep.PersonRegistrationId == Guid.Empty)
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, serviceClient.UpdatePersonRegistration(epep));
                    break;
            }

        }

        private void Send_PersonAssignment(MQEpep mq)
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
                    AddIntegrationKey(mq, serviceClient.InsertPersonAssignment(epep));
                    break;
                case EpepConstants.Methods.Update:
                    UpdateMQ(mq, serviceClient.UpdatePersonAssignment(epep));
                    break;
            }

        }

        private void Send_LawyerRegistration(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<LawyerRegistration>(Encoding.UTF8.GetString(mq.Content));
            epep.LawyerRegistrationId = getKeyGuidNullable(SourceTypeSelectVM.EpepUser, mq.SourceId);

            var epepUser = repo.GetById<EpepUser>((int)mq.SourceId);
            var lawyerInfoFromEpep = serviceClient.GetLawyerByNumber(epepUser.LawyerNumber);
            if (lawyerInfoFromEpep != null)
            {
                epep.LawyerId = lawyerInfoFromEpep.LawyerId ?? Guid.Empty;
            }
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
                        var lawyerIdentifiers = serviceClient.GetLawyerRegistrationIdentifiersByLawyerId(epep.LawyerId);
                        if (lawyerIdentifiers.Any())
                        {
                            //Вече има регистрация в ЕПЕП
                            AddIntegrationKey(mq, lawyerIdentifiers.Last());
                            return;
                        }
                    }
                    catch (FaultException fex)
                    {

                    }

                    AddIntegrationKey(mq, serviceClient.InsertLawyerRegistration(epep));
                    break;
                case EpepConstants.Methods.Update:
                    if (epep.LawyerRegistrationId == Guid.Empty)
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, serviceClient.UpdateLawyerRegistration(epep));
                    break;
            }

        }

        private void Send_LawyerAssignment(MQEpep mq)
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
                    AddIntegrationKey(mq, serviceClient.InsertLawyerAssignment(epep));
                    break;
                case EpepConstants.Methods.Update:
                    UpdateMQ(mq, serviceClient.UpdateLawyerAssignment(epep));
                    break;
            }

        }

        private void Send_IncomingDocument(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<IncomingDocument>(Encoding.UTF8.GetString(mq.Content));
            if (string.IsNullOrEmpty(epep.IncomingDocumentTypeCode))
            {
                SetErrorToMQ(mq, IntegrationStates.MissingCodeError);
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

                    AddIntegrationKey(mq, serviceClient.InsertIncomingDocument(epep));
                    break;
                case EpepConstants.Methods.Update:
                    UpdateMQ(mq, serviceClient.UpdateIncomingDocument(epep));
                    break;
            }
        }
        private void Send_OutgoingDocument(MQEpep mq)
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

            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, serviceClient.InsertOutgoingDocument(epep));

                    break;
                case EpepConstants.Methods.Update:
                    UpdateMQ(mq, serviceClient.UpdateOutgoingDocument(epep));
                    break;
            }

        }

        private async Task Send_IncomingDocumentFile(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<IncomingDocumentFile>(Encoding.UTF8.GetString(mq.Content));
            if (epep.IncomingDocumentId == Guid.Empty)
            {

                epep.IncomingDocumentId = getKeyGuid(SourceTypeSelectVM.Document, mq.ParentSourceId);

                if (epep.IncomingDocumentId == Guid.Empty)
                {
                    SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                    return;
                }
            }
            var fileModel = await cdnService.MongoCdn_Download(mq.SourceId);
            if (fileModel == null)
            {
                mq.ErrorDescription = $"Грешен файл с ID={mq.SourceId}";
                SetErrorToMQ(mq, IntegrationStates.DataContentError);
                return;
            }
            epep.IncomingDocumentMimeType = fileModel.ContentType;
            epep.IncomingDocumentContent = fileModel.GetBytes();

            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, serviceClient.InsertIncomingDocumentFile(epep));
                    break;
            }

        }
        private async Task Send_OutgoingDocumentFile(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<OutgoingDocumentFile>(Encoding.UTF8.GetString(mq.Content));
            if (epep.OutgoingDocumentId == Guid.Empty)
            {

                var docGuid = getKey(SourceTypeSelectVM.Document, mq.ParentSourceId);

                if (string.IsNullOrEmpty(docGuid))
                {
                    SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                    return;
                }
                epep.OutgoingDocumentId = Guid.Parse(docGuid);
            }
            var fileModel = await cdnService.MongoCdn_Download(mq.SourceId);
            if (fileModel == null)
            {
                mq.ErrorDescription = $"Грешен файл с ID={mq.SourceId}";
                SetErrorToMQ(mq, IntegrationStates.DataContentError);
                return;
            }
            epep.OutgoingDocumentMimeType = fileModel.ContentType;
            epep.OutgoingDocumentContent = fileModel.GetBytes();
            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, serviceClient.InsertOutgoingDocumentFile(epep));
                    break;
            }

        }

        private void Send_Assignment(MQEpep mq)
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
                    AddIntegrationKey(mq, serviceClient.InsertAssignment(epep));
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

            var fileModel = await cdnService.MongoCdn_Download(new CdnFileSelect() { SourceType = SourceTypeSelectVM.CaseSelectionProtokol, SourceId = mq.SourceId.ToString() });
            if (fileModel != null)
            {
                epep.ProtocolMimeType = fileModel.ContentType;
                epep.ProtocolContent = FlattenSignatures(fileModel.GetBytes());
            }
            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, serviceClient.InsertAssignmentFile(epep));
                    break;
                case EpepConstants.Methods.Update:
                    UpdateMQ(mq, serviceClient.UpdateAssignmentFile(epep));
                    break;
            }

        }

        private void Send_Case(MQEpep mq)
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
                    caseId = serviceClient.InsertCase(epep);
                    AddIntegrationKey(mq, caseId);
                    //try
                    //{
                    //    caseId = serviceClient.InsertCase(epep);
                    //    AddIntegrationKey(mq, caseId);
                    //}
                    //catch (FaultException fex)
                    //{
                    //    var _error = fex.GetMessageFault() ?? "";
                    //    if (_error.ToLower().Contains("неидентифицирана"))
                    //    {
                    //        var doc = repo.AllReadonly<IOWebApplication.Infrastructure.Data.Models.Cases.Case>()
                    //                        .Include(x => x.Document)
                    //                        .Where(x => x.Id == mq.SourceId)
                    //                        .Select(x => x.Document)
                    //                        .FirstOrDefault();

                    //        try
                    //        {
                    //            caseId = serviceClient.GetCaseId(doc.DocumentNumberValue.Value, doc.DocumentDate.Year, epep.CourtCode);
                    //            if (caseId.HasValue)
                    //            {
                    //                AddIntegrationKey(mq, caseId);
                    //            }
                    //        }
                    //        catch { }
                    //    }
                    //}

                    break;
                case EpepConstants.Methods.Update:
                    if (epep.CaseId == Guid.Empty)
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, serviceClient.UpdateCase(epep));
                    break;
                case EpepConstants.Methods.Delete:
                    if (epep.CaseId == Guid.Empty)
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, serviceClient.DeleteCase(epep.CaseId.Value));
                    break;
            }

        }

        private void Send_Side(MQEpep mq)
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
                    AddIntegrationKey(mq, serviceClient.InsertSide(epep));
                    break;
                case EpepConstants.Methods.Update:
                    if (epep.SideId.IsEmpty())
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, serviceClient.UpdateSide(epep));
                    break;
                case EpepConstants.Methods.Delete:
                    if (epep.SideId.IsEmpty())
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, serviceClient.DeleteSide(epep.SideId.Value));
                    break;
            }

        }
        private void Send_Reporter(MQEpep mq)
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
                    AddIntegrationKey(mq, serviceClient.InsertReporter(epep));
                    break;
                case EpepConstants.Methods.Update:
                    if (epep.ReporterId.IsEmpty())
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, serviceClient.UpdateReporter(epep));
                    break;
                case EpepConstants.Methods.Delete:
                    if (epep.ReporterId.IsEmpty())
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, serviceClient.DeleteReporter(epep.ReporterId ?? Guid.Empty));
                    break;
            }

        }

        private void Send_Hearing(MQEpep mq)
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
                    AddIntegrationKey(mq, serviceClient.InsertHearing(epep));
                    if (caseSession.SessionStateId == NomenclatureConstants.SessionState.Provedeno)
                    {
                        send_HearingParticipants(caseSession);
                    }
                    break;
                case EpepConstants.Methods.Update:
                    if (epep.HearingId.IsEmpty())
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    var res = serviceClient.UpdateHearing(epep);
                    UpdateMQ(mq, res);

                    if (caseSession.SessionStateId == NomenclatureConstants.SessionState.Provedeno)
                    {
                        send_HearingParticipants(caseSession);
                    }
                    break;
                case EpepConstants.Methods.Delete:
                    if (epep.HearingId.IsEmpty())
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, serviceClient.DeleteHearing(epep.HearingId.Value));
                    break;
            }

        }
        private void send_HearingParticipants(CaseSession caseSession)
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
                            var returnGuid = serviceClient.InsertHearingParticipant(epep);
                            if (returnGuid.HasValue)
                            {
                                AddIntegrationKey(SourceTypeSelectVM.CaseLawUnit, lawUnit.Id, returnGuid.Value.ToString());
                            }
                        }
                        break;
                    case Methods.Update:
                        {
                            HearingParticipant epep = new HearingParticipant()
                            {
                                HearingId = HearingId,
                                HearingParticipantId = luID,
                                JudgeName = lawUnit.FullName,
                                Role = lawUnit.JudgeRole
                            };
                            serviceClient.UpdateHearingParticipant(epep);
                        }
                        break;
                    case Methods.Delete:
                        {
                            serviceClient.DeleteHearingParticipant(luID.Value);
                        }
                        break;
                }
            }
        }

        private void Send_HearingParticipant(MQEpep mq)
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
                    AddIntegrationKey(mq, serviceClient.InsertHearingParticipant(epep));
                    break;
                case EpepConstants.Methods.Update:
                    if (epep.HearingParticipantId.IsEmpty())
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, serviceClient.UpdateHearingParticipant(epep));
                    break;
                case EpepConstants.Methods.Delete:
                    if (epep.HearingParticipantId.IsEmpty())
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, serviceClient.DeleteHearingParticipant(epep.HearingParticipantId.Value));
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

            SummonFile summonFile = null;

            CdnItemVM aFile = cdnService.Select(SourceTypeSelectVM.CaseNotificationPrint, mq.SourceId.ToString()).Where(x => x.FileName.EndsWith(".pdf")).FirstOrDefault();
            if (aFile != null)
            {
                summonFile = new SummonFile();
                var fileModel = await cdnService.MongoCdn_Download(aFile);
                summonFile.MimeType = fileModel.ContentType;
                summonFile.Content = fileModel.GetBytes();
            }

            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, serviceClient.InsertSummon(epep, epepUser));

                    if (mq.IntegrationStateId == IntegrationStates.TransferOK && summonFile != null)
                    {
                        summonFile.SummonId = Guid.Parse(mq.ReturnGuidId);

                        serviceClient.InsertSummonFile(summonFile);

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
                    UpdateMQ(mq, serviceClient.UpdateSummon(epep));
                    break;
                case EpepConstants.Methods.Delete:
                    if (epep.SummonId.IsEmpty())
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, serviceClient.DeleteSummon(epep.SummonId.Value));
                    break;
            }

        }
        private void Send_Act(MQEpep mq)
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
            epep.HearingId = getKeyGuidNullable(SourceTypeSelectVM.CaseSession, mq.ParentSourceId);
            if (epep.HearingId.IsEmpty())
            {
                SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError, "Изчаква код на заседание");
                return;
            }


            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, serviceClient.InsertAct(epep));
                    send_ActPreparator((int)mq.SourceId);
                    break;
                case EpepConstants.Methods.Update:
                    if (epep.ActId == Guid.Empty)
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, serviceClient.UpdateAct(epep));
                    send_ActPreparator((int)mq.SourceId);
                    break;
                case EpepConstants.Methods.Delete:
                    if (epep.ActId == Guid.Empty)
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, serviceClient.DeleteAct(epep.ActId.Value));
                    break;
            }

        }
        private void send_ActPreparator(int actId)
        {
            var ActId = getKeyGuid(SourceTypeSelectVM.CaseSessionAct, actId);
            if (ActId == Guid.Empty)
            {
                return;
            }
            var act = repo.GetById<CaseSessionAct>(actId);
            var sessionLawUnits = repo.AllReadonly<CaseLawUnit>()
                                    .Include(x => x.JudgeRole)
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
                                        JudgeRole = x.JudgeRole.Label
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
                        var apId = serviceClient.InsertActPreparator(epep);
                        if (!apId.IsEmpty())
                        {
                            AddIntegrationKey(SourceTypeSelectVM.CaseSessionActPreparator, lawUnit.Id, apId.Value.ToString());
                        }
                        break;
                    case Methods.Update:
                        serviceClient.UpdateActPreparator(epep);
                        break;
                }
            }
        }

        private async Task Send_PrivateActFile(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<PrivateActFile>(Encoding.UTF8.GetString(mq.Content));
            if (epep.ActId == Guid.Empty)
            {
                epep.ActId = getKeyGuid(SourceTypeSelectVM.CaseSessionAct, mq.SourceId);
                if (epep.ActId == Guid.Empty)
                {
                    SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                    return;
                }
            }
            epep.PrivateActFileId = getKeyGuidNullable(SourceTypeSelectVM.CaseSessionActPdf, mq.SourceId);
            if (!epep.PrivateActFileId.IsEmpty())
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }

            var fileModel = await cdnService.MongoCdn_Download(new CdnFileSelect() { SourceType = mq.SourceType, SourceId = mq.SourceId.ToString() });
            epep.PrivateActMimeType = fileModel.ContentType;
            epep.PrivateActContent = FlattenSignatures(fileModel.GetBytes());


            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, serviceClient.InsertPrivateActFile(epep));
                    break;
                case EpepConstants.Methods.Update:
                    if (epep.PrivateActFileId == Guid.Empty)
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, serviceClient.UpdatePrivateActFile(epep));
                    break;

            }
        }

        private async Task Send_PublicActFile(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<PublicActFile>(Encoding.UTF8.GetString(mq.Content));
            if (epep.ActId == Guid.Empty)
            {
                epep.ActId = getKeyGuid(SourceTypeSelectVM.CaseSessionAct, mq.SourceId);
                if (epep.ActId == Guid.Empty)
                {
                    SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                    return;
                }
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
                    SetErrorToMQ(mq, IntegrationStates.MissingCodeError);
                    return;
                }
                epep.PublicActMimeType = fileModel.ContentType;
                epep.PublicActContent = fileModel.GetBytes();
            }
            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, serviceClient.InsertPublicActFile(epep));
                    break;
                case EpepConstants.Methods.Update:
                    if (epep.PublicActFileId == Guid.Empty)
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, serviceClient.UpdatePublicActFile(epep));
                    break;
                case EpepConstants.Methods.Delete:
                    RemoveUnfinishedTasksBeforeDelete(mq);
                    if (epep.PublicActFileId == Guid.Empty)
                    {
                        UpdateMQ(mq, true);
                        return;
                    }
                    UpdateMQ(mq, serviceClient.DeletePublicActFile(epep.ActId));
                    RemoveIntegrationKeys(mq);
                    break;
            }

        }



        private async Task Send_PrivateMotiveFile(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<PrivateMotiveFile>(Encoding.UTF8.GetString(mq.Content));
            if (epep.ActId == Guid.Empty)
            {
                epep.ActId = getKeyGuid(SourceTypeSelectVM.CaseSessionAct, mq.SourceId);
                if (epep.ActId == Guid.Empty)
                {
                    SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                    return;
                }
            }
            epep.PrivateMotiveFileId = getKeyGuidNullable(SourceTypeSelectVM.CaseSessionActMotivePdf, mq.SourceId);
            if (!epep.PrivateMotiveFileId.IsEmpty())
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }
            var fileModel = await cdnService.MongoCdn_Download(new CdnFileSelect() { SourceType = mq.SourceType, SourceId = mq.SourceId.ToString() });
            epep.PrivateMotiveMimeType = fileModel.ContentType;
            epep.PrivateMotiveContent = FlattenSignatures(fileModel.GetBytes());
            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, serviceClient.InsertPrivateMotiveFile(epep));
                    break;
                case EpepConstants.Methods.Update:
                    if (epep.PrivateMotiveFileId == Guid.Empty)
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, serviceClient.UpdatePrivateMotiveFile(epep));
                    break;
            }
        }

        private async Task Send_PublicMotiveFile(MQEpep mq)
        {
            var epep = JsonConvert.DeserializeObject<PublicMotiveFile>(Encoding.UTF8.GetString(mq.Content));
            if (epep.ActId == Guid.Empty)
            {
                epep.ActId = getKeyGuid(SourceTypeSelectVM.CaseSessionAct, mq.SourceId);
                if (epep.ActId == Guid.Empty)
                {
                    SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                    return;
                }
            }
            epep.PublicMotiveFileId = getKeyGuidNullable(SourceTypeSelectVM.CaseSessionActMotiveDepersonalized, mq.SourceId);
            if (mq.MethodName == EpepConstants.Methods.Add && !epep.PublicMotiveFileId.IsEmpty())
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }
            if (mq.MethodName != EpepConstants.Methods.Delete)
            {
                var fileModel = await cdnService.MongoCdn_Download(new CdnFileSelect() { SourceType = mq.SourceType, SourceId = mq.SourceId.ToString() });
                epep.PublicMotiveMimeType = fileModel.ContentType;
                epep.PublicMotiveContent = fileModel.GetBytes();
            }

            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, serviceClient.InsertPublicMotiveFile(epep));
                    break;
                case EpepConstants.Methods.Update:
                    if (epep.PublicMotiveFileId == Guid.Empty)
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, serviceClient.UpdatePublicMotiveFile(epep));
                    break;
                case EpepConstants.Methods.Delete:
                    RemoveUnfinishedTasksBeforeDelete(mq);
                    if (epep.PublicMotiveFileId == Guid.Empty)
                    {
                        UpdateMQ(mq, true);
                        return;
                    }

                    UpdateMQ(mq, serviceClient.DeletePublicMotiveFile(epep.ActId));
                    RemoveIntegrationKeys(mq);
                    break;
            }

        }

        private void Send_Appeal(MQEpep mq)
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
                    AddIntegrationKey(mq, serviceClient.InsertAppeal(epep));
                    break;
                case EpepConstants.Methods.Update:
                    if (epep.ActId == Guid.Empty)
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, serviceClient.UpdateAppeal(epep));
                    break;
                case EpepConstants.Methods.Delete:
                    if (epep.AppealId.IsEmpty())
                    {
                        SetErrorToMQ(mq, IntegrationStates.WaitForParentIdError);
                        return;
                    }
                    UpdateMQ(mq, serviceClient.DeleteAppeal(epep.AppealId.Value));
                    break;
            }

        }

        public async Task ManageSummons(int fetchCount)
        {
            var dtNow = DateTime.Now;
            //Да не проверява непрекъснато за призовки, а само на всеки три часа
            if(dtNow.Hour % 3 > 0)
            {
                return;
            }

            //извлича всички изпратени, неизтрити призовки, с начин на доставка през ЕПЕП
            var epepNotifications = repo.All<CaseNotification>()
                                        .Where(x => x.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.ByEPEP)
                                        .Where(x => x.NotificationStateId == NomenclatureConstants.NotificationState.ForDelivery)
                                        .Where(x => x.DateSend != null)
                                        .Where(x => x.DateExpired == null)
                                        .Take(fetchCount)
                                        .ToList();

            foreach (var epepNotification in epepNotifications)
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
                    repo.Update(epepNotification);
                    repo.SaveChanges();

                    //Маркира призовката като прочетена
                    await serviceClient.MarkSummonAsReadAsync(epepKey, DateTime.Now);
                }
            }

        }
    }
}



