using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Http;
using IOWebApplication.Infrastructure.Models.Integrations.Ispn;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplicationService.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using static IOWebApplication.Infrastructure.Constants.EpepConstants;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using CaseType = IOWebApplication.Infrastructure.Models.Integrations.Ispn.CaseType;
using SessionType = IOWebApplication.Infrastructure.Models.Integrations.Ispn.SessionType;
using ActType = IOWebApplication.Infrastructure.Models.Integrations.Ispn.ActType;
using System.Transactions;

namespace IOWebApplicationService.Infrastructure.Services
{
    public class ISPNCaseService : BaseMQService, IISPNCaseService
    {
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory clientFactory;
        public List<MQEpep> caseMq = null;
        private Uri uploadUrl;
        private HttpRequester requester;
        public ISPNCaseService(
            IRepository _repo,
            ICdnService _cdnService,
            ILogger<ISPNCaseService> _logger,
            IHttpClientFactory _clientFactory,
            IConfiguration _configuration)
        {
            repo = _repo;
            cdnService = _cdnService;
            this.IntegrationTypeId = NomenclatureConstants.IntegrationTypes.ISPN;
            logger = _logger;
            configuration = _configuration;
            this.mqID = null;
            clientFactory = _clientFactory;
        }


        public int ServiceMethodToAction(ServiceMethod serviceMethod)
        {
            switch (serviceMethod)
            {
                case ServiceMethod.Add:
                    return 13001;
                case ServiceMethod.Update:
                    return 13002;
                case ServiceMethod.Delete:
                    return 13003;
                default:
                    return 13004;
            }
        }
        private SideType FillSide(PersonNamesBase person, long id, int roleId, int sourceTypID)
        {
            (var newId, var action) = AppendUpdateIntegrationKeyAction(sourceTypID, id, false);

            SideType result = new SideType();
            result.side_id = newId;
            result.side_involvement = GetNomValueInt(EpepConstants.Nomenclatures.PersonRoles, roleId);
            if (person.IsPerson)
            {
                SideTypeSide_citizen sidePerson = new SideTypeSide_citizen();
                sidePerson.side_name_1 = person.FirstName;
                sidePerson.side_rename = person.MiddleName;
                sidePerson.side_family_1 = person.FamilyName;
                sidePerson.side_family_2 = person.Family2Name;
                sidePerson.side_egn = person.Uic;
                result.Item = sidePerson;
                //TODO: Да се махне
                //if (string.IsNullOrEmpty(sidePerson.side_family_1))
                //    sidePerson.side_family_1 = "ФАМИЛИЯ"; 
            }
            else
            {
                SideTypeSide_firm sidePerson = new SideTypeSide_firm();
                sidePerson.side_name = person.FullName;
                sidePerson.side_bulstat = person.Uic;
                result.Item = sidePerson;
            }
            return result;
        }

