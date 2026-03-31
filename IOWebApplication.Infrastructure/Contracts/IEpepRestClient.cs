// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Integration.Epep;
using IOWebApplication.Infrastructure.Models.Integrations.EpepRest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestEpep = IOWebApplication.Infrastructure.Models.Integrations.EpepRest;

namespace IOWebApplication.Infrastructure.Contracts
{
    public interface IEpepRestClient
    {
        Task<bool> DeleteAct(Guid gid);
        Task<bool> DeleteAttachedDocument(Guid gid);
        Task<bool> DeleteIncomingDocument(Guid gid);
        Task<bool> ExpireAccessKey(ExecProcessAccessChange model);
        Task<bool> SetCourtAcceptListDeliveryDate(Guid gid);
        Task<bool> UpdateAct(RestEpep.Act epepModel);
        Task<bool> UpdateElectronicDocumentSetDateCourtAccept(Guid electronicDocumentId, DateTime dateAccept);
        Task<bool> UpdateElectronicDocumentSetMoneyAccept(Guid electronicDocumentId, DateTime dateAccept);
        Task<bool> UpdateIncomingDocument(RestEpep.IncomingDocument incomingDocument);
        Task<byte[]> DownloadDocumentBinary(Guid gid);
        Task<ExecProcessDeliveredListVM[]> SelectNewExecListDeliveryDates();
        Task<bool> DeletePrivateActFile(Guid actGid);
        Task<bool> DeletePrivateMotiveFile(Guid actGid);
        Task<bool> DeletePublicActFile(Guid actGid);
        Task<bool> DeletePublicMotiveFile(Guid actGid);
        Task<Guid> InsertAccessKey(ExecProcessAccessChange model);
        Task<Guid> InsertAct(RestEpep.Act epepModel);
        Task<Guid> InsertAttachedDocument(AttachedDocument fileUploadModel);
        Task<Guid> InsertExecProcess(RestEpep.ExecProcess execProcess);
        Task<Guid> InsertIncomingDocument(RestEpep.IncomingDocument incomingDocument);
        Task<Guid> InsertPrivateActFile(PrivateActFile epepModel);
        Task<Guid> InsertPrivateMotiveFile(PrivateMotiveFile epepModel);
        Task<Guid> InsertPublicActFile(PublicActFile epepModel);
        Task<Guid> InsertPublicMotiveFile(PublicMotiveFile epepModel);
        Task<Guid> InsertSummonFile(SummonFile epepModel);
        Task<Guid> InsertSummonReport(SummonFile epepModel);
        Task<Guid> UpdateAttachedDocument(AttachedDocument fileUploadModel);
        Task<Guid> UpdatePrivateActFile(PrivateActFile epepModel);
        Task<Guid> UpdatePrivateMotiveFile(PrivateMotiveFile epepModel);
        Task<Guid> UpdatePublicActFile(PublicActFile epepModel);
        Task<Guid> UpdatePublicMotiveFile(PublicMotiveFile epepModel);
        Task<Guid> UpdateSummonFile(SummonFile epepModel);
        Task<Guid> UpdateSummonReport(SummonFile epepModel);
        Task<RestEpep.IncomingDocument> GetIncomingDocumentById(Guid gid);
        Task<List<Guid>> GetNewElectronicDocumentIdentifiers(string courtCode);
        Task<List<RestEpep.ElectronicDocumentPayment>> GetElectronicDocumentPayments(string courtCode);
        Task<RestEpep.Act> GetActByIdAsync(Guid gid);
        Task<RestEpep.ElectronicDocument> GetElectronicDocument(Guid gid);
        Task<RestEpep.ExecProcessDetailsVM> GetExecProcessById(Guid gid);
        void InitClient();
        Task<Guid> InsertSummon(SummonSaveModel epepModel);
        Task<bool> UpdateSummon(SummonSaveModel epepModel);
        Task<RestEpep.Summon> GetSummonById(Guid summonGid);
        Task<bool> DeleteSummonReport(Guid summonGid);
        Task<bool> DeleteSummon(Guid summonGid);
        Task<bool> DeleteSummonFile(Guid summonGid);
        Task<UserRegistration[]> GetUserRegistrations(DateTime? modifyFromDate);
        Task<byte[]> DownloadFileContent(Guid gid);
        Task<UserDeactivatedRegistration[]> GetUserDeactivatedRegistrations(DateTime? modifyFromDate);
        Task<bool> UpdateStreets(EkStreet[] updatedStreets);
        Task<Guid> InsertOutgoingDocument(OutgoingDocument incomingDocument);
        Task<bool> UpdateOutgoingDocument(OutgoingDocument incomingDocument);
        Task<OutgoingDocument> GetOutgoingDocumentById(Guid gid);
        Task<bool> DeleteOutgoingDocument(Guid gid);
        Task<Guid> InsertCase(Case incomingDocument);
        Task<bool> UpdateCase(Case incomingDocument);
        Task<Case> GetCaseById(Guid gid);
        Task<bool> DeleteCase(Guid gid);
        Task<Guid> InsertCaseMigration(CaseMigrationRegistration model);
        Task<Guid[]> GetFinishedCaseMigrations(CaseMigrationCourtFilter model);
        Task<CaseMigrationResult> GetResultCaseMigration(Guid caseMigrationId);
        Task<bool> EndProcessCaseMigration(Guid caseMigrationId);
        Task<Guid> InsertConnectedCase(ConnectedCase model);
        Task<bool> UpdateConnectedCase(ConnectedCase model);
        Task<ConnectedCase> GetConnectedCaseByCaseId(Guid caseId);
        Task<bool> DeleteConnectedCase(Guid caseId);
        Task<SummonReadModel[]> GetCourtUnreadSummons();
        Task<RestEpep.SummonReadTimeResult> GetSummonsReadTimestamp(Guid summonGid);
        Task<bool> MarkSummonAsCourtRead(SummonCourtRead epepModel);
        Task<byte[]> GetSummonReportDocument(Guid summonGid);
        Task<SummaryCase> GetSummaryCase(Guid gid);
    }
}
