// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using DnsClient.Internal;
using Integration.Epep;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.Integrations.EpepRest;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using RestEpep = IOWebApplication.Infrastructure.Models.Integrations.EpepRest;

namespace IOWebApplication.Infrastructure.Services
{
    public class EpepRestClient : IEpepRestClient
    {
        public static string FactoryName = "epepRestHttpClient";

        HttpClient epepClient;
        IHttpClientFactory clientFactory;

        private readonly RestEpep.EpepRestConfigurationVM config;
        private RestEpep.AuthTokenVM epepToken;
        private readonly ILogger<EpepRestClient> logger;
        public EpepRestClient(IOptions<RestEpep.EpepRestConfigurationVM> configOptions,
            IHttpClientFactory _clientFactory,
            ILogger<EpepRestClient> _logger)
        {
            config = configOptions.Value;
            clientFactory = _clientFactory;
            logger = _logger;
        }


        #region Act

        public Task<Guid> InsertAct(RestEpep.Act epepModel)
        {
            return postData<Guid>(epepModel, "act/InsertAct");
        }
        public Task<bool> UpdateAct(RestEpep.Act epepModel)
        {
            return postData<bool>(epepModel, "act/UpdateAct");
        }

        public Task<RestEpep.Act> GetActByIdAsync(Guid gid)
        {
            return postData<RestEpep.Act>(gid, "act/GetActById");
        }

        public Task<bool> DeleteAct(Guid gid)
        {
            return postData<bool>(gid, "act/DeleteAct");
        }

        //------ActPrivateFile

        public Task<Guid> InsertPrivateActFile(PrivateActFile epepModel)
        {
            return postData<Guid>(epepModel, "act/InsertPrivateActFile");
        }
        public Task<Guid> UpdatePrivateActFile(PrivateActFile epepModel)
        {
            return postData<Guid>(epepModel, "act/UpdatePrivateActFile");
        }
        public Task<bool> DeletePrivateActFile(Guid actGid)
        {
            return postData<bool>(actGid, "act/DeletePrivateActFile");
        }

        //------ActPublicFile

        public Task<Guid> InsertPublicActFile(PublicActFile epepModel)
        {
            return postData<Guid>(epepModel, "act/InsertPublicActFile");
        }
        public Task<Guid> UpdatePublicActFile(PublicActFile epepModel)
        {
            return postData<Guid>(epepModel, "act/UpdatePublicActFile");
        }
        public Task<bool> DeletePublicActFile(Guid actGid)
        {
            return postData<bool>(actGid, "act/DeletePublicActFile");
        }

        //------MotivePrivateFile

        public Task<Guid> InsertPrivateMotiveFile(PrivateMotiveFile epepModel)
        {
            return postData<Guid>(epepModel, "act/InsertPrivateMotiveFile");
        }
        public Task<Guid> UpdatePrivateMotiveFile(PrivateMotiveFile epepModel)
        {
            return postData<Guid>(epepModel, "act/UpdatePrivateMotiveFile");
        }
        public Task<bool> DeletePrivateMotiveFile(Guid actGid)
        {
            return postData<bool>(actGid, "act/DeletePrivateMotiveFile");
        }

        //------MotivePublicFile

        public Task<Guid> InsertPublicMotiveFile(PublicMotiveFile epepModel)
        {
            return postData<Guid>(epepModel, "act/InsertPublicMotiveFile");
        }
        public Task<Guid> UpdatePublicMotiveFile(PublicMotiveFile epepModel)
        {
            return postData<Guid>(epepModel, "act/UpdatePublicMotiveFile");
        }
        public Task<bool> DeletePublicMotiveFile(Guid actGid)
        {
            return postData<bool>(actGid, "act/DeletePublicMotiveFile");
        }

        #endregion

        #region AttachedDocument
        public Task<Guid> InsertAttachedDocument(AttachedDocument fileUploadModel)
        {
            return postData<Guid>(fileUploadModel, "common/InsertAttachedDocumentMultiPart", true);
        }

        public Task<Guid> UpdateAttachedDocument(AttachedDocument fileUploadModel)
        {
            return postData<Guid>(fileUploadModel, "common/UpdateAttachedDocument");
        }


