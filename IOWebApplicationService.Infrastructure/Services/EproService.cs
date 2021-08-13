using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Http;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplicationService.Infrastructure.Contracts;
using IOWebApplicationService.Infrastructure.Models.EPRO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplicationService.Infrastructure.Services
{
    public class EproService : BaseMQService, IEproService
    {
        private readonly IHttpClientFactory clientFactory;
        private readonly IConfiguration configuration;
        private Uri uploadUrl;
        private HttpClient client;
        private readonly EproCryptoHelper cryptoHelper;

        public EproService(
                IRepository _repo,
                ICdnService _cdnService,
                IHttpClientFactory _clientFactory,
                IConfiguration _configuration,
                EproCryptoHelper _cryptoHelper,
                ILogger<CubipsaService> _logger)
        {
            repo = _repo;
            cdnService = _cdnService;
            logger = _logger;
            configuration = _configuration;
            cryptoHelper = _cryptoHelper;
            clientFactory = _clientFactory;
            this.IntegrationTypeId = NomenclatureConstants.IntegrationTypes.EPRO;
        }
        protected override async Task<bool> InitChanel()
        {
            uploadUrl = new Uri(configuration.GetValue<string>("EPRO:URI"));
            client = clientFactory.CreateClient("eproHttpClient");
            return true;
        }

        protected override async Task CloseChanel()
        {
        }

        protected override async Task SendMQ(MQEpep mq)
        {
            switch (mq.SourceType)
            {
                case SourceTypeSelectVM.CaseLawUnitDismisal:
                    await SendDismissal(mq);
                    break;
                case SourceTypeSelectVM.CaseSelectionProtokol:
                    await SendReplaceJudge(mq);
                    break;
                case SourceTypeSelectVM.CaseSessionActDepersonalized:
                    await SendActData(mq);
                    break;
                default:
                    break;
            }
        }

        private async Task SendDismissal(MQEpep mq)
        {
            var info = repo.AllReadonly<CaseLawUnitDismisal>()
                                .Include(x => x.Case)
                                .ThenInclude(x => x.Court)
                                .Include(x => x.Document)
                                .Include(x => x.CaseSessionAct)
                                .Include(x => x.CaseSessionAct.CaseSession)
                                .Include(x => x.CaseSessionAct.CaseSession.SessionType)
                                .Include(x => x.CaseLawUnit)
                                .Include(x => x.CaseLawUnit.LawUnit)
                                .Include(x => x.Document)
                                .Include(x => x.Document.DocumentType)
                                .Include(x => x.DocumentPerson)
                                .Include(x => x.DocumentPerson.PersonRole)
                                .Where(x => x.Id == mq.SourceId)
                                .Select(x => new
                                {
                                    CourtId = x.Case.CourtId,
                                    CourtCode = x.Case.Court.Code,
                                    CaseType = x.Case.CaseTypeId.ToString(),
                                    CaseNumber = x.Case.RegNumber,
                                    CaseYear = x.Case.RegDate.Year,
                                    JudgeRole = x.CaseLawUnit.JudgeRoleId.ToString(),
                                    DismissalTypeId = x.DismisalTypeId,
                                    x.Description,
                                    //----JudgeModel
                                    IsChairman = x.CaseLawUnit.JudgeDepartmentRoleId == NomenclatureConstants.JudgeDepartmentRole.Predsedatel,
                                    JudgeName = x.CaseLawUnit.LawUnit.FullName,
                                    //----DecisionModel
                                    HearingDate = x.CaseSessionAct.CaseSession.DateFrom,
                                    HearingType = (x.CaseSessionAct.CaseSession.SessionType.SessionTypeGroup ?? 0).ToString(),
                                    ActDeclaredDate = x.CaseSessionAct.ActDeclaredDate ?? x.CaseSessionAct.CaseSession.DateFrom,
                                    ActNumber = x.CaseSessionAct.RegNumber,
                                    ActTypeId = x.CaseSessionAct.ActTypeId,
                                    //----ObjectionModel
                                    ObjectionUpheld = (x.DismissalStateId ?? NomenclatureConstants.DismissalStates.Confirmed) == NomenclatureConstants.DismissalStates.Confirmed,
                                    DismissalStateId = x.DismissalStateId,
                                    DocumentType = (x.Document != null) ? x.Document.DocumentType.Label : "",
                                    DocumentNumber = (x.Document != null) ? x.Document.DocumentNumberValue ?? 0 : 0,
                                    DocumentDate = (x.Document != null) ? x.Document.DocumentDate : (DateTime?)null,
                                    PersonName = (x.DocumentPerson != null) ? x.DocumentPerson.FullName : null,
                                    PersonRole = (x.DocumentPerson != null) ? x.DocumentPerson.PersonRole.Label : null
                                })
                                .FirstOrDefault();

            var data = new DismissalRegistrationRequest()
            {
                Court = info.CourtCode,
                CaseType = GetNomValue(EpepConstants.Nomenclatures.CaseTypes, info.CaseType),
                DismissalType = info.DismissalTypeId.ToString(),
                CaseNumber = info.CaseNumber,
                CaseYear = info.CaseYear,
                CaseRole = info.JudgeRole,
                ObjectionUpheld = info.ObjectionUpheld,
                DismissalReason = info.Description,
                Judge = new JudgeModel()
                {
                    IsChairman = info.IsChairman,
                    JudgeName = info.JudgeName
                },
                Decision = new DecisionModel()
                {
                    HearingDate = info.HearingDate,
                    HearingType = info.HearingType,
                    ActDeclaredDate = info.ActDeclaredDate,
                    ActNumber = int.Parse(info.ActNumber),
                    ActType = GetNomValue(EpepConstants.Nomenclatures.ActTypes, info.ActTypeId)
                }
            };
            if (info.DismissalTypeId == NomenclatureConstants.DismisalType.Otvod && info.DocumentDate != null)
            {
                data.Objection = new ObjectionModel()
                {
                    DocumentType = info.DocumentType,
                    DocumentNumber = info.DocumentNumber,
                    DocumentDate = info.DocumentDate.Value,
                    SideName = info.PersonName,
                    SideInvolmentKind = info.PersonRole
                };
            }


            var response = await sendDataToEPRO<DismissalRegistrationResponse>(info.CourtId, "DismissalInsert", data);
            if (response != null && response.DismissalId.HasValue)
            {
                AddIntegrationKey(mq, response.DismissalId, false);
                return;
            }
            else
            {
                SetErrorToMQ(mq, EpepConstants.IntegrationStates.DataContentError, response.Error?.GetErrorDescription());
            }
        }

        private async Task SendReplaceJudge(MQEpep mq)
        {
            var info = repo.AllReadonly<CaseLawUnit>()
                                .Include(x => x.CaseSelectionProtokol)
                                .Include(x => x.LawUnit)
                                .Where(x => x.CaseSelectionProtokolId == mq.SourceId && x.CaseSession == null)
                                .OrderBy(x => x.Id)
                                .Select(x => new
                                {
                                    CourtId = x.Case.CourtId,
                                    CourtCode = x.Case.Court.Code,
                                    DismissalId = x.CaseSelectionProtokol.CaseLawUnitDismisalId ?? 0,
                                    ReplaceJudgeName = x.LawUnit.FullName,
                                    IsChairman = x.JudgeDepartmentRoleId == NomenclatureConstants.JudgeDepartmentRole.Predsedatel
                                })
                                .FirstOrDefault();

            var data = new ReplaceDismissalRequest()
            {
                DismissalId = getKeyGuid(SourceTypeSelectVM.CaseLawUnitDismisal, info.DismissalId),
                ReplaceJudge = new JudgeModel()
                {
                    IsChairman = info.IsChairman,
                    JudgeName = info.ReplaceJudgeName
                }
            };

            if (data.DismissalId == Guid.Empty)
            {
                SetErrorToMQ(mq, EpepConstants.IntegrationStates.WaitForParentIdError, "Изчаква код на отвод");
                return;
            }

            var response = await sendDataToEPRO<UpdateResponse>(info.CourtId, "ReplaceUpdate", data);
            if (response != null && response.UpdateSuccessful)
            {
                UpdateMQ(mq, true);
                return;
            }
            else
            {
                SetErrorToMQ(mq, EpepConstants.IntegrationStates.DataContentError, response.Error?.GetErrorDescription());
            }
        }

        private async Task SendActData(MQEpep mq)
        {
            var fileInfo = await cdnService.MongoCdn_Download(new CdnFileSelect()
            {
                SourceType = SourceTypeSelectVM.CaseSessionActDepersonalized,
                SourceId = mq.SourceId.ToString()
            });

            if (fileInfo == null)
            {
                SetErrorToMQ(mq, EpepConstants.IntegrationStates.WaitForParentIdError, "Няма обезличен файл");
                return;
            }

            var info = repo.AllReadonly<CaseLawUnitDismisal>()
                                .Where(x => x.Id == mq.ParentSourceId)
                                .Select(x => new
                                {
                                    CourtId = x.CourtId ?? 0,
                                    DismissalId = x.Id
                                }).FirstOrDefault();

            var data = new ActPublicationRequest()
            {
                DismissalId = getKeyGuid(SourceTypeSelectVM.CaseLawUnitDismisal, info.DismissalId),
                FileName = fileInfo.FileName,
                MimeType = fileInfo.ContentType,
                FileSource = fileInfo.FileContentBase64
            };

            if (data.DismissalId == Guid.Empty)
            {
                SetErrorToMQ(mq, EpepConstants.IntegrationStates.WaitForParentIdError, "Изчаква код на отвод");
                return;
            }

            var response = await sendDataToEPRO<UpdateResponse>(info.CourtId, "ActUpdate", data);
            if (response != null && response.UpdateSuccessful)
            {
                UpdateMQ(mq, true);
                return;
            }
            else
            {
                SetErrorToMQ(mq, EpepConstants.IntegrationStates.DataContentError, response.Error?.GetErrorDescription());
            }
        }

        private async Task<Tresponse> sendDataToEPRO<Tresponse>(int courtId, string methodName, object data) where Tresponse : class, IBaseEproResponseModel
        {
            var apiKey = repo.AllReadonly<CourtApiKey>()
                                    .Where(x => x.CourtId == courtId)
                                    .Select(x => new
                                    {
                                        x.Key,
                                        x.Secret
                                    }).FirstOrDefault();
            if (apiKey == null)
            {
                return null;
            }
            string requestBody = JsonConvert.SerializeObject(data);
            var requestBytes = System.Text.Encoding.UTF8.GetBytes(requestBody);
            var hass = cryptoHelper.ComputeHash(requestBytes, apiKey.Secret);
            var autorizationToken = $"{apiKey.Key}.{hass}";


            Uri address = new Uri(uploadUrl, methodName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autorizationToken);
            HttpContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(address.AbsoluteUri, content);
            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Tresponse>(responseContent);
            }
            else
            {
                throw new Exception($"Response Error : {response.StatusCode.ToString()}");
            }
        }
    }

    public class EproCryptoHelper
    {
        public string ComputeHash(byte[] data, string secret)
        {
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                byte[] computedHash = hmac.ComputeHash(data);

                return ToHexString(computedHash);
            }
        }
        /// <summary>
        /// Кодира текст в шестнайсетичен код
        /// </summary>
        /// <param name="bytes">Текста за кодиране, 
        /// като масив от байтове</param>
        /// <returns>текст в шестнайсетичен код</returns>
        private string ToHexString(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (var t in bytes)
            {
                sb.Append(t.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}



