using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.EISPP;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.Integrations.Eispp;
using IOWebApplication.Infrastructure.Utils;
using IOWebApplicationService.Infrastructure.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static IOWebApplication.Infrastructure.Constants.EISPPConstants;
using static IOWebApplication.Infrastructure.Constants.EpepConstants;


namespace IOWebApplicationService.Infrastructure.Services
{
    /// <summary>
    /// Изпращане и получаване на съобщения към и от ядрото на ЕИСПП
    /// </summary>
    public class EisppCommunicationService : BaseMQService, IEisppCommunicationService
    {

        private List<string> eisppReplies;

        private Integration.Eispp.EisppServiceClient eisppClient;

        private readonly IEisppConnectionService connector;

        private readonly IEisppRulesService eisppRulesService;
        #region Settings

        // Package properties

        private readonly string senderNode;

        private readonly string receiverNode;

        private readonly int messageType;

        private readonly int receiverStructure;

        // Context properties

        private readonly int workingMode;

        private readonly int userSystemId;

        private readonly int workPlaceId;

        #endregion
        private readonly string byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());

        /// <summary>
        /// Инжектиране на зависимости
        /// </summary>
        /// <param name="_repo">Достъп до базата данни</param>
        public EisppCommunicationService(
            IRepository _repo,
            ICdnService _cdnService,
            IEisppRulesService _eisppRulesService,
            IConfiguration _config,
            ILogger<EisppCommunicationService> _logger,
            IEisppConnectionService _connector)
        {
            repo = _repo;
            cdnService = _cdnService;
            eisppRulesService = _eisppRulesService;
            IntegrationTypeId = NomenclatureConstants.IntegrationTypes.EISPP;
            logger = _logger;
            senderNode = _config.GetValue<string>("EISPP:SenderNode");
            receiverNode = _config.GetValue<string>("EISPP:ReceiverNode");
            messageType = _config.GetValue<int>("EISPP:MessageType");
            receiverStructure = _config.GetValue<int>("EISPP:ReceiverStructure");
            workingMode = _config.GetValue<int>("EISPP:WorkingMode");
            userSystemId = _config.GetValue<int>("EISPP:UserSystemId");
            workPlaceId = _config.GetValue<int>("EISPP:WorkPlaceId");
            connector = _connector;
        }

   
        public new void SetErrorToMQ(MQEpep mq, int integrationState, string errorDescription)
        {
            mq.ErrorCount = (mq.ErrorCount ?? 0) + 1;
            mq.IntegrationStateId = integrationState;
            mq.ErrorDescription = errorDescription;
            repo.Update(mq);
            repo.SaveChanges();
        }
       
