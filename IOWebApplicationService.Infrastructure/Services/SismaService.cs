using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.Integrations.Sisma;
using IOWebApplicationService.Infrastructure.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplicationService.Infrastructure.Services
{
    public class SismaService : BaseMQService, ISismaService
    {
        private readonly IConfiguration config;
        private readonly IHttpClientFactory clientFactory;
        private readonly IStatisticsReportService reportService;
        //private readonly string autoDays;
        private Uri uploadUrl;
        private HttpClient client;

        public SismaService(
            IConfiguration _config,
            IHttpClientFactory _clientFactory,
            IRepository _repo,
            ILogger<SismaService> _logger,
            IStatisticsReportService _reportService)
        {
            this.repo = _repo;
            config = _config;
            logger = _logger;
            reportService = _reportService;
            clientFactory = _clientFactory;
            this.IntegrationTypeId = NomenclatureConstants.IntegrationTypes.Sisma;
            //autoDays = config.GetValue<string>("SISMA:AutoCalcDays");
            // mqID = 22244101;
        }

        public override async Task<bool> FetchResult()
        {
            try
            {

                var reportDate = DateTime.Now.AddMonths(-1);

                //reportDate = new DateTime(2022, 1, 1);

                var mqID = $"{reportDate:yyyy-MM}";

                if (!repo.AllReadonly<MQEpep>()
                            .Where(x => x.IntegrationTypeId == this.IntegrationTypeId && x.MQId == mqID)
                            .Any())
                {


                    //29.03.2022 - Към момента са 25 каталозите на ЕИСС
                    for (int catalogNo = 1; catalogNo <= 25; catalogNo++)
                    {
                        var request = JsonConvert.SerializeObject(new SismaMqRequestModel()
                        {
                            Month = reportDate.Month,
                            Year = reportDate.Year,
                            CatalogNo = catalogNo
                        });
                        var newMq = new MQEpep()
                        {
                            DateWrt = DateTime.Now,
                            IntegrationTypeId = this.IntegrationTypeId,
                            IntegrationStateId = EpepConstants.IntegrationStates.New,
                            SourceType = catalogNo,
                            SourceId = long.Parse($"{reportDate.Year}{reportDate.Month:D2}"),
                            TargetClassName = $"Автоматично стартиране",
                            //Има право на 5 грешки, защото операцията продължава много дълго
                            ErrorCount = EpepConstants.IntegrationMaxErrorCount - 5,
                            Content = Encoding.UTF8.GetBytes(request),
                            MQId = mqID
                        };

                        repo.Add(newMq);
                    }

                    await repo.SaveChangesAsync();

                }

                await PushMQWithFetch(100);
            }
            finally
            {

            }
            return true;
        }

        protected override async Task SendMQ(MQEpep mq)
        {
            var request = JsonConvert.DeserializeObject<SismaMqRequestModel>(Encoding.UTF8.GetString(mq.Content));

            try
            {
                var sismaModel = await reportService.GetSismaData(request.Month, request.Year, request.CatalogNo, $"1-{request.CatalogNo:D2}");

                var response = await sendDataToSISMA(sismaModel);
                if (response != null)
                {
                    if (response.ResultCode == SismaConstants.ErrorCodes.OK)
                    {
                        UpdateMQ(mq, true);
                    }
                    else
                    {
                        mq.ErrorDescription = $"{response.ResultCode}, {response.Message}";
                        UpdateMQ(mq, false);
                    }
                }
                else
                {
                    mq.ErrorDescription = $"Грешка при изпращане на данни към СИСМА!";
                    UpdateMQ(mq, false);
                }
            }
            catch (Exception ex)
            {
                mq.ErrorDescription = ex.Message;
                UpdateMQ(mq, false);
            }
        }

        private async Task<SismaResponseModel> sendDataToSISMA(object data)
        {
            //var apiKey = repo.AllReadonly<CourtApiKey>()
            //                        .Where(x => x.CourtId == courtId)
            //                        .Select(x => new
            //                        {
            //                            x.Key,
            //                            x.Secret
            //                        }).FirstOrDefault();
            //if (apiKey == null)
            //{
            //    return null;
            //}
            string requestBody = JsonConvert.SerializeObject(data);
            var requestBytes = System.Text.Encoding.UTF8.GetBytes(requestBody);
            //var hass = cryptoHelper.ComputeHash(requestBytes, apiKey.Secret);
            //var autorizationToken = $"{apiKey.Key}.{hass}";


            //Uri address = new Uri(uploadUrl, "");
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autorizationToken);
            HttpContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(uploadUrl, content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<SismaResponseModel>(responseContent);
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return new SismaResponseModel() { ResultCode = SismaConstants.ErrorCodes.GeneralException, Message = responseContent };
                }
                else
                {
                    throw new Exception($"Response Error : {response.StatusCode}");
                }
            }
        }

        protected override Task<bool> InitChanel()
        {
            uploadUrl = new Uri(config.GetValue<string>("SISMA:URI"));
            client = clientFactory.CreateClient("sismaHttpClient");
            return Task.Run(() => true);
        }
    }
}
