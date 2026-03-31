// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using DnsClient.Internal;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Models.Integrations.RNFL
{
    public class RnflRestClient : IRnflRestClient
    {
        public static string FactoryName = "rnflRestHttpClient";

        HttpClient rnflClient;
        IHttpClientFactory clientFactory;

        private readonly RnflRestConfigurationVM config;
        private AuthTokenVM rnflToken;
        private readonly ILogger<RnflRestClient> logger;
        public RnflRestClient(IOptions<RnflRestConfigurationVM> configOptions,
            IHttpClientFactory _clientFactory,
            ILogger<RnflRestClient> _logger)
        {
            config = configOptions.Value;
            clientFactory = _clientFactory;
            logger = _logger;
        }
        public async Task<SaveResultVM> HealthTest()
        {
            try
            {
                await getToken();
                if (rnflToken == null)
                {
                    return new SaveResultVM(false, "Token is null!!");
                }
                if (rnflToken.Result)
                {
                    rnflToken.Message = $"AuthToken expires in {rnflToken.ExpiresIn}";
                }
                return new SaveResultVM(rnflToken.Result, rnflToken.Message);
            }
            catch (Exception e)
            {
                return new SaveResultVM(false, e.Message);
            }
        }



        #region Insolvency

        public Task<Guid> InsertCase(ApiCase rnflModel)
        {
            return postData<Guid>(rnflModel, "Insolvency/InsertCase");
        }

        public Task<bool> UpdateCase(ApiCase rnflModel)
        {
            return postData<bool>(rnflModel, "Insolvency/UpdateCase");
        }

        public Task<Guid> InsertDebtor(ApiDebtor rnflModel)
        {
            return postData<Guid>(rnflModel, "Insolvency/InsertDebtor");
        }

        public Task<bool> UpdateDebtor(ApiDebtor rnflModel)
        {
            return postData<bool>(rnflModel, "Insolvency/UpdateDebtor");
        }

        public Task<Guid> InsertSyndic(ApiSyndic rnflModel)
        {
            return postData<Guid>(rnflModel, "Insolvency/InsertSyndic");
        }

        public Task<bool> UpdateSyndic(ApiSyndic rnflModel)
        {
            return postData<bool>(rnflModel, "Insolvency/UpdateSyndic");
        }

        public Task<Guid> InsertAct(ApiAct rnflModel)
        {
            return postData<Guid>(rnflModel, "Insolvency/InsertAct");
        }

        public Task<bool> UpdateAct(ApiAct rnflModel)
        {
            return postData<bool>(rnflModel, "Insolvency/UpdateAct");
        }

        public Task<Guid> InsertDocument(ApiDocument rnflModel)
        {
            return postData<Guid>(rnflModel, "Insolvency/InsertDocument");
        }

        public Task<bool> UpdateDocument(ApiDocument rnflModel)
        {
            return postData<bool>(rnflModel, "Insolvency/UpdateDocument");
        }

        public Task<Guid> InsertSummon(ApiSummon rnflModel)
        {
            return postData<Guid>(rnflModel, "Insolvency/InsertSummon");
        }

        public Task<bool> UpdateSummon(ApiSummon rnflModel)
        {
            return postData<bool>(rnflModel, "Insolvency/UpdateSummon");
        }

        public Task<Guid> InsertAppeal(ApiAppeal rnflModel)
        {
            return postData<Guid>(rnflModel, "Insolvency/InsertAppeal");
        }

        public Task<bool> UpdateAppeal(ApiAppeal rnflModel)
        {
            return postData<bool>(rnflModel, "Insolvency/UpdateAppeal");
        }


        public Task<Guid> InsertFile(ApiFile fileUploadModel)
        {
            return postData<Guid>(fileUploadModel, "Insolvency/InsertFile", true);
        }

        public Task<ApiFile> DownloadFile(Guid gid)
        {
            return postData<ApiFile>(gid, "Insolvency/DownloadFile");
        }
        public Task<bool> DeleteFile(Guid gid)
        {
            return postData<bool>(gid, "Insolvency/DeleteFile");
        }

        #endregion

        #region Common

        public void InitClient()
        {
            if (rnflClient == null)
            {
                rnflClient = clientFactory.CreateClient(FactoryName);
                rnflClient.BaseAddress = new Uri(config.RestAPI);
            }
        }


        public async Task<byte[]> DownloadFileContent(Guid gid)
        {
            return await postDataBinaryResult(gid, "common/DownloadFileContent");
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
            if (rnflToken == null || rnflToken.ExpiresIn < DateTime.Now.AddMinutes(1))
            {
                try
                {
                    await getToken();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "RnflRestClient.getToken");
                }
            }
            if (rnflToken == null)
            {
                throw new Exception($"RnflRestClient.getToken error: Token is null");
            }
            if (!rnflToken.Result)
            {
                throw new Exception($"RnflRestClient.getToken error: Result - False!!; {rnflToken.Code} - {rnflToken.Message}");
            }
            if (rnflToken.ExpiresIn < DateTime.Now)
            {
                throw new Exception($"RnflRestClient.getToken error: Token is Expired;{rnflToken.ExpiresIn}");
            }

            string tokenAuthHeader = $"Bearer {rnflToken.Token}";

            rnflClient.DefaultRequestHeaders.Clear();
            rnflClient.DefaultRequestHeaders.Add("Authorization", tokenAuthHeader);
            rnflClient.DefaultRequestHeaders
                           .Accept
                           .Add(new MediaTypeWithQualityHeaderValue("application/json"));


            HttpContent sendContent = null;
            if (multipartForm)
            {
                var form = new MultipartFormDataContent();
                var fileContent = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data))));
                form.Add(fileContent, "formContent", "formContent");
                rnflClient.DefaultRequestHeaders
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
                string postUrl = getUrl(path);
                var response = await rnflClient.PostAsync(path, sendContent);
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
                            var error = JsonConvert.DeserializeObject<ServiceErrorVM>(stringContent);
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
                        throw new Exception($"HttpError: Url:{postUrl} statusCode: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task<byte[]> postDataBinaryResult(object data, string path)
        {
            if (rnflToken == null || rnflToken.ExpiresIn < DateTime.Now.AddMinutes(1))
            {
                try
                {
                    await getToken();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "RnflRestClient.getToken");
                }
            }
            if (rnflToken == null)
            {
                throw new Exception($"getToken error: Token is null");
            }
            if (!rnflToken.Result)
            {
                throw new Exception($"getToken error: Result - False!!; {rnflToken.Code} - {rnflToken.Message}");
            }
            if (rnflToken.ExpiresIn < DateTime.Now)
            {
                throw new Exception($"getToken error: Token is Expired;{rnflToken.ExpiresIn}");
            }

            string tokenAuthHeader = $"Bearer {rnflToken.Token}";

            rnflClient.DefaultRequestHeaders.Clear();
            rnflClient.DefaultRequestHeaders.Add("Authorization", tokenAuthHeader);
            rnflClient.DefaultRequestHeaders
                           .Accept
                           .Add(new MediaTypeWithQualityHeaderValue("application/json"));


            HttpContent sendContent = EpepAuthExtensions.GetJsonContent(data);


            try
            {
                Stopwatch st = new Stopwatch();
                st.Start();
                var response = await rnflClient.PostAsync(path, sendContent);
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
                            var error = JsonConvert.DeserializeObject<ServiceErrorVM>(stringContent);
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
            if (rnflToken != null)
            {
                if (rnflToken.Result && (rnflToken.ExpiresIn > DateTime.Now.AddMinutes(5)))
                {
                    return;
                }
            }

            string tokenAuthHeader = $"token {config.AppKey}";

            AuthTokenRequestVM tokenRequest = new AuthTokenRequestVM()
            {
                Data = DateTime.Now.ToString("yyyMMddHHmm")
            };
            tokenRequest.Hash = EpepAuthExtensions.ComputeHashFromData(tokenRequest.Data, config.AppSecret);
            HttpContent content = EpepAuthExtensions.GetJsonContent(tokenRequest);
            rnflClient.DefaultRequestHeaders.Clear();
            rnflClient.DefaultRequestHeaders.Add("Authorization", tokenAuthHeader);
            rnflClient.DefaultRequestHeaders
                              .Accept
                              .Add(new MediaTypeWithQualityHeaderValue("application/json"));


            var response = await rnflClient.PostAsync("auth/gettoken", content);
            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                case System.Net.HttpStatusCode.BadRequest:
                    {
                        var stringContent = await response.Content.ReadAsStringAsync();
                        rnflToken = JsonConvert.DeserializeObject<AuthTokenVM>(stringContent);
                    }
                    break;
                case System.Net.HttpStatusCode.Unauthorized:
                    rnflToken = new AuthTokenVM()
                    {
                        Result = false,
                        Code = "403",
                        Message = "Unauthorized"
                    };
                    break;
                case System.Net.HttpStatusCode.InternalServerError:
                    {
                        var stringContent = await response.Content.ReadAsStringAsync();
                        logger.LogError($"GetToken ISE Error : {stringContent}");
                        rnflToken = null;
                    }
                    break;
                default:
                    {
                        var stringContent = await response.Content.ReadAsStringAsync();
                        logger.LogError($"Code: {response.StatusCode}; GetToken Error : {stringContent}; RequestUrl:{response.RequestMessage.RequestUri}");
                        rnflToken = null;
                    }
                    break;
            }

        }
        #endregion
    }
}