        /// <summary>
        /// Инициализира канала за връзка ЕИСПП
        /// </summary>
        /// <returns></returns>
        protected override async Task<bool> InitChanel()
        {
            bool result = false;

            try
            {
                eisppReplies = new List<string>();
                eisppClient = await connector.Connect();
                if (eisppClient.State != System.ServiceModel.CommunicationState.Opened)
                {
                    logger.LogError(new Exception("EisppClient.State"), $"Error opening channel {eisppClient.State}!");
                }

                result = true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error initiating channel!");
            }

            return result;
        }

        
        /// <summary>
        /// Изпраща съобщение до ЕИСПП
        /// </summary>
        /// <param name="mq">Съобщение от опашката</param>
        protected override async Task SendMQ(MQEpep mq)
        {
            try
            {
                string message = null;
                bool isReadyMessage = false;
                
                if (mq.Content != null)
                {
                    message = Encoding.UTF8.GetString(mq.Content);
                   // isReadyMessage = true;
                }
                else
                {

                    var eisppEventItem = repo.AllReadonly<EisppEventItem>()
                                             .Where(x => x.MQEpepId == mq.Id)
                                            .FirstOrDefault();
                    if (!string.IsNullOrEmpty(eisppEventItem.RequestXML))
                    {
                        isReadyMessage = true;
                        message = eisppEventItem.RequestXML;
                    }
                    else
                    {
                        var model = JsonConvert.DeserializeObject<EisppPackage>(eisppEventItem.RequestData);
                        eisppRulesService.SetIsSelectedAndClear(model);
                        eisppRulesService.CreatePunismentFromProbationMeasuares(model);
                        message = XmlUtils.SerializeEisppPackage(model);
                    }
                }
                if (!isReadyMessage)
                {
                    if (message.StartsWith(byteOrderMarkUtf8, StringComparison.Ordinal))
                    {
                        message = message.Remove(0, byteOrderMarkUtf8.Length);
                    }

                    EisppPackage package = XmlUtils.DeserializeXml<EisppPackage>(message);

                    if (package != null)
                    {
                        var eisppEvent = package.Data.Events.Where(x => x.EventKind == EventKind.NewEvent).FirstOrDefault();
                        if (eisppEvent == null && package.Data.Events.Length > 0)
                            eisppEvent = package.Data.Events[0];
                        var structureId = eisppEvent.StructureId;
                        var eventType = eisppEvent.EventType;

                        package.CorrelationId = mq.MQId;
                        package.SenderNode = senderNode;
                        package.ReceiverNode = receiverNode;
                        package.ReceiverStructure = receiverStructure;
                        package.MessageType = messageType;

                        int eventTypeResSid = eventType;
                        var eisppEventOld = package.Data.Events.Where(x => x.EventKind == EventKind.OldEvent).FirstOrDefault();
                        if (eisppEventOld?.EventType > 0)
                        {
                            eventTypeResSid = EventType.DeleteEvent;
                            if (eisppEvent.EventKind == EventKind.NewEvent)
                                eventTypeResSid = EventType.ChangeEvent;
                        }

                        package.Data.Context = new Context()
                        {
                            StructureId = structureId,
                            EventDate = eisppEvent.EventDate,
                            EventType = eventType > 0 ? eventType : 0,
                            ResourceId = eisppRulesService.GetResSidFromRules(eventTypeResSid)
                        };
                        if (package.Data.Context.ResourceId <= 0)
                            package.Data.Context.ResourceId = GetNomValueInt(EISPPConstants.EisppMapping.ResSid, eisppEvent.CriminalProceeding.Case.LegalProceedingType);

                        if (package.Data?.Context != null)
                        {
                            package.Data.Context.WorkingMode = workingMode;
                            package.Data.Context.UserSystemId = userSystemId;
                            package.Data.Context.WorkPlaceId = workPlaceId;
                        }
                        else
                        {
                            throw new ArgumentException("Context is required");
                        }

                        foreach (var eisppEventItem in package.Data.Events)
                        {
                            if (eisppEventItem.CriminalProceeding?.Case?.Persons == null)
                                continue;
                            foreach (var person in eisppEventItem.CriminalProceeding.Case.Persons)
                            {
                                if (person.BirthPlace != null)
                                {
                                    if ((person.BirthPlace.SettelmentAbroad == "0" || string.IsNullOrEmpty(person.BirthPlace.SettelmentAbroad)) &&
                                          person.BirthPlace.SettelmentBg == 0 && person.BirthPlace.PlaceId == 0)
                                    {
                                        person.BirthPlace = null;
                                    }
                                }
                            }
                        }
                        if (eventType == EventType.GetCase)
                        {
                            eisppEvent.CriminalProceeding.Case.CaseSetupType = -1;
                            eisppEvent.CriminalProceeding.Case.LegalProceedingType = -1;
                        }

                        message = XmlUtils.SerializeEisppPackage(package);
                        message = await eisppRulesService.ApplyRules(structureId, message, eventType);
                    }
                    else
                    {
                        message = string.Empty;
                    }
                }
                if (!string.IsNullOrEmpty(message))
                {
                    mq.Content = Encoding.UTF8.GetBytes(message);
                    if (await Send(message))
                    {
                        MarkAsSent(mq, message);
                    }
                    else
                    {
                        throw new ApplicationException("Error sending message!");
                    }
                }
                else
                {
                    throw new ArgumentException("At least one Event is required");
                }
            }
            catch (ArgumentException ex)
            {
                SetErrorToMQ(mq, IntegrationStates.DataContentError, ex.Message);
            }
            catch (InvalidOperationException iop)
            {
                SetErrorToMQ(mq, IntegrationStates.DataContentError, $"The content is not a valid EISPP package {iop.Message}");
            }
        }

