// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using DnsClient.Internal;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Http;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace IOWebApplication.Infrastructure.Services
{
    public class CaisConnectionFactory : ICaisConnectionFactory, IDisposable
    {
        private readonly string BaseURI;

        private readonly IHttpClientFactory clientFactory;
        private HttpClient client;
        private readonly ILogger<CaisConnectionFactory> logger;

        public CaisConnectionFactory(
            IConfiguration config,
            IHttpClientFactory clientFactory,
            ILogger<CaisConnectionFactory> logger)
        {
            this.logger = logger;
            this.clientFactory = clientFactory;
            BaseURI = config.GetValue<string>("CAIS:URI");
        }

        public void CreateClient(string clientName = "caisHttpClient")
        {
            client = clientFactory.CreateClient(clientName);
        }



        public async Task<SaveResultVM> SendDataToCais(string xml, string methodName)
        {
            //Премахване на водещия XmlDocumentaion таг
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            var firstChild = xmlDoc.FirstChild;
            if (firstChild != null && firstChild.NodeType == XmlNodeType.XmlDeclaration)
            {
                xmlDoc.RemoveChild(firstChild);
            }
            

            var processedXml = xmlDoc.OuterXml;

            //Завиване в SOAP envelope
            var soapXML = SoapHelper.ConstructSoap(processedXml, methodName, "http://cs.mjs.bg/EISSServicesModel-v1.0");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, BaseURI)
            {
                Content = new StringContent(soapXML, System.Text.Encoding.UTF8, "text/xml")
            };

            try
            {
                var response = await client.SendAsync(httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    var responseText = await response.Content.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(responseText))
                    {
                        //Извличане на валидационни грешки от ЦАИС
                        var xmlReponse = new XmlDocument();
                        xmlReponse.LoadXml(responseText);
                        var hasErrorNode = xmlReponse.GetElementsByTagName("HasError").Item(0);
                        bool hasError = (hasErrorNode == null) || (hasErrorNode?.InnerText == "true");
                        var errorList = xmlReponse.GetElementsByTagName("ErrorText");
                        var errors = new List<string>();
                        if (errorList != null)
                        {

                            for (int i = 0; i < errorList.Count; i++)
                            {
                                errors.Add(errorList[i].InnerText);
                            }
                        }
                        if (!hasError)
                        {
                            return new SaveResultVM(true);
                        }
                        return new SaveResultVM(false)
                        {
                            Content = string.Join("; ", errors)
                        };
                    }
                    else
                    {
                        return new SaveResultVM(false, "Грешка: Празен отговор от ЦАЙС");
                    }
                }
                else
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                }
                return new SaveResultVM(false, "Грешка в данните");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"");
                return new SaveResultVM(false, $"{ex.Message}; {ex.InnerException?.Message}");
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool suppressFinal)
        {
            if (client != null)
            {
                client.Dispose();
                client = null;
            }
        }
    }
}