        private SideType[] FillSides(ICollection<CasePerson> casePersonsAll)
        {
            var casePersons = casePersonsAll
              .Where(x => x.DateExpired == null)
              .Where(x => x.CaseSessionId == null)
              .ToList();
            var sides = new List<SideType>();
            for (int i = 0; i < casePersons.Count; i++)
            {
                var itemPerson = casePersons[i];
                var side = FillSide(itemPerson, itemPerson.Id, itemPerson.PersonRoleId, SourceTypeSelectVM.CasePerson);
                if (side != null)
                    sides.Add(side);
            }
            return sides.ToArray();
        }
        private CaseType FillCase(Case caseAll, List<Document> compliantDocuments, List<CaseMigration> migrations)
        {
            var judgeReporter = caseAll.CaseLawUnits
                  .Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                  .Where(x => x.CaseSessionId == null)
                  .OrderByDescending(x => x.DateTo ?? DateTime.Now.AddYears(100))
                  .FirstOrDefault();

            (var newId, var action) = AppendUpdateIntegrationKeyAction(SourceTypeSelectVM.Case, caseAll.Id, false);
            (var newDocumentId, var actionDocument) = AppendUpdateIntegrationKeyAction(SourceTypeSelectVM.Document, caseAll.DocumentId, false);

            var ispnCase = new CaseType()
            {
                case_id = newId,
                case_action = ServiceMethodToAction(action),
                case_court = GetNomValueInt(EpepConstants.Nomenclatures.Courts, caseAll.CourtId),
                case_kind = GetNomValueInt(EpepConstants.Nomenclatures.CaseTypes, caseAll.CaseTypeId),
                case_no = caseAll.ShortNumberValue ?? 0,
                case_year = caseAll.RegDate.Year.ToString(),
                case_date = caseAll.RegDate,
                InDoc = new InDocType
                {
                    indoc_id = newDocumentId,
                    indoc_kind = GetNomValueInt(EpepConstants.Nomenclatures.IncommingDocumentTypes, caseAll.Document.DocumentTypeId),
                    indoc_no = caseAll.Document.DocumentNumberValue ?? 0,
                    indoc_noSpecified = (caseAll.Document.DocumentNumberValue > 0),
                    indoc_year = caseAll.Document.DocumentDate.Year,
                    indoc_date = caseAll.Document.DocumentDate,
                },
                Side = FillSides(caseAll.CasePersons),
                Session = FillSessionTypeArr(caseAll.CaseSessions, caseAll.CaseLawUnits, migrations),
                Judge = FillJudgeType(judgeReporter),
                Surround = FillSurroundArray(compliantDocuments)
            };
            return ispnCase;
        }
        public CaseType FillCaseAll(MQEpep model)
        {
            var caseAll = repo.AllReadonly<Case>()
                              .Include(x => x.Document)
                              .Include(x => x.CaseSessions)
                              .ThenInclude(x => x.CaseSessionActs)
                              .ThenInclude(x => x.CaseSessionActComplains)
                              .ThenInclude(x => x.ComplainDocument)
                              .ThenInclude(x => x.DocumentPersons)
                              .Include(x => x.CaseSessions)
                              .ThenInclude(x => x.CaseSessionActs)
                              .ThenInclude(x => x.ActComplainIndex)
                              .Include(x => x.CaseSessions)
                              .ThenInclude(x => x.CaseSessionResults)
                              .Include(x => x.CasePersons)
                              .Include(x => x.CaseLawUnits)
                              .ThenInclude(x => x.LawUnit)
                              .Where(x => x.Id == model.ParentSourceId)
                              .FirstOrDefault();

            var compliantDocuments = repo.AllReadonly<Document>()
                                .Include(x => x.DocumentPersons)
                                .Where(x => x.DocumentCaseInfo.Where(a => a.CaseId == caseAll.Id).Any())
                                .Where(x => x.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.CompliantDocument &&
                                            x.DateExpired == null)
                                .ToList();

            var migrations = repo.AllReadonly<CaseMigration>()
                             .Include(x => x.OutDocument)
                             .Where(x => x.CaseId == caseAll.Id)
                             .Where(x => x.CaseSessionActId != null)
                             .Where(x => x.OutDocumentId != null &&
                                         x.DateExpired == null)
                             .ToList();

            return FillCase(caseAll, compliantDocuments, migrations);
        }

