// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Integration.Epep;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Proxy.V2T.Core.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using RestEpep = IOWebApplication.Infrastructure.Models.Integrations.EpepRest;

namespace IOWebApplication.Core.Services
{
    public class ProxyEissService : BaseService, IProxyEissService
    {
        string userUIC = "";
        string BASE_PROXY_URL;
        public ProxyEissService(
            IUserContext _userContext,
            IRepository _repo,
            ILogger<ProxyEissService> _logger)
        {
            userContext = _userContext;
            repo = _repo;
            logger = _logger;
        }

        #region Voice To Text

        private void initProxy()
        {
            if (!string.IsNullOrEmpty(BASE_PROXY_URL))
            {
                return;
            }
            BASE_PROXY_URL = this.SystemParam_SelectValue(NomenclatureConstants.SystemParamName.URL_PROXY_EISS);
        }

        private async Task<bool> initV2T()
        {
            if (!string.IsNullOrEmpty(userUIC))
            {
                return true;
            }
            initProxy();
            try
            {
                userUIC = await repo.GetPropByIdAsync<LawUnit, string>(x => x.Id == userContext.LawUnitId, x => x.Uic);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"ProxyEissService.initV2T; UserLU:{userContext.LawUnitId}");
            }
            return false;
        }

        public async Task<V2TResponse> V2TIsAuthenticated()
        {

            bool initOk = await initV2T();
            if (!initOk)
            {
                return new V2TResponse();
            }

            if (string.IsNullOrEmpty(BASE_PROXY_URL))
            {
                return new V2TResponse();
            }

            var res = await requestV2Tdata("v2t/isauthorized", new V2TRequest() { UserUIC = userUIC }, true).ConfigureAwait(false);
            return res;
        }

        public async Task<V2TFileList[]> V2TList(string term)
        {

            bool initOk = await initV2T();
            if (!initOk)
            {
                return null;
            }

            if (string.IsNullOrEmpty(BASE_PROXY_URL))
            {
                return null;
            }
            var res = await requestV2Tdata("v2t/list", new V2TRequest() { Term = term, UserUIC = userUIC }).ConfigureAwait(false);
            try
            {
                if (!string.IsNullOrEmpty(res.Response) && res.IsAuthorized)
                {
                    var result = JsonConvert.DeserializeObject<V2TFileList[]>(res.Response);
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<string> V2TContent(string fileId)
        {
            bool initOk = await initV2T();
            if (!initOk)
            {
                return null;
            }

            if (string.IsNullOrEmpty(BASE_PROXY_URL))
            {
                return null;
            }
            var res = await requestV2Tdata("v2t/filecontent", new V2TRequest() { Id = fileId, UserUIC = userUIC }).ConfigureAwait(false);
            try
            {
                if (!string.IsNullOrEmpty(res.Response))
                {
                    return res.Response;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private async Task<V2TResponse> requestV2Tdata(string method, V2TRequest request, bool deserializeResponse = false)
        {
            bool initOk = await initV2T();
            if (!initOk)
            {
                return new V2TResponse()
                {
                    GeneralError = true
                };
            }

            try
            {
                HttpResponseMessage resList;
                using (var client = new HttpClient())
                {
                    using (HttpContent content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"))
                    {
                        var url = new Uri(new Uri(BASE_PROXY_URL), method);

                        resList = await client.PostAsync(url, content).ConfigureAwait(false);
                    }
                    client.Dispose();
                }
                switch (resList.StatusCode)
                {
                    case System.Net.HttpStatusCode.Forbidden:
                        return new V2TResponse()
                        {
                            IsAuthorized = false
                        };
                    case System.Net.HttpStatusCode.OK:

                        var contentResponse = await resList.Content.ReadAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrEmpty(contentResponse))
                        {
                            if (deserializeResponse)
                            {
                                try
                                {
                                    return JsonConvert.DeserializeObject<V2TResponse>(contentResponse);
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                            else
                            {
                                return new V2TResponse()
                                {
                                    GeneralError = false,
                                    IsAuthorized = true,
                                    Response = contentResponse
                                };

                            }
                        }

                        return new V2TResponse()
                        {
                            GeneralError = true,
                            IsAuthorized = false
                        };
                    default:
                        return new V2TResponse()
                        {
                            GeneralError = true
                        };
                }
            }
            catch (Exception ex)
            {
                return new V2TResponse()
                {
                    GeneralError = true
                };
            }

        }

        #endregion

        #region ЕПЕП

        public Task<CaseMigrationResult> EpepGetResultCaseMigration(Guid gid)
        {
            return requestEPEPdata<CaseMigrationResult>($"epep/GetResultCaseMigration/{gid}");
        }

        public Task<SummaryCase> EpepGetSummaryCase(Guid gid)
        {
            return requestEPEPdata<SummaryCase>($"epep/GetSummaryCase/{gid}");
        }

        public Task<RestEpep.ExecProcessDetailsVM> EpepGetExecProcess(Guid gid)
        {
           
            return requestEPEPdata<RestEpep.ExecProcessDetailsVM>($"epep/GetExecProcess/{gid}");
        }

        private async Task<Tres> requestEPEPdata<Tres>(string method) where Tres : class
        {
            var response = await requestData("get", method);

            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    if (!string.IsNullOrEmpty(response.ResponseContent))
                    {
                        return JsonConvert.DeserializeObject<Tres>(response.ResponseContent);
                    }
                    return null;
                default:
                    return null;
            }


        }

        #endregion

        #region Бюлетин за съдимост - валидация на данни преди изпращане

        public async Task<SaveResultVM> CaisValidateBulletin(int bulletinId)
        {
            if (!userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.CaisBulletinValidate))
            {
                return new SaveResultVM(true);
            }


            var response = await requestData("post", $"cais/ValidateBulletin?bulletinId={bulletinId}");
            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    if (!string.IsNullOrEmpty(response.ResponseContent))
                    {
                        return JsonConvert.DeserializeObject<SaveResultVM>(response.ResponseContent);
                    }
                    return null;
                default:
                    return new SaveResultVM(false, "Възникна проблем при извикване на проверка. Моля, опитайте по-късно");
            }
        }

        #endregion

        private async Task<HttpResponseModel> requestData(string httpMethod, string method, HttpContent content = null)
        {
            try
            {
                initProxy();

                HttpResponseMessage resList = null;
                using (var client = new HttpClient())
                {

                    var url = new Uri(new Uri(BASE_PROXY_URL), method);


                    switch (httpMethod)
                    {
                        case "get":
                            resList = await client.GetAsync(url).ConfigureAwait(false);
                            break;
                        case "post":
                            resList = await client.PostAsync(url, content).ConfigureAwait(false);
                            break;
                        default:
                            return new HttpResponseModel()
                            {
                                StatusCode = System.Net.HttpStatusCode.NotImplemented,
                                ResponseContent = string.Empty
                            };
                    }
                    client.Dispose();
                }
                return new HttpResponseModel()
                {
                    StatusCode = resList.StatusCode,
                    ResponseContent = await resList.Content.ReadAsStringAsync().ConfigureAwait(false)
                };

            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"requestData:{method}");
                return new HttpResponseModel()
                {
                    StatusCode = System.Net.HttpStatusCode.Conflict,
                    ResponseContent = string.Empty
                };
            }

        }
    }
}