        public Task<bool> DeleteAttachedDocument(Guid gid)
        {
            var deleteModel = new FileSelectModel()
            {
                AttachedDocumentId = gid
            };
            return postData<bool>(deleteModel, "common/DeleteAttachedDocument");
        }
        #endregion

        #region Case

        public Task<Guid> InsertCase(Case incomingDocument)
        {
            return postData<Guid>(incomingDocument, "case/InsertCase");
        }
        public Task<bool> UpdateCase(Case incomingDocument)
        {
            return postData<bool>(incomingDocument, "case/UpdateCase");
        }

        public Task<Case> GetCaseById(Guid gid)
        {
            return postData<Case>(gid, "case/GetCaseById");
        }

        public Task<bool> DeleteCase(Guid gid)
        {
            return postData<bool>(gid, "case/DeleteCase");
        }

        public Task<SummaryCase> GetSummaryCase(Guid gid)
        {
            return postData<SummaryCase>(gid, "case/GetSummaryCase");
        }

        #endregion

        #region CaseMigration

        public Task<Guid> InsertCaseMigration(CaseMigrationRegistration model)
        {
            return postData<Guid>(model, "case/InsertCaseMigration");
        }

        public Task<Guid[]> GetFinishedCaseMigrations(CaseMigrationCourtFilter model)
        {
            return postData<Guid[]>(model, "case/GetFinishedCaseMigrations");
        }

        public Task<CaseMigrationResult> GetResultCaseMigration(Guid caseMigrationId)
        {
            return postData<CaseMigrationResult>(caseMigrationId, "case/GetResultCaseMigration");
        }

        public Task<bool> EndProcessCaseMigration(Guid caseMigrationId)
        {
            return postData<bool>(caseMigrationId, "case/EndProcessCaseMigration");
        }


        #endregion

        #region ConnectedCase

        public Task<Guid> InsertConnectedCase(ConnectedCase model)
        {
            return postData<Guid>(model, "case/InsertConnectedCase");
        }
        public Task<bool> UpdateConnectedCase(ConnectedCase model)
        {
            return postData<bool>(model, "case/UpdateConnectedCase");
        }

        public Task<ConnectedCase> GetConnectedCaseByCaseId(Guid caseId)
        {
            return postData<ConnectedCase>(caseId, "case/GetConnectedCaseByCaseId");
        }

        public Task<bool> DeleteConnectedCase(Guid caseId)
        {
            return postData<bool>(caseId, "case/DeleteConnectedCase");
        }

        #endregion

        #region ElectronicDocument

        public Task<List<Guid>> GetNewElectronicDocumentIdentifiers(string courtCode)
        {
            return postData<List<Guid>>(courtCode, "document/GetNewElectronicDocumentIdentifiers");
        }

        public Task<RestEpep.ElectronicDocument> GetElectronicDocument(Guid gid)
        {
            return postData<RestEpep.ElectronicDocument>(gid, "document/GetElectronicDocument");
        }

        public Task<List<RestEpep.ElectronicDocumentPayment>> GetElectronicDocumentPayments(string courtCode)
        {
            return postData<List<RestEpep.ElectronicDocumentPayment>>(courtCode, "document/GetElectronicDocumentPayments");
        }

        public Task<bool> UpdateElectronicDocumentSetDateCourtAccept(Guid electronicDocumentId, DateTime dateAccept)
        {
            return postData<bool>(new RequestUpdateElectronicDocumentAccept()
            {
                Gid = electronicDocumentId,
                DateAccept = dateAccept
            }, "document/UpdateElectronicDocumentSetDateCourtAccept");
        }

        public Task<bool> UpdateElectronicDocumentSetMoneyAccept(Guid electronicDocumentId, DateTime dateAccept)
        {
            return postData<bool>(new RequestUpdateElectronicDocumentAccept()
            {
                Gid = electronicDocumentId,
                DateAccept = dateAccept
            }, "document/UpdateElectronicDocumentSetMoneyAccept");
        }

        #endregion

