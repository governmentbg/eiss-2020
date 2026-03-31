using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Extensions;
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
                ILogger<EproService> _logger)
        {
            repo = _repo;
            cdnService = _cdnService;
            logger = _logger;
            configuration = _configuration;
            cryptoHelper = _cryptoHelper;
            clientFactory = _clientFactory;
            this.IntegrationTypeId = NomenclatureConstants.IntegrationTypes.EPRO;
        }
        protected override Task<bool> InitChanel()
        {
            uploadUrl = new Uri(configuration.GetValue<string>("EPRO:URI"));
            client = clientFactory.CreateClient("eproHttpClient");
            return Task.FromResult(true);
        }

        protected override async Task CloseChanel()
        {
            await Task.Yield();
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
            var info = await repo.AllReadonly<CaseLawUnitDismisal>()
                                .Where(x => x.Id == mq.SourceId)
                                .Where(x => x.CaseSessionActId > 0)
                                .Select(x => new
                                {
                                    CourtId = x.Case.CourtId,
                                    CourtCode = x.Case.Court.Code,
                                    CaseType = x.Case.CaseTypeId.ToString(),
                                    CaseNumber = x.Case.RegNumber,
                                    CaseYear = x.Case.RegDate.Year,
                                    JudgeRole = x.CaseLawUnit.JudgeRoleId,
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
                                    DismissalRequestType = x.DismissalRequestType ?? NomenclatureConstants.DismissalRequestTypes.Document,
                                    DocumentType = (x.Document != null) ? x.Document.DocumentType.Label : "",
                                    DocumentNumber = (x.Document != null) ? x.Document.DocumentNumberValue ?? 0 : 0,
                                    DocumentDate = (x.Document != null) ? x.Document.DocumentDate : (DateTime?)null,
                                    DocumentPersonName = (x.DocumentPerson != null) ? x.DocumentPerson.FullName : null,
                                    DocumentPersonRole = (x.DocumentPerson != null) ? x.DocumentPerson.PersonRole.Label : null,

                                    DismissalActType = (x.DismissalSessionAct != null) ? x.DismissalSessionAct.ActType.Label : "",
                                    DismissalActNumber = (x.DismissalSessionAct != null) ? x.DismissalSessionAct.RegNumber : "",
                                    DismissalActDate = (x.DismissalSessionAct != null) ? x.DismissalSessionAct.ActDeclaredDate : (DateTime?)null,
                                    DismissalPersonName = (x.DismissalCasePerson != null) ? x.DismissalCasePerson.FullName : (string)null,
                                    DismissalPersonRole = (x.DismissalCasePerson != null) ? x.DismissalCasePerson.PersonRole.Label : (string)null
                                })
                                .FirstOrDefaultAsync();
            if (info == null)
            {
                SetErrorToMQ(mq, EpepConstants.IntegrationStates.MissingObjectEISS, "Ненамерен отвод или отвод без избран акт.");
            }

            var data = new DismissalRegistrationRequest()
            {
                Court = info.CourtCode,
                CaseType = GetNomValue(EpepConstants.Nomenclatures.CaseTypes, info.CaseType),
                DismissalType = info.DismissalTypeId.ToString(),
                CaseNumber = info.CaseNumber,
                CaseYear = info.CaseYear,
                CaseRole = GetNomValue(EpepConstants.Nomenclatures.EPRO_CaseRole, info.JudgeRole),
                ObjectionUpheld = info.ObjectionUpheld,
                DismissalReason = info.Description.TrimLength(4000),
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
                    ActType = GetNomValue(EpepConstants.Nomenclatures.EPRO_ActType, info.ActTypeId)
                }
            };
            if (info.DismissalTypeId == NomenclatureConstants.DismisalType.Otvod)
            {
                if (info.DismissalRequestType == NomenclatureConstants.DismissalRequestTypes.Document && info.DocumentDate != null)
                {
                    data.Objection = new ObjectionModel()
                    {
                        DocumentType = info.DocumentType,
                        DocumentNumber = info.DocumentNumber,
                        DocumentDate = info.DocumentDate.Value,
                        SideName = info.DocumentPersonName,
                        SideInvolmentKind = info.DocumentPersonRole
                    };
                }
                if (info.DismissalRequestType == NomenclatureConstants.DismissalRequestTypes.Session && info.DismissalActDate != null)
                {
                    data.Objection = new ObjectionModel()
                    {
                        DocumentType = info.DismissalActType,
                        DocumentNumber = int.Parse(info.DismissalActNumber),
                        DocumentDate = info.DismissalActDate.Value,
                        SideName = info.DismissalPersonName ?? " ",
                        SideInvolmentKind = info.DismissalPersonRole ?? " "
                    };
                }
            }

            data.DecodeTexts();
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
            var info = await repo.AllReadonly<CaseLawUnit>()
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
                                .FirstOrDefaultAsync();

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
            var apiKey = await repo.AllReadonly<CourtApiKey>()
                                    .Where(x => x.CourtId == courtId)
                                    .Select(x => new
                                    {
                                        x.Key,
                                        x.Secret
                                    }).FirstOrDefaultAsync();
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
                var resError = Activator.CreateInstance<Tresponse>();
                resError.Error = new ErrorModel()
                {
                    ErrorType = "Response Error",
                    Reason = response.StatusCode.ToString()
                };
                return resError;
                //throw new Exception($"Response Error : {response.StatusCode.ToString()}");
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



