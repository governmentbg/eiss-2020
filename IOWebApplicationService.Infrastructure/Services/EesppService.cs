using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Extensions.HTML;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplicationService.Infrastructure.Contracts;
using IOWebApplicationService.Infrastructure.Models.EESPP;
using Microsoft.EntityFrameworkCore;
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
    public class EesppService : BaseMQService, IEesppService
    {
        private readonly IHttpClientFactory clientFactory;
        private readonly IConfiguration configuration;
        private HttpClient client;
        private readonly EproCryptoHelper cryptoHelper;
        private string baseURL;
        //        private readonly IWorkNotificationService notificationService;

        public EesppService(
                IRepository _repo,
                ICdnService _cdnService,
                IHttpClientFactory _clientFactory,
                IConfiguration _configuration,
                EproCryptoHelper _cryptoHelper,
                ILogger<EesppService> _logger
            //IWorkNotificationService _notificationService
            )
        {
            repo = _repo;
            cdnService = _cdnService;
            logger = _logger;
            configuration = _configuration;
            cryptoHelper = _cryptoHelper;
            clientFactory = _clientFactory;
            //notificationService = _notificationService;
            this.IntegrationTypeId = NomenclatureConstants.IntegrationTypes.Eespp;
            baseURL = configuration.GetValue<string>("EESPP:URI");
        }
        protected override async Task<bool> InitChanel()
        {
            client = clientFactory.CreateClient("eesppHttpClient");
            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return await Task.FromResult(true);
        }

        protected override async Task CloseChanel()
        {
            if (client != null)
                client.Dispose();
            client = null;
            await Task.Yield();
        }

        protected override async Task SendMQ(MQEpep mq)
        {
            switch (mq.SourceType)
            {
                case SourceTypeSelectVM.CaseLawyerHelp:
                    switch (mq.MethodName)
                    {
                        case EpepConstants.Methods.Add:
                        case EpepConstants.Methods.Restart:
                            await SendNewLawyerHelp(mq);
                            break;
                        case EpepConstants.Methods.Delete:
                            await SendDeleteLawyerHelp(mq);
                            break;
                    }
                    break;
                case SourceTypeSelectVM.CaseLawyerHelpAssignedLawyer:
                    await SendApointedLawyer(mq);
                    break;
                default:
                    break;
            }
        }

        private async Task SendNewLawyerHelp(MQEpep mq)
        {
            if (mq.MethodName == EpepConstants.Methods.Restart)
            {
                var delResult = await deleteLawyerHelp(mq.SourceId);
                if (!delResult.Result)
                {
                    SetErrorToMQ(mq, EpepConstants.IntegrationStates.DataContentError, $"Restart error: {delResult.ErrorMessage}");
                    return;
                }
            }

            var requestId = getKey(SourceTypeSelectVM.CaseLawyerHelp, mq.SourceId);
            if (!string.IsNullOrEmpty(requestId))
            {
                mq.ReturnGuidId = requestId;
                mq.ErrorDescription = $"Dublicated";
                UpdateMQ(mq, true);
                return;
            }
            var data = await initAssistanceDataModel((int)mq.SourceId);

            var response = await sendEesppData(HttpMethod.Post, "RequestLegalAssistance", data);
            if (response.Result && response.Content.Contains("ok", StringComparison.InvariantCultureIgnoreCase))
            {
                AddIntegrationKey(SourceTypeSelectVM.CaseLawyerHelp, mq.SourceId, data.RequestId);
                foreach (var client in data.Clients)
                {
                    AddIntegrationKey(SourceTypeSelectVM.CaseLawyerHelpPerson, client.CaseLawyerHelpPersonId, client.ID);
                    var person = repo.GetById<CaseLawyerHelpPerson>(client.CaseLawyerHelpPersonId);
                    person.EesppPersonStateId = NomenclatureConstants.EesppPersonState.Sent;
                }
                mq.ReturnGuidId = data.RequestId;
                UpdateMQ(mq, true);
            }
            else
            {
                SetErrorToMQ(mq, EpepConstants.IntegrationStates.DataContentError, response.ErrorMessage);
            }
        }
        private async Task SendDeleteLawyerHelp(MQEpep mq)
        {
            var delResult = await deleteLawyerHelp(mq.SourceId);
            if (delResult.Result)
            {
                UpdateMQ(mq, true);
            }
            else
            {
                SetErrorToMQ(mq, EpepConstants.IntegrationStates.DataContentError, delResult.ErrorMessage);
            }


            //var requestId = getKey(SourceTypeSelectVM.CaseLawyerHelp, mq.SourceId);
            //if (string.IsNullOrEmpty(requestId))
            //{
            //    SetErrorToMQ(mq, EpepConstants.IntegrationStates.WaitForParentIdError);
            //    return;
            //}

            //var response = await sendEesppData(HttpMethod.Post, $"StopLegalAssistance/{requestId}");
            //if (response.Result && response.Content.Contains("ok", StringComparison.InvariantCultureIgnoreCase))
            //{
            //    UpdateMQ(mq, true);
            //    return;
            //}
            //else
            //{
            //    SetErrorToMQ(mq, EpepConstants.IntegrationStates.DataContentError, response.ErrorMessage);
            //}
        }

        private async Task<SaveResultVM> deleteLawyerHelp(long lawyerHelpId)
        {
            var requestId = getKey(SourceTypeSelectVM.CaseLawyerHelp, lawyerHelpId);
            if (string.IsNullOrEmpty(requestId))
            {
                return new SaveResultVM(true);
            }

            var response = await sendEesppData(HttpMethod.Post, $"StopLegalAssistance/{requestId}");
            if (response.Result && response.Content.Contains("ok", StringComparison.InvariantCultureIgnoreCase))
            {
                RemoveIntegrationKeys(null, SourceTypeSelectVM.CaseLawyerHelp, lawyerHelpId);
                var clients = repo.AllReadonly<CaseLawyerHelpPerson>()
                                        .Where(x => x.CaseLawyerHelpId == (int)lawyerHelpId)
                                        .Select(x => x.Id)
                                        .ToList();
                foreach (var personId in clients)
                {
                    RemoveIntegrationKeys(null, SourceTypeSelectVM.CaseLawyerHelpPerson, personId);
                }

                return new SaveResultVM(true);
            }
            else
            {
                return new SaveResultVM(false, response.ErrorMessage);
            }
        }

        public async Task<bool> FetchResult(int fetchCount)
        {

            var waitingRequests = repo.AllReadonly<CaseLawyerHelp>()
                                         .Where(x => x.CaseLawyerHelpPersons
                                                    .Any(p => p.EesppPersonStateId == NomenclatureConstants.EesppPersonState.Sent && p.DateExpired == null))
                                         .Where(x => x.DateExpired == null)
                                         .OrderBy(x => x.Id)
                                         .Select(x => x.Id)
                                         .Take(fetchCount)
                                         .ToArray();


            if (!waitingRequests.Any())
            {
                return false;
            }
            if (!await InitChanel())
            {
                return false;
            }


            int savedCount = 0;
            foreach (var lawyerHelpId in waitingRequests)
            {
                var requestKey = await repo.AllReadonly<IntegrationKey>()
                                            .Where(x => x.IntegrationTypeId == IntegrationTypeId
                                            && x.SourceType == SourceTypeSelectVM.CaseLawyerHelp
                                            && x.SourceId == lawyerHelpId)
                                            .Select(x => new
                                            {
                                                x.DateWrt,
                                                x.OuterCode
                                            }).FirstOrDefaultAsync();
                if (requestKey == null)
                {
                    continue;
                }

                if (requestKey.DateWrt < DateTime.Now.AddMonths(-1).AddDays(-5))
                {
                    //Всички по-стари от 1 месец заявки се считат за отказани
                    var lawyerHelpPersonsWaiting = await repo.All<CaseLawyerHelpPerson>()
                                                            .Where(x => x.CaseLawyerHelpId == lawyerHelpId)
                                                            .Where(x => x.EesppPersonStateId == NomenclatureConstants.EesppPersonState.Sent && x.DateExpired == null)
                                                            .ToListAsync();

                    if (lawyerHelpPersonsWaiting.Any())
                    {
                        foreach (var person in lawyerHelpPersonsWaiting)
                        {
                            person.EesppPersonStateId = NomenclatureConstants.EesppPersonState.Declined;
                            person.DescriptionExpired = $"Отказано поради изтичане на едномесечен срок за отговор, {DateTime.Now: dd.MM.yyyy HH:mm}";
                        }
                        var lawyerHelp = repo.GetById<CaseLawyerHelp>(lawyerHelpId);
                        lawyerHelp.Description = (lawyerHelp.Description ?? "")
                            + Environment.NewLine
                            + "Забележка: Искането за правна помощ се счита отказано поради изтичане на едномесечен срок за отговор.";

                        await repo.SaveChangesAsync();

                        continue;
                    }
                }

                //https://lawyerswcf.nbpp.government.bg/AppointedLawyer/b7bb3984-8482-4f28-97cb-da977cd34845
                //var requestId = getKey(SourceTypeSelectVM.CaseLawyerHelp, lawyerHelpId);
                //logger.LogWarning($"Check Appoint lawyer : {requestId}");
                var response = await sendEesppData(HttpMethod.Get, $"AppointedLawyer/{requestKey.OuterCode}");
                if (response.Result)
                {
                    try
                    {
                        var appointedLawyers = JsonConvert.DeserializeObject<AppointedLawyerResponse>(response.Content);
                        if (appointedLawyers != null && appointedLawyers.Lawyers != null)
                        {
                            foreach (var appointedLawyer in appointedLawyers.Lawyers)
                            {
                                if (!repo.AllReadonly<CaseLawyerHelpAssignedLawyer>()
                                                .Where(x => x.NotificationId == appointedLawyer.NotificationID)
                                                .Any())
                                {
                                    await assingLawyerFromEespp(appointedLawyer, lawyerHelpId);
                                    savedCount++;
                                }
                            }
                        }
                        else
                        {
                            logger.LogError($"EESPP DeserializeObject Error. Content: {response.Content} ({lawyerHelpId})", new Exception("DeserializeObject Error"));
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"EESPP Error: AppointedLawyer/{requestKey.OuterCode} ({lawyerHelpId}); Message: {ex.Message}");
                    }
                }
            }

            await CloseChanel();

            return savedCount > 0;
        }

        private async Task assingLawyerFromEespp(AppointedLawyer appointedLawyer, int lawyerHelpId)
        {
            var newLawyer = new CaseLawyerHelpAssignedLawyer()
            {
                CaseLawyerHelpId = lawyerHelpId,
                DateReturned = DateTime.Now,
                LawyerNumber = appointedLawyer.LawyerID.ToString(),
                LawyerName = appointedLawyer.LawyerName,
                LawyerStateId = NomenclatureConstants.EesppLawyerState.Assigned,
                NotificationId = appointedLawyer.NotificationID
            };

            foreach (var clientCode in appointedLawyer.Clients)
            {
                var personId = (int)getSourceIdByOuterCode(SourceTypeSelectVM.CaseLawyerHelpPerson, clientCode);
                if (personId > 0)
                {
                    newLawyer.Persons.Add(new CaseLawyerHelpAssignedLawyerPerson()
                    {
                        CaseLawyerHelpPersonId = personId
                    });

                    var lhPerson = repo.GetById<CaseLawyerHelpPerson>(personId);
                    if (lhPerson != null)
                    {
                        lhPerson.EesppPersonStateId = NomenclatureConstants.EesppPersonState.Assigned;
                    }
                }
            }

            repo.Add(newLawyer);
            repo.SaveChanges();
            if (!string.IsNullOrEmpty(appointedLawyer.NotifyFile1) && !string.IsNullOrEmpty(appointedLawyer.NotifyFileName1))
            {
                await cdnService.MongoCdn_UploadFile(new IOWebApplication.Infrastructure.Models.Cdn.CdnUploadRequest()
                {
                    SourceType = SourceTypeSelectVM.CaseLawyerHelpAssignedLawyer,
                    SourceId = newLawyer.Id.ToString(),
                    FileName = appointedLawyer.NotifyFileName1,
                    FileContentBase64 = appointedLawyer.NotifyFile1
                });
            }

            var newNotification = await createNotification(newLawyer);
            if (newNotification != null)
            {
                repo.Add(newNotification);
                await repo.SaveChangesAsync();
            }
        }

        private async Task SendApointedLawyer(MQEpep mq)
        {
            var model = repo.GetById<CaseLawyerHelpAssignedLawyer>((int)mq.SourceId);
            if (model == null)
            {
                SetErrorToMQ(mq, EpepConstants.IntegrationStates.DataContentError, "Ненамерен обект - адвокат");
                return;
            }

            var actInfo = repo.AllReadonly<CaseSessionAct>()
                                    .Where(x => x.Id == model.CaseSessionActAssignedId)
                                    .Select(x => new
                                    {
                                        ActName = $"{x.ActType.Label} {x.RegNumber}/{x.RegDate:dd.MM.yyyy}"
                                    }).FirstOrDefault();



            LawyerDocument lawyerDocument = new LawyerDocument()
            {
                NotificationID = model.NotificationId,

                IsApproved = model.LawyerStateId == NomenclatureConstants.EesppLawyerState.Confirmed
            };

            if (lawyerDocument.IsApproved)
            {
                var fileModel = await cdnService.MongoCdn_Download(new CdnFileSelect() { SourceType = SourceTypeSelectVM.CaseSessionActPdf, SourceId = model.CaseSessionActAssignedId?.ToString() }, CdnFileSelect.PostProcess.Flatten);
                if (fileModel == null || string.IsNullOrEmpty(fileModel?.FileContentBase64))
                {
                    SetErrorToMQ(mq, EpepConstants.IntegrationStates.MissingObjectEISS, "Грешен или липсващ файл.");
                    return;
                }
                lawyerDocument.AppointFileName1 = fileModel.FileName;
                lawyerDocument.AppointFile1 = fileModel.FileContentBase64;
            }

            var requestId = getKey(SourceTypeSelectVM.CaseLawyerHelp, model.CaseLawyerHelpId);
            var response = await sendEesppData(HttpMethod.Post, $"AppointedLawyerDocument", lawyerDocument);
            if (response.Result && response.Content.Contains("ok", StringComparison.InvariantCultureIgnoreCase))
            {
                UpdateMQ(mq, true);
                return;
            }
            else
            {
                SetErrorToMQ(mq, EpepConstants.IntegrationStates.DataContentError, response.ErrorMessage);
            }
        }


        private async Task<AssistanceDataModel> initAssistanceDataModel(int id)
        {
            var info = await repo.AllReadonly<CaseLawyerHelp>()
                                    .Where(x => x.Id == id)
                                    .Select(x => new
                                    {
                                        x.Case.CourtId,
                                        CourtName = x.Case.Court.Label,
                                        CourtCode = x.Case.Court.Code,
                                        RequestDate = x.CaseSessionAct.ActDeclaredDate.Value,
                                        Reason = x.LawyerHelpBase.Label,
                                        x.LawyerHelpTypeId,
                                        LawServiceType = x.LawyerHelpType.Code,
                                        JudicalCompositionName = (x.Case.JudicalComposition != null) ? x.Case.JudicalComposition.Label : "",
                                        OtdelenieName = (x.Case.Otdelenie != null) ? x.Case.Otdelenie.Label : "",
                                        CaseId = x.Case.ShortNumber,
                                        CaseTypeId = x.Case.CaseTypeId,
                                        CaseYear = x.Case.RegDate.Year,
                                        CaseDescr = x.Case.CaseCode.Label,
                                        JudgeName = x.CaseSessionAct.CaseSession
                                                        .CaseLawUnits
                                                        .Where(l => l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter
                                                        && l.DateTo == null)
                                                        .Select(l => l.LawUnit.FullName).FirstOrDefault(),
                                        JudDocNo = x.CaseSessionAct.RegNumber,
                                        JudDocDate = x.CaseSessionAct.RegDate.Value,
                                        JudDocType = x.CaseSessionAct.ActType.Label,
                                        JudgementDate = (x.CaseSessionToGoId > 0) ? x.CaseSession.DateFrom : (DateTime?)null,
                                        SessionHall = (x.CaseSessionToGoId > 0 && x.CaseSession.CourtHallId > 0) ? x.CaseSession.CourtHall.Name : "",
                                        ConflictInterests = x.HasInterestConflict,
                                        ОppositeLawyers = x.CaseLawyerHelpOtherLawyers.Select(l => l.CasePerson.FullName).ToArray(),
                                        PrevLawyers = x.PrevDefenderName,
                                        ActTypeName = x.CaseSessionAct.ActType.Label,
                                        ActNumber = x.CaseSessionAct.RegNumber,
                                        ActDate = x.CaseSessionAct.ActDeclaredDate,
                                        ActId = x.CaseSessionActId
                                    })
                                    .FirstOrDefaultAsync();

            var docInfo = await repo.AllReadonly<DocumentTemplate>()
                                        .Where(x => x.SourceType == SourceTypeSelectVM.CaseLawyerHelp && x.SourceId == id)
                                        .Where(x => x.DateExpired == null)
                                        .Where(x => x.DocumentId > 0 && x.Document.DateExpired == null)
                                        .Select(x => new
                                        {
                                            DocumentId = x.DocumentId.Value,
                                            x.Document.DocumentNumber,
                                            x.Document.DocumentDate,
                                            x.SignerId,
                                            x.AuthorId
                                        }).FirstOrDefaultAsync();

            var authorId = docInfo.SignerId ?? docInfo.AuthorId;
            var authorInfo = repo.AllReadonly<ApplicationUser>()
                                        .Where(x => x.Id == authorId)
                                        .Select(x => new
                                        {
                                            fullName = x.LawUnit.FullName,
                                            typeName = x.LawUnit.LawUnitType.Label
                                        }).FirstOrDefault();

            var clients = await repo.AllReadonly<CaseLawyerHelpPerson>()
                                    .Where(x => x.CaseLawyerHelpId == id)
                                    .Select(x => new ClientModel
                                    {
                                        CaseLawyerHelpPersonId = x.Id,
                                        Name = x.CasePerson.FullName,
                                        EIN = (x.CasePerson.UicTypeId == NomenclatureConstants.UicTypes.EGN) ? (x.CasePerson.Uic ?? "9999999999") : "9999999999",
                                        Address = x.CasePerson.Addresses.Where(a => a.Address.AddressTypeId == NomenclatureConstants.AddressType.Current && a.DateExpired == null)
                                                    .Select(a => a.Address.FullAddress)
                                                    .FirstOrDefault(),
                                        SystemNumber = x.CasePerson.PersonRoleId.ToString()
                                    }).ToListAsync();

            foreach (var client in clients)
            {
                client.Name = client.Name.Decode();
                client.ID = Guid.NewGuid().ToString();
                if (string.IsNullOrEmpty(client.Address))
                {
                    //ако Няма настоящ адрес, взема постоянния, ако не - България
                    var permanentAddress = repo.AllReadonly<CaseLawyerHelpPerson>()
                                                        .Where(x => x.Id == client.CaseLawyerHelpPersonId)
                                                        .Select(x => x.CasePerson.Addresses.Where(a => a.Address.AddressTypeId == NomenclatureConstants.AddressType.Permanent && a.DateExpired == null)
                                                            .Select(a => a.Address.FullAddress)
                                                            .FirstOrDefault())
                                                        .FirstOrDefault();

                    if (!string.IsNullOrEmpty(permanentAddress))
                    {
                        client.Address = permanentAddress;
                    }
                    else
                    {
                        client.Address = "България";
                    }
                }
            }

            var result = new AssistanceDataModel()
            {
                RequestId = Guid.NewGuid().ToString(),// AppendUpdateIntegrationKey(SourceTypeSelectVM.CaseLawyerHelp, id),
                LawyersAssociation = GetNomValue("lawyerhelp_colegia", info.CourtId),
                RequestDate = info.RequestDate,
                DocNo = docInfo.DocumentNumber,
                DocDate = docInfo.DocumentDate,
                Reason = info.Reason,
                LawServiceType = info.LawServiceType,
                JusticeCourtId = info.CourtCode,
                //JusticeCourtId = GetNomValue("lawyerhelp_court", info.CourtId),
                JusticeCourtDescr = $"{info.OtdelenieName},{info.JudicalCompositionName}",
                //CaseType = info.CaseTypeId.ToString(),
                CaseType = GetNomValue("epep_casetype", info.CaseTypeId),
                CaseID = info.CaseId,
                CaseYear = info.CaseYear,
                CaseDescr = info.CaseDescr,
                JudgeName = info.JudgeName,
                JudDocType = info.JudDocType,
                JudDocNo = info.JudDocNo,
                JudDocDate = info.JudDocDate,
                //JudgementDate = info.JudgementDate,
                //JudgementAddress = info.CourtName,
                ConflictInterests = (info.ConflictInterests) ? 1 : 0,
                ОppositeLawyers = string.Join(',', info.ОppositeLawyers),
                PrevLawyers = info.PrevLawyers,
                AgentName = authorInfo.fullName,
                AgentType = authorInfo.typeName,
                Clients = clients.ToArray()
            };
            if (info.JudgementDate.HasValue)
            {
                result.JudgementDate = info.JudgementDate;
                result.JudgementAddress = info.CourtName;
                if (!string.IsNullOrWhiteSpace(info.SessionHall))
                {
                    result.JudgementAddress += $" ,{info.SessionHall}";
                }
            }

            var actFileModel = await cdnService.MongoCdn_Download(new CdnFileSelect() { SourceType = SourceTypeSelectVM.CaseSessionActPdf, SourceId = info.ActId.ToString() }, CdnFileSelect.PostProcess.Flatten);
            if (actFileModel != null)
            {
                result.FileData1 = actFileModel.FileContentBase64;
                result.FileName1 = actFileModel.FileName;
            }
            var docFileModel = await cdnService.MongoCdn_Download(new CdnFileSelect() { SourceType = SourceTypeSelectVM.DocumentPdf, SourceId = docInfo.DocumentId.ToString() }, CdnFileSelect.PostProcess.Flatten);
            if (docFileModel != null)
            {
                result.FileData2 = docFileModel.FileContentBase64;
                result.FileName2 = docFileModel.FileName;
            }
            result.Sanitize();
            return result;
        }

        private async Task<SaveResultVM> sendEesppData(HttpMethod httpMethod, string methodName, object data = null)
        {
            var uri = new Uri(new Uri(baseURL), methodName);

            var request = new HttpRequestMessage(httpMethod, uri);

            if (data != null)
            {
                var jsonData = JsonConvert.SerializeObject(data);
                request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            }


            try
            {
                var response = await client.SendAsync(request);
                if (response != null)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = new SaveResultVM();
                    if (response.IsSuccessStatusCode)
                    {
                        result.Result = true;
                        result.Content = responseContent;
                    }
                    else
                    {
                        result.Result = false;
                        var errorModel = JsonConvert.DeserializeObject<Models.EESPP.ErrorModel>(responseContent);
                        result.ErrorMessage = $"Грешка {errorModel.ErrorCode}, {errorModel.ErrorDetails}";
                    }
                    return result;
                }
                else
                {
                    return new SaveResultVM(false, $"Грешка при изпращане на заявка. Метод {methodName}");
                }
            }
            catch (Exception ex)
            {
                return new SaveResultVM(false, $"Exception при изпращане на заявка. Метод {methodName}, {ex.Message}");
            }


            ///RequestLegalAssistance
            //var result = GetHttpResponseAsync(HttpMethod.Post, "https://212.122.188.141:4443/DOSLawyersWCF/RequestLegalAssistance", data).Result;
            ///AppointedLawyer/{ RequestId }
            //string RequestId = "10099";
            //var result = GetHttpResponseAsync(HttpMethod.Get, $"https://212.122.188.141:4443/DOSLawyersWCF/AppointedLawyer/{RequestId}").Result;
            ///AppointedLawyerDocument
            //var result = GetHttpResponseAsync(HttpMethod.Post, "https://212.122.188.141:4443/DOSLawyersWCF/AppointedLawyerDocument", data).Result;
            ///StopLegalAssistance/{ RequestId }
            //var result = GetHttpResponseAsync(HttpMethod.Post, $"https://212.122.188.141:4443/DOSLawyersWCF/StopLegalAssistance/{RequestId}").Result;
        }


        private async Task<WorkNotification> createNotification(CaseLawyerHelpAssignedLawyer model)
        {

            var judgeReporter = await repo.AllReadonly<CaseLawyerHelp>()
                                .Include(x => x.Case)
                                .ThenInclude(x => x.CaseLawUnits)
                                .ThenInclude(x => x.LawUnit)
                                .Where(x => x.Id == model.CaseLawyerHelpId)
                                .SelectMany(x => x.Case.CaseLawUnits)
                                .Where(x => x.DateTo == null && x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                .AsSplitQuery()
                                .Select(x => new
                                {
                                    x.Id,
                                    x.LawUnitUserId,
                                    x.LawUnit.FullName
                                })
                                .FirstOrDefaultAsync();

            if (judgeReporter == null)
            {
                return null;
            }

            var caseInfo = await repo.AllReadonly<CaseLawyerHelp>()
                                .Include(x => x.Case)
                                .ThenInclude(x => x.CaseType)
                                .Where(x => x.Id == model.CaseLawyerHelpId)
                                .Select(x => new
                                {
                                    CourtId = x.Case.CourtId,
                                    CaseType = x.Case.CaseType.Code,
                                    CaseNumber = x.Case.ShortNumber,
                                    CaseYear = x.Case.RegDate.Year
                                }).FirstOrDefaultAsync();

            if (caseInfo == null)
            {
                return null;
            }
            var workNotification = new WorkNotification();
            workNotification.SourceType = SourceTypeSelectVM.CaseLawyerHelp;
            workNotification.SourceId = model.CaseLawyerHelpId;
            workNotification.WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.CaseLawyerHelpAssigned;
            workNotification.Title = $"Определен служебен защитник";
            workNotification.Description = $"Има определен служебен защитник по искане за правна помощ към дело {caseInfo.CaseType} {caseInfo.CaseNumber}/{caseInfo.CaseYear}.";
            workNotification.LinkLabel = "Искане НБПП";
            workNotification.CourtId = caseInfo.CourtId;
            workNotification.FromCourtId = caseInfo.CourtId;
            workNotification.DateCreated = model.DateReturned;
            workNotification.UserId = judgeReporter.LawUnitUserId;
            return workNotification;
        }
    }
}