        #region ExecProcess
        public Task<Guid> InsertExecProcess(ExecProcess execProcess)
        {
            return postData<Guid>(execProcess, "execprocess/InsertExecProcess");
        }
        public Task<ExecProcessDetailsVM> GetExecProcessById(Guid gid)
        {
            return postData<ExecProcessDetailsVM>(gid, "execprocess/GetExecProcessById");
        }
        public Task<Guid> InsertAccessKey(ExecProcessAccessChange model)
        {
            return postData<Guid>(model, "execprocess/InsertAccessKey");
        }
        public Task<bool> ExpireAccessKey(ExecProcessAccessChange model)
        {
            return postData<bool>(model, "execprocess/ExpireAccessKey");
        }
        public Task<ExecProcessDeliveredListVM[]> SelectNewExecListDeliveryDates()
        {
            return postData<ExecProcessDeliveredListVM[]>(string.Empty, "execprocess/SelectNewExecListDeliveryDates");
        }
        public Task<bool> SetCourtAcceptListDeliveryDate(Guid gid)
        {
            return postData<bool>(gid, "execprocess/SetCourtAcceptListDeliveryDate");
        }
        #endregion

        #region IncomingDocument

        public Task<Guid> InsertIncomingDocument(RestEpep.IncomingDocument incomingDocument)
        {
            return postData<Guid>(incomingDocument, "document/InsertIncomingDocument");
        }
        public Task<bool> UpdateIncomingDocument(RestEpep.IncomingDocument incomingDocument)
        {
            return postData<bool>(incomingDocument, "document/UpdateIncomingDocument");
        }

        public Task<RestEpep.IncomingDocument> GetIncomingDocumentById(Guid gid)
        {
            return postData<RestEpep.IncomingDocument>(gid, "document/GetIncomingDocumentById");
        }

        public Task<bool> DeleteIncomingDocument(Guid gid)
        {
            return postData<bool>(gid, "document/DeleteIncomingDocument");
        }

        #endregion

        #region OutgoingDocument

        public Task<Guid> InsertOutgoingDocument(OutgoingDocument incomingDocument)
        {
            return postData<Guid>(incomingDocument, "document/InsertOutgoingDocument");
        }
        public Task<bool> UpdateOutgoingDocument(OutgoingDocument incomingDocument)
        {
            return postData<bool>(incomingDocument, "document/UpdateOutgoingDocument");
        }

        public Task<OutgoingDocument> GetOutgoingDocumentById(Guid gid)
        {
            return postData<OutgoingDocument>(gid, "document/GetOutgoingDocumentById");
        }

        public Task<bool> DeleteOutgoingDocument(Guid gid)
        {
            return postData<bool>(gid, "document/DeleteOutgoingDocument");
        }

        #endregion