        private ActType FillActType(CaseSessionAct sessionAct, CaseSession session, List<CaseMigration> migrations)
        {
            (var newId, var action) = AppendUpdateIntegrationKeyAction(SourceTypeSelectVM.CaseSessionAct, sessionAct.Id, sessionAct.DateExpired != null);
            if (string.IsNullOrEmpty(newId))
                return null;

            /*  CdnDownloadResult actFile = cdnService.MongoCdn_Download(new CdnFileSelect()
             {
                 SourceType = SourceTypeSelectVM.CaseSessionActDepersonalized,
                 SourceId = sessionAct.Id.ToString()
             }).Result; */
            int[] reason = sessionAct.ActISPNDebtorStateId > 0 ?
                              new int[1] { sessionAct.ActISPNDebtorStateId ?? 0 } :
                              new int[0];
            return new ActType()
            {
                act_action = ServiceMethodToAction(action),
                act_date = (session.DateTo ?? session.DateFrom).Date,
                act_id = newId,
                act_kind = GetNomValueInt(EpepConstants.Nomenclatures.ActTypes, sessionAct.ActTypeId),
                act_no = int.Parse(sessionAct.RegNumber),
                act_text = sessionAct.Description,
                act_debtor_status = sessionAct.ActISPNDebtorStateId ?? 0,
                act_reason = reason,
                Appeal = FillAppealArray(sessionAct.CaseSessionActComplains.ToList(),
                                       migrations.Where(x => x.CaseSessionActId == sessionAct.Id).ToList(), sessionAct),
                //act_image = Convert.FromBase64String(actFile.FileContentBase64)
            };
        }
        private SessionType FillSessionType(CaseSession session, CaseSessionResult sessionResult, ICollection<CaseLawUnit> caseLawUnitsAll, List<CaseMigration> migrations)
        {
            var caseLawUnits = caseLawUnitsAll.Where(x => (x.DateTo ?? session.DateFrom) >= session.DateFrom)
                                              .Where(x => x.CaseSessionId == session.Id)
                                              .Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                              .ToList();
            var acts = session.CaseSessionActs
                              .Where(x => x.DateExpired == null &&
                                          x.ActDeclaredDate != null &&
                                          x.ActTypeId != NomenclatureConstants.ActType.ExecListPrivatePerson &&
                                          x.ActTypeId != NomenclatureConstants.ActType.ObezpechitelnaZapoved
                                          )
                              .Select(x => FillActType(x, session, migrations)).ToArray();
            //acts = acts.Where(x => x.act_reason.Length > 0 || x.act_debtor_status > 0 || !string.IsNullOrEmpty(x.act_text)).ToArray();
            (var newId, var action) = AppendUpdateIntegrationKeyAction(SourceTypeSelectVM.CaseSession, session.Id, session.DateExpired != null);
            if (string.IsNullOrEmpty(newId))
                return null;
            int sessionResultIspn = 11004;
            try
            {
                sessionResultIspn = GetNomValueInt(EpepConstants.Nomenclatures.SessionResults, sessionResult.Id); /* nom_session_result */
            }
            catch
            {

            }
            return new SessionType()
            {
                session_action = ServiceMethodToAction(action),
                Act = acts,
                session_date = (session.DateTo ?? session.DateFrom).Date,
                session_id = newId,
                session_kind = GetNomValueInt(EpepConstants.Nomenclatures.SessionTypes, session.SessionTypeId),
                session_text = session.Description ?? "",
                session_time = session.DateTo ?? session.DateFrom,
                session_result = sessionResultIspn,
                Surround = FillSurroundProcessedArray(session.CaseSessionDocs.ToList()),
                Judge = FillJudgeTypeArray(caseLawUnits),
                //extensions =
            };
        }
        private SessionType[] FillSessionTypeArr(ICollection<CaseSession> sessions, ICollection<CaseLawUnit> caseLawUnits, List<CaseMigration> migrations)
        {
            var sessionList = new List<SessionType>();
            foreach (var session in sessions.Where(x => x.DateExpired == null))
            {
                var sessionResult = session.CaseSessionResults.FirstOrDefault(x => x.DateExpired == null && x.IsMain);
                if (sessionResult != null)
                    sessionList.Add(FillSessionType(session, sessionResult, caseLawUnits, migrations));
            }
            return sessionList.ToArray();
        }

        private JudgeType FillJudgeType(CaseLawUnit caseLawUnit)
        {
            JudgeType result = null;
            if (caseLawUnit != null)
            {
                (var newId, var action) = AppendUpdateIntegrationKeyAction(SourceTypeSelectVM.CaseLawUnit, caseLawUnit.Id, false);
                result = new JudgeType()
                {
                    judge_id = newId,
                    judge_name_1 = caseLawUnit.LawUnit.FirstName,
                    judge_rename = caseLawUnit.LawUnit.MiddleName,
                    judge_family_1 = caseLawUnit.LawUnit.FamilyName,
                    judge_family_2 = caseLawUnit.LawUnit.Family2Name,
                    judge_egn = caseLawUnit.LawUnit.Uic
                };
            }

            return result;
        }