        /// <summary>
        /// Прочита ако има отговори
        /// Затваря канала за връзка с ЕИСПП
        /// </summary>
        /// <returns></returns>
        protected override async Task CloseChanel()
        {
            try
            {
                var messages = await Receive();
                eisppReplies.AddRange(messages);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error receiving messages!");
            };
            try
            {
                await eisppClient.CloseAsync();
                eisppClient = null;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error closing channel!");
            }

            await ProcessReplies();
        }


        /// <summary>
        /// Обработва отговорите на ЕИСПП
        /// </summary>
        /// <returns></returns>
        private async Task ProcessReplies()
        {
            foreach (var item in eisppReplies)
            {
                try
                {
                    XDocument message = XDocument.Parse(item);
                    string correlationId = message?.Descendants("Property")
                        .Where(e => e.Attribute("name")?.Value == "correlation_id")
                        .Select(e => e.Attribute("value")?.Value)
                        .FirstOrDefault();

                    XElement result = message?.Element("EISPPMessage")?.Element("EISPPPAKET")?.Element("DATA")?.Element("RZT");
                    XElement exception = result?.Element("Exception");
                    XElement htmlCard = result?.Element("KartaNPR");

                    if (correlationId != null)
                    {
                        string eisppNumber = result
                            ?.Descendants()
                            .Where(e => e.NodeType == System.Xml.XmlNodeType.Attribute)
                            .Where(a => a.Name == "nprnmr")
                            .Select(a => a.Value)
                            .FirstOrDefault() ?? "";
                        await SaveResponse(correlationId, item, eisppNumber);
                        if (exception != null)
                        {
                            SetError(correlationId, exception.Value);
                        }
                        else if (htmlCard != null)
                        {
                            await SaveCard(correlationId, htmlCard.Value, eisppNumber);
                        }
                        else if (result != null)
                        {
                            SetOk(correlationId);
                        }
                        else
                        {
                            SetError(correlationId, "Липсва отговор от ЕИСПП!");
                        }
                    } else
                    {
                        var eisppSignal = new EisppSignal();
                        eisppSignal.ResponseData = item;
                        eisppSignal.DateCreate = DateTime.Now;
                        repo.Add(eisppSignal);
                        repo.SaveChanges();
                        XElement signal = message?.Element("EISPPPAKET")?.Element("DATA")?.Element("SNOSAO");
                        if (signal != null)
                        {
                            eisppSignal.Message = signal.Value;
                            eisppSignal.StructureId = signal.Attribute("adrstr").Value.ToInt();
                            eisppSignal.EISSPNumber = signal.Attribute("adrnprnmr").Value;
                            eisppSignal.CaseType = signal.Attribute("adrdlovid").Value.ToInt();
                            eisppSignal.ExactCaseType = signal.Attribute("adrdlosig").Value.ToInt();
                            eisppSignal.CaseYear = signal.Attribute("adrdlogdn").Value.ToInt();
                            eisppSignal.ShortNumber = signal.Attribute("adrdlonmr").Value.ToInt();
                            repo.Update(eisppSignal);
                            repo.SaveChanges();
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing replies!");
                }
            }
        }

        /// <summary>
        /// Маркира трансфера като успешен
        /// </summary>
        /// <param name="correlationId">Идентификатора на съобщението</param>
        private void SetOk(string correlationId, MQEpep mq = null)
        {
            try
            {
                if (mq == null)
                {
                    mq = repo.All<MQEpep>()
                        .FirstOrDefault(m => m.MQId == correlationId);
                }

                mq.IntegrationStateId = IntegrationStates.TransferOK;

                repo.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in SetOK with corelationId: {correlationId}!", correlationId);
            }
        }

        /// <summary>
        /// Записва ЕИСПП карта
        /// </summary>
        /// <param name="correlationId">Идентификатор на съобщение</param>
        /// <param name="htmlCard">Получена карта</param>
        /// <param name="eisppNumber">ЕИСПП номер на Наказателно Производство</param>
        private async Task SaveCard(string correlationId, string htmlCard, string eisppNumber)
        {
            MQEpep mq = null;

            try
            {
                mq = repo.All<MQEpep>()
                       .FirstOrDefault(m => m.MQId == correlationId);

                string decodedCard = "<!DOCTYPE html>\n" + WebUtility.HtmlDecode(htmlCard);
                string fileName = $"EISPP_Card_{eisppNumber}_{ DateTime.Today.ToString("dd.MM.yyyy") }.html";
                CdnUploadRequest request = new CdnUploadRequest()
                {
                    ContentType = System.Net.Mime.MediaTypeNames.Text.Html,
                    FileContentBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(decodedCard)),
                    FileName = fileName,
                    SourceId = mq.ParentSourceId.ToString(),
                    SourceType = IOWebApplication.Infrastructure.Models.ViewModels.Common.SourceTypeSelectVM.Integration_EISPP_CardNP,
                    Title = $"КАРТА ЗА СЪСТОЯНИЕ НА НП: {eisppNumber} към { DateTime.Today.ToString("dd.MM.yyyy") }"
                };

                if (!(await cdnService.MongoCdn_AppendUpdate(request)))
                {
                    logger.LogError("Error in SaveCard with corelationId: {correlationId}!", correlationId);
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in SaveCard with corelationId: {correlationId}!", correlationId); ;
            }

            SetOk(correlationId, mq);
        }
        // <summary>
        /// Записва ЕИСПП Отговор
        /// </summary>
        /// <param name="correlationId">Идентификатор на съобщение</param>
        /// <param name="htmlCard">Получена карта</param>
        /// <param name="eisppNumber">ЕИСПП номер на Наказателно Производство</param>
        private async Task SaveResponse(string correlationId, string response, string eisppNumber)
        {
            MQEpep mq = null;

            try
            {
                mq = repo.All<MQEpep>()
                       .FirstOrDefault(m => m.MQId == correlationId);

                string fileName = $"EISPP_Response_{eisppNumber}_{ DateTime.Today.ToString("dd.MM.yyyy_HH:mm:ss") }.xml";
                CdnUploadRequest request = new CdnUploadRequest()
                {
                    ContentType = System.Net.Mime.MediaTypeNames.Text.Html,
                    FileContentBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(response)),
                    FileName = fileName,
                    SourceId = correlationId, //mq.ParentSourceId.ToString(),
                    SourceType = IOWebApplication.Infrastructure.Models.ViewModels.Common.SourceTypeSelectVM.Integration_EISPP_Response,
                    Title = $"ОТГОВОР За Събитие НП: {eisppNumber} към { DateTime.Today.ToString("dd.MM.yyyy") }"
                };

                if (!(await cdnService.MongoCdn_AppendUpdate(request)))
                {
                    logger.LogError("Error in SaveCard with corelationId: {correlationId}!", correlationId);
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in SaveCard with corelationId: {correlationId}!", correlationId); ;
            }

            SetOk(correlationId, mq);
        }

        /// <summary>
        /// Записва грешка
        /// </summary>
        /// <param name="correlationId">Идентификатора на съобщението</param>
        /// <param name="exception">Възникнала грешка</param>
        private void SetError(string correlationId, string exception)
        {
            try
            {
                MQEpep mq = repo.All<MQEpep>()
                .FirstOrDefault(m => m.MQId == correlationId);

                mq.IntegrationStateId = EpepConstants.IntegrationStates.ReplyContainsError;
                mq.ErrorDescription = exception;

                repo.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in SetError with corelationId: {correlationId}, error: {exception}!", correlationId, exception);
            }
        }

        /// <summary>
        /// Изпращане на съобщение към ядрото на ЕИСПП
        /// </summary>
        /// <param name="message">Съобщение, което се изпраща</param>
        private async Task<bool> Send(string message)
        {
            return await eisppClient.SendMessageAsync(message);
        }

        /// <summary>
        /// Получаване на съобщения от ядрото на ЕИСПП
        /// </summary>
        /// <returns></returns>
        private async Task<string[]> Receive()
        {
            return await  eisppClient.ReceiveMessagesAsync();
        }

        /// <summary>
        /// Обновява съдържанието и статуса на ЕИСПП съобщение 
        /// </summary>
        /// <param name="mq">Обект от опашката</param>
        /// <param name="message">Сериализиран ЕИСПП пакет</param>
        private void MarkAsSent(MQEpep mq, string message)
        {
            try
            {
                mq.DateTransfered = DateTime.Now;
                mq.IntegrationStateId = EpepConstants.IntegrationStates.WaitingForReply;
                mq.Content = Encoding.UTF8.GetBytes(message);

                repo.Update(mq);
                repo.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in SetOK with MQId: {MQId}!", mq.Id);
            }
        }
    }
}
