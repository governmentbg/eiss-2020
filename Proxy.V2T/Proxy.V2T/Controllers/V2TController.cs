// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Proxy.V2T.Core;
using Proxy.V2T.Core.Models;
using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Proxy.EISS.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class V2TController : ControllerBase
    {

        private readonly ILogger<V2TController> _logger;
        private readonly string BASE_URL;
        private readonly int LIST_SIZE;
        private readonly string TEST_HEADER;

        private readonly string CERT_PATH;
        private readonly string CERT_PASSWORD;
        private readonly string AuthorizationMode;

        public V2TController(ILogger<V2TController> logger, IConfiguration config)
        {
            _logger = logger;
            BASE_URL = config.GetValue<string>("Voice2Text:URL");
            LIST_SIZE = config.GetValue<int>("Voice2Text:ListSize", 20);
            AuthorizationMode = config.GetValue<string>("Voice2Text:AuthorizationMode", Constants.AuthorizationModes.Certificate);
            TEST_HEADER = config.GetValue<string>("Voice2Text:TEST_HEADER", "");
            CERT_PATH = config.GetValue<string>("Voice2Text:CERT_PATH", "");
            CERT_PASSWORD = config.GetValue<string>("Voice2Text:CERT_PASSWORD", "");
        }

        [HttpGet]
        [Route("index", Name = "index")]
        public string Index()
        {
            return $"Proxy.V2T -  Base URL: {BASE_URL};AuthMode: {AuthorizationMode}; List size: {LIST_SIZE}; TestCertificate: {!string.IsNullOrEmpty(CERT_PATH)}";
        }

        [HttpPost]
        [Route("isauthorized", Name = "isauthorized")]
        public async Task<V2TResponse> IsAuthorized([FromBody] V2TRequest request)
        {
            var res = await getV2Tresponse("textfile.list?limit=1", request?.UserUIC).ConfigureAwait(false);

            if (res == null)
            {
                return new V2TResponse()
                {
                    GeneralError = true,
                    IsAuthorized = false
                };
            }

            return res;
        }

        [HttpPost]
        [Route("list", Name = "list")]
        public async Task<V2TFileList[]> List([FromBody] V2TRequest request)
        {
            var res = await getV2Tresponse($"textfile.list?name={request?.Term}&limit={LIST_SIZE}", request?.UserUIC).ConfigureAwait(false);
            if (res.GeneralError || !res.IsAuthorized)
            {
                return null;
            }
            try
            {
                var result = JsonConvert.DeserializeObject<V2TFileList[]>(res.Response);
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        [Route("test", Name = "test")]
        public async Task<V2TFileList[]> Test()
        {
            var res = await getV2Tresponse($"textfile.list", "8711198726");
            if (res.GeneralError || !res.IsAuthorized)
            {
                return null;
            }
            try
            {
                var result = JsonConvert.DeserializeObject<V2TFileList[]>(res.Response);
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpPost]
        [Route("filecontent", Name = "filecontent")]
        public async Task<string> FileContent([FromBody] V2TRequest request)
        {
            var res = await getV2Tresponse($"textfile.content?id={request?.Id}", request?.UserUIC).ConfigureAwait(false);
            if (res.GeneralError || !res.IsAuthorized)
            {
                return null;
            }
            return JsonConvert.DeserializeObject<string>(res.Response);
        }


        private async Task<V2TResponse> getV2Tresponse(string method, string userUIC)
        {
            X509Certificate2 clientCertificate = null;

            try
            {
                string header = string.Empty;


                switch (AuthorizationMode)
                {
                    case Constants.AuthorizationModes.CertificateHeader:
                        {
                            clientCertificate = new X509Certificate2(CERT_PATH, CERT_PASSWORD);
                            if (!string.IsNullOrEmpty(TEST_HEADER))
                            {
                                header = TEST_HEADER;
                            }
                            else
                            {
                                header = $"SERIALNUMBER=PNOBG-{userUIC}";
                            }
                        }
                        break;

                    default:
                        break;
                }

                HttpClientHandler handler = new HttpClientHandler();

                if (clientCertificate != null && (AuthorizationMode == Constants.AuthorizationModes.Certificate || AuthorizationMode == Constants.AuthorizationModes.CertificateHeader))
                {
                    handler.ClientCertificates.Add(clientCertificate);
                }
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                

                HttpClient client = new HttpClient(handler);
                if (!string.IsNullOrEmpty(header) && (AuthorizationMode == Constants.AuthorizationModes.Header || AuthorizationMode == Constants.AuthorizationModes.CertificateHeader))
                {
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    client.DefaultRequestHeaders.Add("X-Client-Cert-Subject", header);
                }

                var url = new Uri(new Uri(BASE_URL), method);

                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls13;

                var resList = await client.GetAsync(url).ConfigureAwait(false);
                client.Dispose();
                if (resList.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    _logger.LogError($"Response status : {resList.StatusCode}");
                }
                switch (resList.StatusCode)
                {
                    case System.Net.HttpStatusCode.Forbidden:
                        return new V2TResponse()
                        {
                            IsAuthorized = false
                        };
                    case System.Net.HttpStatusCode.OK:
                        return new V2TResponse()
                        {
                            IsAuthorized = true,
                            Response = await resList.Content.ReadAsStringAsync().ConfigureAwait(false)
                        };
                    default:
                        return new V2TResponse()
                        {
                            GeneralError = true,
                            Response = $"GeneralError:{url}: {resList.StatusCode}; {resList.ReasonPhrase}"
                        };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "getV2Tresponse:method");
                return new V2TResponse()
                {
                    GeneralError = true,
                    Response = $"{ex.Message}; {ex.InnerException?.Message}; {ex.InnerException?.InnerException?.Message}"
                };
            }

        }
    }
}