        private JudgeType[] FillJudgeTypeArray(List<CaseLawUnit> caseLawUnits)
        {
            JudgeType[] judges = new JudgeType[caseLawUnits.Count];
            for (int i = 0; i < caseLawUnits.Count; i++)
            {
                judges[i] = FillJudgeType(caseLawUnits[i]);
            }
            return judges;
        }
        private SurroundType FillSurround(Document compliantDocument)
        {
            (var newId, var action) = AppendUpdateIntegrationKeyAction(SourceTypeSelectVM.Document, compliantDocument.Id, compliantDocument.DateExpired != null);
            if (string.IsNullOrEmpty(newId))
                return null;

            SurroundType result = new SurroundType();
            result.surround_id = newId;
            result.surround_action = ServiceMethodToAction(action);
            result.surround_no = compliantDocument.DocumentNumberValue ?? 0;
            result.surround_year = compliantDocument.DocumentDate.Year.ToString();
            result.surround_date = compliantDocument.DocumentDate;
            result.surround_text = compliantDocument.Description;
            result.Side = FillDocumentSides(compliantDocument.DocumentPersons.ToList());
            if (string.IsNullOrEmpty(result.surround_text))
            {
                var docType = repo.AllReadonly<DocumentType>().FirstOrDefault(x => x.Id == compliantDocument.DocumentTypeId);
                result.surround_text = docType?.Label ?? "";
            }
            return result;
        }
        private SurroundType[] FillSurroundArray(List<Document> compliantDocuments)
        {
            List<SurroundType> surrounds = new List<SurroundType>();
            for (int i = 0; i < compliantDocuments.Count; i++)
            {
                var document = FillSurround(compliantDocuments[i]);
                if (document != null)
                    surrounds.Add(document);
            }
            return surrounds.ToArray();
        }

        private SideType[] FillDocumentSides(List<DocumentPerson> documentPersons)
        {
            var sides = new List<SideType>();
            for (int i = 0; i < documentPersons.Count; i++)
            {
                var itemPerson = documentPersons[i];
                var side = FillSide(itemPerson, itemPerson.Id, itemPerson.PersonRoleId, SourceTypeSelectVM.DocumentPerson);
                if (side != null)
                    sides.Add(side);
            }
            return sides.ToArray();

        }

        private SurroundProcessedType FillSurroundProcessed(CaseSessionDoc sessionDoc)
        {
            (var newId, var action) = AppendUpdateIntegrationKeyAction(SourceTypeSelectVM.CaseSessionActDoc, sessionDoc.Id, false);

            SurroundProcessedType result = new SurroundProcessedType();
            result.surround_id = newId;
            result.surround_no = sessionDoc.Document.DocumentNumberValue ?? 0;
            result.surround_year = sessionDoc.Document.DocumentDate.Year.ToString();

            return result;
        }
        private SurroundProcessedType[] FillSurroundProcessedArray(List<CaseSessionDoc> sessionDocs)
        {
            SurroundProcessedType[] surrounds = new SurroundProcessedType[sessionDocs.Count];
            for (int i = 0; i < sessionDocs.Count; i++)
            {
                surrounds[i] = FillSurroundProcessed(sessionDocs[i]);
            }
            return surrounds;
        }

        private AppealType FillAppeal(CaseSessionActComplain actComplain)
        {
            (var newId, var action) = AppendUpdateIntegrationKeyAction(SourceTypeSelectVM.CaseSessionActComplain, actComplain.Id, false);

            AppealType result = new AppealType();
            result.appeal_id = newId;
            result.appeal_action = ServiceMethodToAction(action);
            result.appeal_kind = GetNomValueInt(EpepConstants.Nomenclatures.SessionActAppealDocType, actComplain.ComplainDocument.DocumentTypeId);
            result.appeal_no = actComplain.ComplainDocument.DocumentNumberValue ?? 0;
            result.appeal_year = actComplain.ComplainDocument.DocumentDate.Year.ToString();
            result.appeal_date = actComplain.ComplainDocument.DocumentDate;
            result.Side = FillDocumentSides(actComplain.ComplainDocument.DocumentPersons.ToList());

            return result;
        }