        #region Summon
        public Task<Guid> InsertSummon(SummonSaveModel epepModel)
        {
            return postData<Guid>(epepModel, $"summon/InsertSummon");
        }
        public Task<bool> UpdateSummon(SummonSaveModel epepModel)
        {
            return postData<bool>(epepModel, $"summon/UpdateSummon");
        }
        public Task<RestEpep.Summon> GetSummonById(Guid summonGid)
        {
            return postData<RestEpep.Summon>(summonGid, $"summon/GetSummonById");
        }
        public Task<bool> DeleteSummon(Guid summonGid)
        {
            return postData<bool>(summonGid, $"summon/DeleteSummon");
        }
        public Task<Guid> InsertSummonFile(SummonFile epepModel)
        {
            return postData<Guid>(epepModel, $"summon/InsertSummonFile");
        }
        public Task<Guid> UpdateSummonFile(SummonFile epepModel)
        {
            return postData<Guid>(epepModel, $"summon/UpdateSummonFile");
        }
        public Task<bool> DeleteSummonFile(Guid summonGid)
        {
            return postData<bool>(summonGid, $"summon/DeleteSummonFile");
        }
        public Task<Guid> InsertSummonReport(SummonFile epepModel)
        {
            return postData<Guid>(epepModel, $"summon/InsertSummonReport");
        }
        public Task<Guid> UpdateSummonReport(SummonFile epepModel)
        {
            return postData<Guid>(epepModel, $"summon/UpdateSummonReport");
        }
        public Task<bool> DeleteSummonReport(Guid summonGid)
        {
            return postData<bool>(summonGid, $"summon/DeleteSummonReport");
        }
        public Task<SummonReadModel[]> GetCourtUnreadSummons()
        {
            return postData<SummonReadModel[]>(null, $"summon/GetCourtUnreadSummons");
        }
        public Task<RestEpep.SummonReadTimeResult> GetSummonsReadTimestamp(Guid summonGid)
        {
            return postData<RestEpep.SummonReadTimeResult>(summonGid, $"summon/GetSummonsReadTimestamp");
        }
        public Task<bool> MarkSummonAsCourtRead(SummonCourtRead epepModel)
        {
            return postData<bool>(epepModel, $"summon/MarkSummonAsCourtRead");
        }
        public Task<byte[]> GetSummonReportDocument(Guid summonGid)
        {
            try
            {
                return postData<byte[]>(summonGid, $"summon/GetSummonReportDocument");
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region User

        public Task<UserRegistration[]> GetUserRegistrations(DateTime? modifyFromDate)
        {
            return postData<UserRegistration[]>(modifyFromDate, "user/GetUserRegistrations");
        }

        public Task<UserDeactivatedRegistration[]> GetUserDeactivatedRegistrations(DateTime? modifyFromDate)
        {
            return postData<UserDeactivatedRegistration[]>(modifyFromDate, "user/GetUserDeactivatedRegistrations");
        }

        #endregion

        #region Common

        public void InitClient()
        {
            if (epepClient == null)
            {
                epepClient = clientFactory.CreateClient(FactoryName);
            }
        }

        public async Task<byte[]> DownloadDocumentBinary(Guid gid)
        {
            var fileModel = await postData<FileBlobVM>(gid, "common/DownloadDocumentBinary");
            if (fileModel == null)
            {
                return null;
            }
            /*
                Върнатия обект от Epep.Api е FileBlobVM fileModel
                Полето BinaryContent съдържа byte[] epepFileContent
                Препоръчваме да се използва този метод
            */
            return fileModel.BinaryContent;
        }

        public async Task<byte[]> DownloadFileContent(Guid gid)
        {
            return await postDataBinaryResult(gid, "common/DownloadFileContent");
        }

        #endregion

        #region Nomenclatures

        public Task<bool> UpdateStreets(EkStreet[] updatedStreets)
        {
            return postData<bool>(updatedStreets, "Nomenclature/UpdateStreets");
        }
        #endregion 

        //===================================================================================================================

        #region Auth and send data

        private string getUrl(string path)
        {
            return new Uri(new Uri(config.RestAPI), path).ToString();
        }

        private async Task<TResult> postData<TResult>(object data, string path, bool multipartForm = false)
        {
            if (epepToken == null || epepToken.ExpiresIn < DateTime.Now.AddMinutes(1))
            {
                try
                {
                    await getToken();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "EpepRestClient.getToken");
                }
            }
            if (epepToken == null)
            {
                throw new Exception($"getToken error: Token is null");
            }
            if (!epepToken.Result)
            {
                throw new Exception($"getToken error: Result - False!!; {epepToken.Code} - {epepToken.Message}");
            }
            if (epepToken.ExpiresIn < DateTime.Now)
            {
                throw new Exception($"getToken error: Token is Expired;{epepToken.ExpiresIn}");
            }

            string tokenAuthHeader = $"Bearer {epepToken.Token}";

            epepClient.DefaultRequestHeaders.Clear();
            epepClient.DefaultRequestHeaders.Add("Authorization", tokenAuthHeader);
            epepClient.DefaultRequestHeaders
                           .Accept
                           .Add(new MediaTypeWithQualityHeaderValue("application/json"));


            HttpContent sendContent = null;
            if (multipartForm)
            {
                var form = new MultipartFormDataContent();
                var fileContent = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data))));
                form.Add(fileContent, "formContent", "formContent");
                epepClient.DefaultRequestHeaders
                         .Accept
                         .Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));
                sendContent = form;
            }
            else
            {
                sendContent = EpepAuthExtensions.GetJsonContent(data);
            }


            try
            {
                Stopwatch st = new Stopwatch();
                st.Start();
                var response = await epepClient.PostAsync(getUrl(path), sendContent);
                st.Stop();
                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.OK:
                        {
                            var stringContent = await response.Content.ReadAsStringAsync();
                            return JsonConvert.DeserializeObject<TResult>(stringContent);
                        }
                    case System.Net.HttpStatusCode.BadRequest:
                        {
                            var stringContent = await response.Content.ReadAsStringAsync();
                            var error = JsonConvert.DeserializeObject<RestEpep.EpepErrorVM>(stringContent);
                            if (error != null)
                            {
                                throw new Exception($"{error.Code}; {error.Message}.{error.Details}");
                            }
                            else
                            {
                                throw new Exception(stringContent);
                            }
                        }
                    case System.Net.HttpStatusCode.Unauthorized:
                        throw new Exception($"Unauthorized");
                    default:
                        throw new Exception($"HttpError, statusCode: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task<byte[]> postDataBinaryResult(object data, string path)
        {
            if (epepToken == null || epepToken.ExpiresIn < DateTime.Now.AddMinutes(1))
            {
                try
                {
                    await getToken();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "EpepRestClient.getToken");
                }
            }
            if (epepToken == null)
            {
                throw new Exception($"getToken error: Token is null");
            }
            if (!epepToken.Result)
            {
                throw new Exception($"getToken error: Result - False!!; {epepToken.Code} - {epepToken.Message}");
            }
            if (epepToken.ExpiresIn < DateTime.Now)
            {
                throw new Exception($"getToken error: Token is Expired;{epepToken.ExpiresIn}");
            }

            string tokenAuthHeader = $"Bearer {epepToken.Token}";

            epepClient.DefaultRequestHeaders.Clear();
            epepClient.DefaultRequestHeaders.Add("Authorization", tokenAuthHeader);
            epepClient.DefaultRequestHeaders
                           .Accept
                           .Add(new MediaTypeWithQualityHeaderValue("application/json"));


            HttpContent sendContent = EpepAuthExtensions.GetJsonContent(data);


            try
            {
                Stopwatch st = new Stopwatch();
                st.Start();
                var response = await epepClient.PostAsync(getUrl(path), sendContent);
                st.Stop();
                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.OK:
                        {

                            byte[] byteResponse = await response.Content.ReadAsByteArrayAsync();
                            return byteResponse;

                        }
                    case System.Net.HttpStatusCode.BadRequest:
                        {
                            var stringContent = await response.Content.ReadAsStringAsync();
                            var error = JsonConvert.DeserializeObject<RestEpep.EpepErrorVM>(stringContent);
                            if (error != null)
                            {
                                throw new Exception($"{error.Code}; {error.Message}.{error.Details}");
                            }
                            else
                            {
                                throw new Exception(stringContent);
                            }
                        }
                    case System.Net.HttpStatusCode.Unauthorized:
                        throw new Exception($"Unauthorized");
                    default:
                        throw new Exception($"HttpError, statusCode: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task getToken()
        {
            if (epepToken != null)
            {
                if (epepToken.Result && (epepToken.ExpiresIn > DateTime.Now.AddMinutes(5)))
                {
                    return;
                }
            }

            string tokenAuthHeader = $"token {config.AppKey}";

            RestEpep.AuthTokenRequestVM tokenRequest = new RestEpep.AuthTokenRequestVM()
            {
                Data = DateTime.Now.ToString("yyyMMddHHmm")
            };
            tokenRequest.Hash = EpepAuthExtensions.ComputeHashFromData(tokenRequest.Data, config.AppSecret);
            HttpContent content = EpepAuthExtensions.GetJsonContent(tokenRequest);
            epepClient.DefaultRequestHeaders.Clear();
            epepClient.DefaultRequestHeaders.Add("Authorization", tokenAuthHeader);
            epepClient.DefaultRequestHeaders
                              .Accept
                              .Add(new MediaTypeWithQualityHeaderValue("application/json"));


            var response = await epepClient.PostAsync(getUrl("auth/gettoken"), content);
            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                case System.Net.HttpStatusCode.BadRequest:
                    var stringContent = await response.Content.ReadAsStringAsync();
                    epepToken = JsonConvert.DeserializeObject<RestEpep.AuthTokenVM>(stringContent);
                    break;
                case System.Net.HttpStatusCode.Unauthorized:
                    epepToken = new RestEpep.AuthTokenVM()
                    {
                        Result = false,
                        Code = "403",
                        Message = "Unauthorized"
                    };
                    break;
            }

        }
        #endregion
    }
}
