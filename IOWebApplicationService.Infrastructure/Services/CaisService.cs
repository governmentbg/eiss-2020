using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Http;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplicationService.Infrastructure.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using static IOWebApplication.Infrastructure.Constants.EpepConstants;

namespace IOWebApplicationService.Infrastructure.Services
{
    public class CaisService : BaseMQService, ICaisService
    {
        private readonly ICaisConnectionFactory caisFactory;

        public CaisService(
            IConfiguration _config,
            IRepository _repo,
            ICdnService _cdnService,
            ICaisConnectionFactory _caisFactory,
            ILogger<CaisService> _logger)
        {
            this.repo = _repo;
            logger = _logger;
            this.cdnService = _cdnService;
            this.IntegrationTypeId = NomenclatureConstants.IntegrationTypes.Cais;
            caisFactory = _caisFactory;
            //this.mqID = 129696;
        }


        protected override Task<bool> InitChanel()
        {
            caisFactory.CreateClient("caisHttpClient");
            return Task.Run(() => true);
        }

        protected override async Task SendMQ(MQEpep mq)
        {
            this.startTime = null;
            try
            {

                var xml = await cdnService.MongoCdn_Download(new CdnFileSelect() { SourceId = mq.ParentSourceId.ToString(), SourceType = SourceTypeSelectVM.CasePersonBulletinXml });
                if (xml == null)
                {
                    SetErrorToMQ(mq, IntegrationStates.DataContentError, "Липсващ или грешен xml файл на бюлетин");
                    return;
                }

                var xmlData = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(xml.FileContentBase64));

                var bulletinResult = await caisFactory.SendDataToCais(xmlData, "SendBulletinsData");
                var forUpdate = false;

                if (bulletinResult.Result)
                {
                    UpdateMQ(mq, true);
                    forUpdate = true;
                }
                else
                {
                    if (!string.IsNullOrEmpty(bulletinResult.Content))
                    {
                        //Грешка при валидиране на данните
                        SetErrorToMQ(mq, IntegrationStates.DataContentError, bulletinResult.Content);
                        forUpdate = true;
                    }
                    else
                    {
                        SetErrorToMQ(mq, IntegrationStates.TransferError, bulletinResult.ErrorMessage);
                    }
                }

                if (forUpdate)
                {
                    var bulletinFile = await repo.GetByIdAsync<CasePersonSentenceBulletinFile>((int)mq.ParentSourceId);
                    if (bulletinFile != null)
                    {
                        bulletinFile.DateSubmited = DateTime.Now;
                        await repo.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                mq.ErrorDescription = $"{ex.Message};{ex.InnerException?.Message}";
                UpdateMQ(mq, false);
            }
        }

        /*
        async Task<SaveResultVM> sendDataToCais(string xml, long mqId)
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
            var soapXML = SoapHelper.ConstructSoap(processedXml, "SendBulletinsData", "http://cs.mjs.bg/EISSServicesModel-v1.0");

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
                return new SaveResultVM(false, "Грешка в данните");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"");
                return new SaveResultVM(false, $"{ex.Message}; {ex.InnerException?.Message}");
            }
        }
        */

        protected override async Task CloseChanel()
        {
            //if (client != null)
            //{
            //    if (client. == System.ServiceModel.CommunicationState.Opened)
            //    {
            //        client.Close();
            //    }
            //    client = null;
            //}
            await Task.Yield();
        }
    }
}