        private AppealType[] FillAppealArray(List<CaseSessionActComplain> actComplains, List<CaseMigration> migrations, CaseSessionAct sessionAct)
        {
            AppealType[] appeals = new AppealType[actComplains.Count];
            for (int i = 0; i < actComplains.Count; i++)
            {
                appeals[i] = FillAppeal(actComplains[i]);

                //Тъй като имаме изпращане на друг съд на ниво акт, а не на ниво жалене на акт - закачам само на първият входящ документ с който се жали
                if (i == 0)
                    appeals[i].SendTo = FillSendToArray(migrations, sessionAct);
            }
            return appeals;
        }

        private SendToType FillSendTo(CaseMigration migration, CaseSessionAct sessionAct)
        {
            (var newId, var action) = AppendUpdateIntegrationKeyAction(SourceTypeSelectVM.CaseMigration, migration.Id, migration.DateExpired != null);
            if (string.IsNullOrEmpty(newId))
                return null;

            SendToType result = new SendToType();
            result.sendto_id = newId;
            result.sendto_action = ServiceMethodToAction(action);
            result.sendto_kind = GetNomValueInt(EpepConstants.Nomenclatures.OutgoingDocumentTypes, migration.OutDocument.DocumentTypeId);
            result.sendto_no = migration.OutDocument.DocumentNumberValue ?? 0;
            result.sendto_year = migration.OutDocument.DocumentDate.Year.ToString();
            result.sendto_date = migration.OutDocument.DocumentDate;
            result.sendto_court = GetNomValueInt(EpepConstants.Nomenclatures.Courts, migration.SendToCourtId);
            result.@return = new ReturnType();
            result.@return.return_result = sessionAct.ActComplainIndex?.Code ?? "";

            return result;
        }

        private SendToType[] FillSendToArray(List<CaseMigration> migrations, CaseSessionAct sessionAct)
        {
            List<SendToType> appeals = new List<SendToType>();
            for (int i = 0; i < migrations.Count; i++)
            {
                SendToType itemAdd = FillSendTo(migrations[i], sessionAct);
                if (itemAdd != null)
                    appeals.Add(itemAdd);
            }
            return appeals.ToArray();
        }
        protected override async Task<bool> InitChanel()
        {
            uploadUrl = new Uri(configuration.GetValue<string>("ISPN:URI"));
            requester = new HttpRequester(clientFactory.CreateClient("ispnHttpClient"));
            return true;
        }

        protected override async Task CloseChanel()
        {
        }
        private string GenerateXml(MQEpep mq)
        {
            string text = string.Empty;

            var resultCase = FillCaseAll(mq);

            Transfer result = new Transfer();
            result.program = "EISS";// "ЕИСС";
            result.version = 1.8088;
            result.Case = resultCase;
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            XmlSerializer x = new XmlSerializer(typeof(Transfer));
            using (var stream = new MemoryStream())
            {
                using (TextWriter writer = new StreamWriter(stream))
                {
                    x.Serialize(writer, result, ns);
                    stream.Position = 0;
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        text = reader.ReadToEnd();
                    }
                }
            }
            return text;
        }
        protected override async Task SendMQ(MQEpep mq)
        {
            string text = mq.Content == null ? "" : Encoding.UTF8.GetString(mq.Content);
            if (string.IsNullOrEmpty(text))
            {

                text = GenerateXml(mq);
                mq.Content = Encoding.UTF8.GetBytes(text);
                repo.Update(mq);
                repo.SaveChanges();

            }

            var response = await requester.PostAsyncTextXml(uploadUrl.AbsoluteUri, text);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                XElement result = null;
                if (!string.IsNullOrEmpty(content))
                {
                    XDocument message = XDocument.Parse(content);
                    result = message?.Element("response")?.Element("success");
                }
                else
                {
                    content = "Получен е празен резултат от ИСПН";
                }
                if (result != null)
                {
                    mq.IntegrationStateId = IntegrationStates.TransferOK;
                    mq.DateTransfered = DateTime.Now;
                    mq.Content = Encoding.UTF8.GetBytes(text);
                    repo.Update(mq);
                    repo.SaveChanges();
                    if (caseMq != null)
                    {
                        var mqHash = caseMq.FirstOrDefault(x => x.Id == mq.Id);
                        mqHash.Content = mq.Content;
                        mqHash.IntegrationStateId = mq.IntegrationStateId;
                    }
                    string fileName = $"Ispn_{mq.Id}_{ DateTime.Today:dd.MM.yyyy}.xml";
                    CdnUploadRequest request = new CdnUploadRequest()
                    {
                        ContentType = System.Net.Mime.MediaTypeNames.Text.Html,
                        FileContentBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(content)),
                        FileName = fileName,
                        SourceId = mq.ParentSourceId.ToString(),
                        SourceType = SourceTypeSelectVM.Integration_ISPN,
                        Title = $"Отговор към { DateTime.Today:dd.MM.yyyy}"
                    };

                    if (!(await cdnService.MongoCdn_AppendUpdate(request)))
                    {
                        logger.LogError("Error in SaveContent ISPN mqId {mq.Id}!", mq.Id.ToString());
                    }
                }
                else
                {
                    SetErrorToMQ(mq, IntegrationStates.DataContentError, content);
                }
            }
            else
            {
                throw new Exception($"Error {response.StatusCode} : {response.ReasonPhrase}  sending message ISPN!");
            }
        }


        protected (string, ServiceMethod) AppendUpdateIntegrationKeyAction(int sourceType, long sourceId, bool isDeleted)
        {
            string key = getKey(sourceType, sourceId);
            if (!string.IsNullOrEmpty(key))
            {
                key = key.Replace("-", "").ToUpper();
                if (caseMq != null)
                {
                    bool isExists = caseMq.Any(x => x.Id < mqID && x.Content != null && Encoding.UTF8.GetString(x.Content).Contains(key));
                    if (!isExists)
                        return (key, ServiceMethod.Add);
                }
                return (key, isDeleted ? ServiceMethod.Delete : ServiceMethod.Update);
            }
            if (!isDeleted && string.IsNullOrEmpty(key))
            {
                key = Guid.NewGuid().ToString();
                if (AddIntegrationKey(sourceType, sourceId, key))
                {
                    key = key.Replace("-", "").ToUpper();
                    return (key, ServiceMethod.Add);
                }
                return (null, ServiceMethod.Delete);
            }
            return ("", ServiceMethod.Delete);
        }

        public void SetMqIdForTest(int mqId)
        {
            this.mqID = mqId;
        }
        /// <summary>
        /// The метода за изпращане на чакащите заявки към ЕПЕП
        /// </summary>
        public async Task<bool> PatchISPN(int caseId)
        {
            if (!(await InitChanel()))
            {
                return false;
            }
            caseMq = repo.AllReadonly<MQEpep>()
                         .Where(x => x.ParentSourceId == caseId && x.IntegrationTypeId == this.IntegrationTypeId)
                         .OrderBy(x => x.Id)
                         .ToList();
            foreach (var mq in caseMq)
            {
                if (mq.IntegrationStateId == IntegrationStates.TransferOK)
                    continue;
                if (caseMq.Any(x => x.Id > mq.Id && x.IntegrationStateId == IntegrationStates.TransferOK))
                    continue;
                try
                {
                    await SendMQ(mq);
                }
                catch (FaultException fex)
                {
                    var _error = fex.Message;
                    SetErrorToMQ(mq, IntegrationStates.DataContentError, _error);
                }
                catch (Exception ex)
                {
                    if (logger != null)
                    {
                        logger.LogError(ex, ex.Message);
                    }
                    SetErrorToMQ(mq, IntegrationStates.TransferError, $"Exception: {ex.Message}");
                }
            };
            await CloseChanel();

            return true;
        }
        public List<int> GetCaseIdSurroundSideErr()
        {
            return repo.AllReadonly<MQEpep>()
                          .Where(x => x.IntegrationStateId != IntegrationStates.TransferOK &&
                                      x.IntegrationTypeId == this.IntegrationTypeId &&
                                      x.ErrorDescription.Contains("encoding="))
                          //   x.ErrorDescription.Contains("{surround_text}"))
                          .GroupBy(x => x.ParentSourceId)
                          .Select(g => (int)g.FirstOrDefault().ParentSourceId)
                          .OrderBy(x => x)
                          .ToList();
        }
    }

}

