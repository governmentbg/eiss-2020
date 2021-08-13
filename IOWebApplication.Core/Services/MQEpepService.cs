using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Epep;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using static IOWebApplication.Infrastructure.Constants.EpepConstants;
using static IOWebApplication.Infrastructure.Constants.NomenclatureConstants;

namespace IOWebApplication.Core.Services
{
    public class MQEpepService : BaseIntegrationService, IMQEpepService
    {
        private readonly ICdnService cdnService;
        private readonly IConfiguration configuration;
        //private readonly ICaseLifecycleService lifecycleService;
        public string mqID = null;

        public MQEpepService(
            ILogger<DocumentService> _logger,
            //ICaseLifecycleService _lifecycleService,
            ICdnService _cdnService,
            IConfiguration _configuration,
            IRepository _repo,
            IUserContext _userContext
        )
        {
            logger = _logger;
            repo = _repo;
            //lifecycleService = _lifecycleService;
            userContext = _userContext;
            cdnService = _cdnService;
            configuration = _configuration;
        }

        public int ReturnAllErrorsInMQ()
        {
            var model = repo.All<MQEpep>()
                                .Where(x => x.ErrorCount >= IntegrationMaxErrorCount && x.IntegrationStateId == IntegrationStates.TransferErrorLimitExceeded)
                                .ToList();

            if (model.Any())
            {
                foreach (var item in model)
                {
                    item.ErrorCount = 0;
                    item.IntegrationStateId = IntegrationStates.New;
                    repo.Update(item);
                }
                repo.SaveChanges();
            }
            return model.Count;
        }


        #region Общи методи за създаване на заявките за изпращане към ЕПЕП

        private void initFromEpepModel(object epepModel, int sourceType, long sourceId, EpepConstants.ServiceMethod method, long? parentSourceId = null)
        {
            var className = epepModel.GetType().Name;
            var mq = new MQEpep()
            {
                MQId = mqID ?? Guid.NewGuid().ToString(),
                IntegrationTypeId = NomenclatureConstants.IntegrationTypes.EPEP,
                SourceType = sourceType,
                SourceId = sourceId,
                ParentSourceId = parentSourceId,
                TargetClassName = className,
                Content = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(epepModel)),
                DateWrt = DateTime.Now,
                UserId = userContext.UserId,
                MethodName = EpepConstants.Methods.GetMethod(method),
                ErrorCount = 0,
                IntegrationStateId = IntegrationStates.New
            };

            repo.Add(mq);
            repo.SaveChanges();
        }
        public void InitMQ(int integrationTypeId, int sourceType, long sourceId, EpepConstants.ServiceMethod method, long? parentSourceId = null, object model = null)
        {
            string message = null;
            if (model != null)
            {
                message = JsonConvert.SerializeObject(model);
            }
            InitMQFromString(integrationTypeId, sourceType, sourceId, method, parentSourceId, message);
        }
        public long InitMQFromString(int integrationTypeId, int sourceType, long sourceId, ServiceMethod method, long? parentSourceId, string message)
        {
            var mq = new MQEpep()
            {
                MQId = mqID ?? Guid.NewGuid().ToString(),
                IntegrationTypeId = integrationTypeId,
                SourceType = sourceType,
                SourceId = sourceId,
                ParentSourceId = parentSourceId,
                DateWrt = DateTime.Now,
                UserId = userContext.UserId,
                MethodName = EpepConstants.Methods.GetMethod(method),
                ErrorCount = 0,
                IntegrationStateId = IntegrationStates.New
            };
            if (message != null)
            {
                mq.Content = System.Text.Encoding.UTF8.GetBytes(message);
            }
            repo.Add(mq);
            repo.SaveChanges();
            return mq.Id;
        }

        private Integration.Epep.Person GetPersonFromModel(PersonNamesBase model)
        {
            if (model.IsPerson)
            {
                var epepPerson = new Integration.Epep.Person()
                {
                    EGN = model.Uic,
                    Firstname = model.FirstName,
                    Secondname = model.MiddleName,
                    Lastname = model.FamilyName
                };
                if (string.IsNullOrEmpty(epepPerson.Firstname))
                {
                    epepPerson.Firstname = ".";
                }
                if (string.IsNullOrEmpty(epepPerson.Lastname))
                {
                    epepPerson.Lastname = ".";
                }
                return epepPerson;
            }
            else
            {
                return null;
            }
        }
        private Integration.Epep.Entity GetEntityFromModel(PersonNamesBase model)
        {
            if (!model.IsPerson)
            {
                var epepEntity = new Integration.Epep.Entity()
                {
                    Bulstat = model.Uic,
                    Name = model.FullName
                };
                return epepEntity;
            }
            else
            {
                return null;
            }
        }

        #endregion

        public bool AppendDocument(Document model, EpepConstants.ServiceMethod method)
        {
            switch (model.DocumentDirectionId)
            {
                case DocumentConstants.DocumentDirection.Incoming:
                    {
                        var docGroup = repo.GetById<IOWebApplication.Infrastructure.Data.Models.Nomenclatures.DocumentGroup>(model.DocumentGroupId);
                        if (DocumentConstants.DocumentKind.InDocsForEPEP.Contains(docGroup.DocumentKindId))
                        {
                            return appendInDocument(model, method);
                        }
                        else
                        {
                            return true;
                        }
                    }
                case DocumentConstants.DocumentDirection.OutGoing:
                    return appendOutDocument(model, method);
                default:
                    return true;
            }
        }

        /// <summary>
        /// Входящ документ
        /// </summary>
        public bool appendInDocument(Document model, EpepConstants.ServiceMethod method)
        {
            try
            {
                var epep = new Integration.Epep.IncomingDocument()
                {
                    CourtCode = getNomValue(EpepConstants.Nomenclatures.Courts, model.CourtId),
                    IncomingDocumentTypeCode = getNomValue(EpepConstants.Nomenclatures.IncommingDocumentTypes, model.DocumentTypeId),
                    IncomingNumber = model.DocumentNumberValue ?? 0,
                    IncomingDate = model.DocumentDate
                };
                if (epep.IncomingDocumentTypeCode.StartsWith("!", StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
                if (method != EpepConstants.ServiceMethod.Add)
                {
                    epep.IncomingDocumentId = getKeyGUID(SourceTypeSelectVM.Document, model.Id) ?? Guid.Empty;
                }
                if (method == ServiceMethod.Delete)
                {
                    initFromEpepModel(epep, SourceTypeSelectVM.Document, model.Id, method);
                    return true;
                }

                int caseId = 0;
                if (model.DocumentCaseInfo.Count > 0)
                {
                    caseId = model.DocumentCaseInfo.First().CaseId ?? 0;
                    epep.CaseId = getKeyGUID(SourceTypeSelectVM.Case, caseId);
                }


                var firstPerson = model.DocumentPersons.FirstOrDefault();
                epep.Person = GetPersonFromModel(firstPerson);
                epep.Entity = GetEntityFromModel(firstPerson);


                initFromEpepModel(epep, SourceTypeSelectVM.Document, model.Id, method);
                if (caseId > 0 && !string.IsNullOrEmpty(model.DocumentNumber))
                {
                    if (ISPN_IsISPN(caseId))
                        ISPN_Document(model.Id, method, caseId);
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "appendInDocument");
                return false;
            }
        }



        /// <summary>
        /// Изходящ документ
        /// </summary>
        public bool appendOutDocument(Document model, EpepConstants.ServiceMethod method)
        {
            try
            {
                var epep = new Integration.Epep.OutgoingDocument()
                {
                    OutgoingDocumentTypeCode = getNomValue(EpepConstants.Nomenclatures.OutgoingDocumentTypes, model.DocumentTypeId),
                    OutgoingNumber = model.DocumentNumberValue ?? 0,
                    OutgoingDate = model.DocumentDate
                };
                if (method != EpepConstants.ServiceMethod.Add)
                {
                    epep.OutgoingDocumentId = getKeyGUID(SourceTypeSelectVM.Document, model.Id) ?? Guid.Empty;
                }

                if (method == ServiceMethod.Delete)
                {
                    initFromEpepModel(epep, SourceTypeSelectVM.Document, model.Id, method);
                    return true;
                }
                int caseId = 0;
                if (model.DocumentCaseInfo.Count > 0)
                {
                    caseId = model.DocumentCaseInfo.First().CaseId ?? 0;
                    epep.CaseId = getKeyGUID(SourceTypeSelectVM.Case, caseId);
                }

                var firstPerson = model.DocumentPersons.FirstOrDefault();
                epep.Person = GetPersonFromModel(firstPerson);
                epep.Entity = GetEntityFromModel(firstPerson);

                initFromEpepModel(epep, SourceTypeSelectVM.Document, model.Id, method);
                if (caseId > 0 && !string.IsNullOrEmpty(model.DocumentNumber))
                {
                    if (ISPN_IsISPN(caseId))
                        ISPN_Document(model.Id, method, caseId);
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "appendOutDocument");
                return false;
            }
        }

        public bool AppendFile(CdnUploadRequest model, EpepConstants.ServiceMethod method)
        {
            var fileId = repo.AllReadonly<MongoFile>()
                                .Where(x => x.FileId == model.FileId)
                                .Select(x => x.Id)
                                .FirstOrDefault();
            switch (model.SourceType)
            {
                case SourceTypeSelectVM.Document:
                    var _doc = repo.GetById<Document>(long.Parse(model.SourceId));
                    switch (_doc.DocumentDirectionId)
                    {
                        case DocumentConstants.DocumentDirection.Incoming:
                            var epepIn = new Integration.Epep.IncomingDocumentFile()
                            {
                                IncomingDocumentId = getKeyGUID(SourceTypeSelectVM.Document, _doc.Id) ?? Guid.Empty
                            };
                            initFromEpepModel(epepIn, SourceTypeSelectVM.Files, fileId, method, _doc.Id);
                            return true;
                        case DocumentConstants.DocumentDirection.OutGoing:
                            var epepOut = new Integration.Epep.OutgoingDocumentFile()
                            {
                                OutgoingDocumentId = getKeyGUID(SourceTypeSelectVM.Document, _doc.Id) ?? Guid.Empty
                            };
                            initFromEpepModel(epepOut, SourceTypeSelectVM.Files, fileId, method, _doc.Id);
                            return true;
                        default:
                            return true;
                    }
                case SourceTypeSelectVM.CaseSelectionProtokol:
                    var _protocolId = int.Parse(model.SourceId);

                    var epepProtokol = new Integration.Epep.AssignmentFile()
                    {
                        AssignmentId = getKeyGUID(SourceTypeSelectVM.CaseSelectionProtokol, _protocolId) ?? Guid.Empty
                    };
                    initFromEpepModel(epepProtokol, SourceTypeSelectVM.Files, fileId, method, _protocolId);
                    return true;
                default:
                    return true;

            }
        }

        public bool AppendCase(Case model, EpepConstants.ServiceMethod method)
        {
            var info = repo.AllReadonly<Case>()
                                .Include(x => x.Document)
                                .Include(x => x.CaseState)
                                .Include(x => x.CaseCode)
                                .Where(x => x.Id == model.Id)
                                .Select(x => new
                                {
                                    DocumentId = x.DocumentId,
                                    DocumentNumber = x.Document.DocumentNumber,
                                    StateName = x.CaseState.Label,
                                    CaseCode = x.CaseCode.Code
                                }).FirstOrDefault();
            var courtDepartment = repo.AllReadonly<CaseLawUnit>()
                                    .Include(x => x.CourtDepartment)
                                    .ThenInclude(x => x.ParentDepartment)
                                    .Where(x => x.CaseId == model.Id && x.CaseSessionId == null)
                                    .Where(x => x.DateFrom <= DateTime.Now && (x.DateTo ?? DateTime.MaxValue) >= DateTime.Now)
                                    .Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                    .Select(x => x.CourtDepartment)
                                    .FirstOrDefault();
            try
            {
                var epep = new Integration.Epep.Case()
                {
                    CourtCode = getNomValue(EpepConstants.Nomenclatures.Courts, model.CourtId),
                    IncomingDocumentId = getKeyGUID(SourceTypeSelectVM.Document, model.DocumentId) ?? Guid.Empty,
                    CaseKindCode = getNomValue(EpepConstants.Nomenclatures.CaseTypes, model.CaseTypeId),
                    CaseTypeCode = getNomValue(EpepConstants.Nomenclatures.CaseGroups, model.CaseGroupId),
                    CaseYear = model.RegDate.Year,
                    Number = model.ShortNumberValue ?? 0,
                    FormationDate = model.RegDate
                    //Status = info.StateName
                    //StatisticCode = info.CaseCode
                };

                if (courtDepartment != null)
                {
                    epep.PanelName = courtDepartment.Label;
                    if (courtDepartment.ParentDepartment != null)
                    {
                        epep.DepartmentName = courtDepartment.ParentDepartment.Label;
                    }
                }

                if (method != EpepConstants.ServiceMethod.Add)
                {
                    epep.CaseId = getKeyGUID(SourceTypeSelectVM.Case, model.Id) ?? Guid.Empty;
                }

                initFromEpepModel(epep, SourceTypeSelectVM.Case, model.Id, method, info.DocumentId);

                //При образуване на делото се подават и всички страни по него
                if (method == EpepConstants.ServiceMethod.Add)
                {
                    foreach (var casePerson in model.CasePersons)
                    {
                        AppendCasePerson(casePerson, EpepConstants.ServiceMethod.Add);
                    }
                }
                if (ISPN_IsISPN(model) && !string.IsNullOrEmpty(model.RegNumber))
                {
                    ISPN_Case(model.Id, method);
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AppendCase");
                return false;
            }
        }

        public bool AppendCasePerson(CasePerson model, EpepConstants.ServiceMethod method)
        {

            try
            {
                var epep = new Integration.Epep.Side()
                {
                    SideInvolvementKindCode = getNomValue(EpepConstants.Nomenclatures.PersonRoles, model.PersonRoleId),
                    CaseId = getKeyGUID(SourceTypeSelectVM.Case, model.CaseId) ?? Guid.Empty,
                    Person = GetPersonFromModel(model),
                    Entity = GetEntityFromModel(model),
                    IsActive = model.DateExpired == null,
                    InsertDate = model.DateFrom
                };

                if (method != EpepConstants.ServiceMethod.Add)
                {
                    epep.CaseId = getKeyGUID(SourceTypeSelectVM.CasePerson, model.Id) ?? Guid.Empty;
                }

                initFromEpepModel(epep, SourceTypeSelectVM.CasePerson, model.Id, method, model.CaseId);
                if (ISPN_IsISPN(model.Case, model.CaseId))
                {
                    ISPN_CasePerson(model.Id, method, model.CaseId);
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AppendCasePerson");
                return false;
            }
        }

        public bool AppendCaseSelectionProtocol(CaseSelectionProtokol model, EpepConstants.ServiceMethod method)
        {
            var info = repo.AllReadonly<CaseSelectionProtokol>()
                               .Include(x => x.Case)
                               .ThenInclude(x => x.Document)
                               .Include(x => x.SelectionMode)
                               .Include(x => x.SelectedLawUnit)
                               .Include(x => x.User)
                               .ThenInclude(x => x.LawUnit)
                               .Where(x => x.Id == model.Id)
                               .Select(x => new
                               {
                                   DocumentId = x.Case.DocumentId,
                                   CaseId = x.CaseId,
                                   SelectionModeName = x.SelectionMode.Label,
                                   JudgeRoleId = x.JudgeRoleId,
                                   JudgeName = x.SelectedLawUnit.FullName,
                                   ProtocolUserName = x.User.LawUnit.FullName
                               }).FirstOrDefault();
            try
            {
                var epep = new Integration.Epep.Assignment()
                {
                    CaseId = getKeyGUID(SourceTypeSelectVM.Case, info.CaseId) ?? Guid.Empty,
                    IncomingDocumentId = getKeyGUID(SourceTypeSelectVM.Document, info.DocumentId) ?? Guid.Empty,
                    Type = info.SelectionModeName,
                    Date = model.SelectionDate,
                    JudgeName = info.JudgeName,
                    Assignor = info.ProtocolUserName
                };

                initFromEpepModel(epep, SourceTypeSelectVM.CaseSelectionProtokol, model.Id, method, info.CaseId);


                var epepFile = new Integration.Epep.AssignmentFile()
                {
                    AssignmentId = Guid.Empty
                };
                initFromEpepModel(epepFile, SourceTypeSelectVM.CaseSelectionProtokolFile, model.Id, EpepConstants.ServiceMethod.Add, model.Id);

                if (info.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                {
                    InitMQ(IntegrationTypes.CSRD, SourceTypeSelectVM.CaseSelectionProtokol, model.Id, method);
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AppendCaseSelectionProtocol");
                return false;
            }
        }

        public bool AppendCaseSession(CaseSession model, EpepConstants.ServiceMethod method)
        {
            var info = repo.AllReadonly<CaseSession>()
                                .Include(x => x.SessionType)
                                .Include(x => x.SessionState)
                                .Include(x => x.CourtHall)
                                .Where(x => x.Id == model.Id)
                                .Select(x => new
                                {
                                    SessionTypeName = (x.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession) ? "Открито" : "Закрито",
                                    HallName = (x.CourtHall != null) ? x.CourtHall.Name : "",
                                    SessionState = x.SessionState.Label,
                                    x.SessionStateId
                                }).FirstOrDefault();
            var sessionResult = repo.AllReadonly<Infrastructure.Data.Models.Cases.CaseSessionResult>()
                                        .Where(x => x.CaseSessionId == model.Id && x.IsMain && x.IsActive)
                                        .Select(x => x.SessionResult.Label)
                                        .FirstOrDefault();
            try
            {
                var epep = new Integration.Epep.Hearing()
                {
                    HearingId = getKeyGUID(SourceTypeSelectVM.CaseSession, model.Id) ?? Guid.Empty,
                    CaseId = getKeyGUID(SourceTypeSelectVM.Case, model.CaseId) ?? Guid.Empty,
                    HearingType = info.SessionTypeName,
                    CourtRoom = info.HallName,
                    Date = model.DateFrom,
                    HearingResult = sessionResult,
                    IsCanceled = NomenclatureConstants.SessionState.CanceledSessions.Contains(info.SessionStateId)
                };
                if (epep.IsCanceled && string.IsNullOrEmpty(epep.HearingResult))
                {
                    epep.HearingResult = info.SessionState;
                }

                initFromEpepModel(epep, SourceTypeSelectVM.CaseSession, model.Id, method, model.CaseId);
                if (method != ServiceMethod.Delete)
                {
                    if (ISPN_IsISPN(model.Case, model.CaseId))
                    {
                        ISPN_CaseSession(model.Id, method, model.CaseId);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AppendCaseSession");
                return false;
            }
        }

        public bool AppendCaseSessionLawUnit(CaseLawUnit model, EpepConstants.ServiceMethod method)
        {
            var info = repo.AllReadonly<CaseLawUnit>()
                                .Include(x => x.LawUnit)
                                .Include(x => x.JudgeRole)
                                .Where(x => x.Id == model.Id)
                                .Select(x => new
                                {
                                    JudgeName = x.LawUnit.FullName,
                                    RoleName = x.JudgeRole.Label
                                }).FirstOrDefault();
            try
            {
                var epep = new Integration.Epep.HearingParticipant()
                {
                    HearingId = getKeyGUID(SourceTypeSelectVM.CaseSession, model.CaseSessionId) ?? Guid.Empty,
                    JudgeName = info.JudgeName,
                    Role = info.RoleName
                };

                if (method == EpepConstants.ServiceMethod.Update || method == EpepConstants.ServiceMethod.Delete)
                {
                    epep.HearingParticipantId = getKeyGUID(SourceTypeSelectVM.CaseLawUnit, model.Id) ?? Guid.Empty;
                }

                initFromEpepModel(epep, SourceTypeSelectVM.Case, model.Id, method, model.CaseSessionId);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AppendCaseSessionLawUnit");
                return false;
            }
        }

        public bool AppendJudgeReporter(int caseLawunitId, EpepConstants.ServiceMethod method)
        {
            var info = repo.AllReadonly<CaseLawUnit>()
                                .Where(x => x.Id == caseLawunitId)
                                .Select(x => new
                                {
                                    CaseId = x.CaseId,
                                    JudgeName = x.LawUnit.FullName,
                                    DateFrom = x.DateFrom,
                                    x.Case
                                }).FirstOrDefault();
            try
            {
                var epep = new Integration.Epep.Reporter()
                {
                    CaseId = getKeyGUID(SourceTypeSelectVM.CaseSession, info.CaseId) ?? Guid.Empty,
                    JudgeName = info.JudgeName,
                    DateAssigned = info.DateFrom
                };

                if (method == EpepConstants.ServiceMethod.Update || method == EpepConstants.ServiceMethod.Delete)
                {
                    epep.ReporterId = getKeyGUID(SourceTypeSelectVM.CaseLawUnit, caseLawunitId) ?? Guid.Empty;
                }



                initFromEpepModel(epep, SourceTypeSelectVM.CaseLawUnit, caseLawunitId, method, info.CaseId);
                if (ISPN_IsISPN(info.Case))
                {
                    ISPN_CaseLawUnit(caseLawunitId, method, info.CaseId);
                }


                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AppendJudgeReporter");
                return false;
            }
        }

        public bool AppendCaseNotification(CaseNotification model, ServiceMethod method)
        {
            //Изпращат се само призовки за страни, с начин на доставка през ЕПЕП
            if (model.CasePersonId == null || model.NotificationDeliveryGroupId != NomenclatureConstants.NotificationDeliveryGroup.ByEPEP)
            {
                return false;
            }
            try
            {
                var notificationInfo = repo.AllReadonly<CaseNotification>()
                                        .Include(x => x.CasePerson)
                                        .Include(x => x.HtmlTemplate)
                                        .Where(x => x.Id == model.Id)
                                        .Select(x => new
                                        {
                                            CaseId = x.CaseId,
                                            CasePersonIdentificator = x.CasePerson.CasePersonIdentificator,
                                            CasePersonIdentificatorL2 = (x.CasePersonL2 != null) ? x.CasePersonL2.CasePersonIdentificator : (string)null,
                                            CasePersonIdentificatorL3 = (x.CasePersonL3 != null) ? x.CasePersonL3.CasePersonIdentificator : (string)null,
                                            FullName = x.CasePerson.FullName,
                                            BlankName = x.HtmlTemplate.Label,
                                            Description = x.Description
                                        }).FirstOrDefault();

                var casePersonId = repo.AllReadonly<CasePerson>()
                             .Where(x => x.CasePersonIdentificator == notificationInfo.CasePersonIdentificator && x.CaseSessionId == null)
                             .Select(x => x.Id)
                             .FirstOrDefault();

                var epepCPid = notificationInfo.CasePersonIdentificatorL3 ?? (notificationInfo.CasePersonIdentificatorL2 ?? notificationInfo.CasePersonIdentificator);

                var epepUserId = getEpepUserIdByCasePersonIdentificator(notificationInfo.CaseId, epepCPid);

                if (epepUserId == 0 && epepCPid != notificationInfo.CasePersonIdentificator)
                {
                    epepUserId = getEpepUserIdByCasePersonIdentificator(notificationInfo.CaseId, notificationInfo.CasePersonIdentificator);
                }

                if (epepUserId == 0)
                {
                    return false;
                }

                var epep = new Integration.Epep.Summon()
                {
                    SummonId = getKeyGUID(SourceTypeSelectVM.CaseNotification, model.Id),
                    SummonTypeCode = SummonTypeCode_Prizovka,//Призовка
                    SummonKind = notificationInfo.BlankName,//Не е ясно
                    SideId = getKeyGUID(SourceTypeSelectVM.CasePerson, casePersonId) ?? Guid.Empty,
                    Addressee = notificationInfo.FullName,
                    Subject = notificationInfo.Description,
                    DateCreated = model.RegDate
                };

                if (method == EpepConstants.ServiceMethod.Add && epep.SummonId != null)
                {
                    method = EpepConstants.ServiceMethod.Update;
                }

                initFromEpepModel(epep, SourceTypeSelectVM.CaseNotification, model.Id, method, epepUserId);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AppendCaseNotification");
                return false;
            }
        }

        private int getEpepUserIdByCasePersonIdentificator(int caseId, string personIdentificatior)
        {
            var epepCasePersonId = repo.AllReadonly<CasePerson>()
                            .Where(x => x.CasePersonIdentificator == personIdentificatior && x.CaseSessionId == null)
                            .Select(x => x.Id)
                            .FirstOrDefault();

            return repo.AllReadonly<EpepUserAssignment>()
                                   .Where(x => x.CaseId == caseId && x.CasePersonId == epepCasePersonId)
                                   .Where(x => x.CanSummon == true)
                                   .Select(x => x.EpepUserId)
                                   .FirstOrDefault();
        }

        public bool AppendCaseNotificationFile(int caseNotificationId)
        {
            var epep = new Integration.Epep.SummonFile();
            initFromEpepModel(epep, SourceTypeSelectVM.CaseNotificationPrint, caseNotificationId, EpepConstants.ServiceMethod.Add, caseNotificationId);
            return true;
        }

        public bool SendActPreparators(int caseSessionActId)
        {
            try
            {
                var epep = new Integration.Epep.ActPreparator();
                initFromEpepModel(epep, SourceTypeSelectVM.CaseSessionActPreparatorByAct, caseSessionActId, EpepConstants.ServiceMethod.Update);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SendActPreparators");
                return false;
            }
        }

        private bool checkSessionActCanSent(int actId)
        {
            var info = repo.AllReadonly<CaseSessionAct>()
                            .Include(x => x.CaseSession)
                            .Where(x => x.Id == actId)
                            .Where(x => x.DateExpired == null)
                            .Where(x => x.ActDeclaredDate != null)
                            .Select(x => new
                            {
                                x.RegNumber,
                                x.CaseSession.SessionStateId
                            }).FirstOrDefault();
            if (info == null)
            {
                return false;
            }
            if (string.IsNullOrEmpty(info.RegNumber))
            {
                return false;
            }
            if (info.SessionStateId != NomenclatureConstants.SessionState.Provedeno)
            {
                return false;
            }
            return true;
        }

        public bool AppendCaseSessionAct(CaseSessionAct model, ServiceMethod method)
        {
            if (!checkSessionActCanSent(model.Id))
            {
                return true;
            }
            try
            {
                var epep = new Integration.Epep.Act()
                {
                    ActId = getKeyGUID(SourceTypeSelectVM.CaseSessionAct, model.Id),
                    HearingId = getKeyGUID(SourceTypeSelectVM.CaseSession, model.CaseSessionId) ?? Guid.Empty,
                    Number = int.Parse(model.RegNumber),
                    Finishing = model.IsFinalDoc,
                    CanBeSubjectToAppeal = model.CanAppeal,
                    ActKindCode = getNomValue(EpepConstants.Nomenclatures.ActTypes, model.ActTypeId),
                    CaseId = getKeyGUID(SourceTypeSelectVM.Case, model.CaseId) ?? Guid.Empty,
                    DateInPower = model.ActInforcedDate,
                    DateSigned = model.ActDeclaredDate.Value,
                    MotiveDate = model.ActMotivesDeclaredDate
                };

                initFromEpepModel(epep, SourceTypeSelectVM.CaseSessionAct, model.Id, method, model.CaseSessionId);
                if (ISPN_IsISPN(model.Case, model.CaseId ?? 0))
                {
                    ISPN_CaseSessionAct(model.Id, method, model.CaseId);
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AppendCaseSessionAct");
                return false;
            }
        }

        public bool AppendActsFromSession(int sessionId)
        {
            //Всички постановени актове, които могат да се подписват по време на заседанието
            var sessionActs = repo.AllReadonly<CaseSessionAct>()
                                     .Where(x => x.CaseSessionId == sessionId)
                                     .Where(x => x.DateExpired == null && x.ActDeclaredDate != null)
                                     .Where(x => NomenclatureConstants.ActType.CanSignBeforeSessionEnd.Contains(x.ActTypeId))
                                     .ToList();

            foreach (var act in sessionActs)
            {
                if (!checkSessionActCanSent(act.Id))
                {
                    continue;
                }
                var hasMQrecords = repo.AllReadonly<MQEpep>()
                                            .Where(x => x.SourceType == SourceTypeSelectVM.CaseSessionAct && x.SourceId == (long)act.Id)
                                            .Where(x => x.IntegrationTypeId == NomenclatureConstants.IntegrationTypes.EPEP)
                                            .Any();
                if (hasMQrecords)
                {
                    continue;
                }

                AppendCaseSessionAct(act, ServiceMethod.Add);
                AppendCaseSessionAct_Public(act.Id, ServiceMethod.Add);
                var hasSignTasks = repo.AllReadonly<WorkTask>()
                                            .Where(x => x.SourceType == SourceTypeSelectVM.CaseSessionAct && x.SourceId == (long)act.Id)
                                            .Where(x => x.TaskTypeId == WorkTaskConstants.Types.CaseSessionAct_Sign)
                                            .Where(x => WorkTaskConstants.States.NotFinished.Contains(x.TaskStateId))
                                            .Any();
                if (!hasSignTasks)
                {
                    AppendCaseSessionAct_Private(act.Id, ServiceMethod.Add);
                }
            }

            return sessionActs.Any();
        }

        public bool AppendCaseSessionComplain(CaseSessionActComplain model, ServiceMethod method)
        {
            var info = repo.AllReadonly<CaseSessionActComplain>()
                                .Include(x => x.ComplainDocument)
                                .Include(x => x.CasePersons)
                                .Where(x => x.Id == model.Id)
                                .Where(x => x.CasePersons != null)
                                .Select(x => new
                                {
                                    ActId = x.CaseSessionActId,
                                    AppealDate = x.ComplainDocument.DocumentDate,
                                    AppealDocType = x.ComplainDocument.DocumentTypeId,
                                    Persons = x.CasePersons.Select(p => p.CasePersonId).ToArray()
                                }).FirstOrDefault();

            //докато все още няма дефинирано лице жалбоподател не се изпраща обжалването
            if (!info.Persons.Any())
            {
                return false;
            }
            try
            {
                var epep = new Integration.Epep.Appeal()
                {
                    AppealId = getKeyGUID(SourceTypeSelectVM.CaseSessionActComplain, model.Id),
                    ActId = getKeyGUID(SourceTypeSelectVM.CaseSessionAct, info.ActId) ?? Guid.Empty,
                    AppealKindCode = getNomValue(EpepConstants.Nomenclatures.SessionActAppealDocType, info.AppealDocType),
                    DateFiled = info.AppealDate,
                    SideId = getKeyGUID(SourceTypeSelectVM.CasePerson, info.Persons.FirstOrDefault()) ?? Guid.Empty
                };

                initFromEpepModel(epep, SourceTypeSelectVM.CaseSessionActComplain, model.Id, method, info.ActId);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AppendCaseSessionComplain");
                return false;
            }
        }
        public bool AppendCaseSessionAct_Private(int actId, ServiceMethod method)
        {
            if (!checkSessionActCanSent(actId))
            {
                return true;
            }
            var actFile = cdnService.Select(SourceTypeSelectVM.CaseSessionActPdf, actId.ToString()).FirstOrDefault();
            if (actFile != null)
            {
                var epep = new Integration.Epep.PrivateActFile()
                {
                    PrivateActFileId = getKeyGUID(SourceTypeSelectVM.CaseSessionActPdf, actId),
                    ActId = getKeyGUID(SourceTypeSelectVM.CaseSessionAct, actId) ?? Guid.Empty
                };
                initFromEpepModel(epep, SourceTypeSelectVM.CaseSessionActPdf, actId, method, actId);

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AppendCaseSessionAct_Public(int actId, ServiceMethod method)
        {
            if (!checkSessionActCanSent(actId))
            {
                return true;
            }
            var actFile = cdnService.Select(SourceTypeSelectVM.CaseSessionActDepersonalized, actId.ToString()).FirstOrDefault();
            if (actFile == null)
            {
                return false;
            }
            var epep = new Integration.Epep.PublicActFile()
            {
                PublicActFileId = getKeyGUID(SourceTypeSelectVM.CaseSessionActDepersonalized, actId),
                ActId = getKeyGUID(SourceTypeSelectVM.CaseSessionAct, actId) ?? Guid.Empty
            };
            initFromEpepModel(epep, SourceTypeSelectVM.CaseSessionActDepersonalized, actId, method, actId);
            var actModel = repo.GetById<CaseSessionAct>(actId);
            if (actModel.IsFinalDoc)
            {
                LegalActs_SendAct(actId, method);
            }
            if (method == ServiceMethod.Add)
            {
                EPRO_AppendActFile(actModel);
            }
            return true;

        }

        public bool AppendCaseSessionAct_PrivateMotive(int actId, ServiceMethod method)
        {
            if (!checkSessionActCanSent(actId))
            {
                return true;
            }
            var actFile = cdnService.Select(SourceTypeSelectVM.CaseSessionActMotivePdf, actId.ToString()).FirstOrDefault();
            if (actFile != null)
            {
                var epep = new Integration.Epep.PrivateMotiveFile()
                {
                    PrivateMotiveFileId = getKeyGUID(SourceTypeSelectVM.CaseSessionActMotivePdf, actId),
                    ActId = getKeyGUID(SourceTypeSelectVM.CaseSessionAct, actId) ?? Guid.Empty
                };
                initFromEpepModel(epep, SourceTypeSelectVM.CaseSessionActMotivePdf, actId, method, actId);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AppendCaseSessionAct_PublicMotive(int actId, ServiceMethod method)
        {
            if (!checkSessionActCanSent(actId))
            {
                return true;
            }
            var actFile = cdnService.Select(SourceTypeSelectVM.CaseSessionActMotiveDepersonalized, actId.ToString()).FirstOrDefault();
            if (actFile == null)
            {
                return false;
            }
            var epep = new Integration.Epep.PublicMotiveFile()
            {
                PublicMotiveFileId = getKeyGUID(SourceTypeSelectVM.CaseSessionActMotiveDepersonalized, actId),
                ActId = getKeyGUID(SourceTypeSelectVM.CaseSessionAct, actId) ?? Guid.Empty
            };
            initFromEpepModel(epep, SourceTypeSelectVM.CaseSessionActMotiveDepersonalized, actId, method, actId);
            var actModel = repo.GetById<CaseSessionAct>(actId);
            if (actModel.IsFinalDoc)
            {
                LegalActs_SendAct(actId, ServiceMethod.Update);
            }
            return true;

        }

        public bool AppendPersonRegistration(EpepUser model, EpepConstants.ServiceMethod method)
        {

            if (model != null)
            {
                var epep = new Integration.Epep.PersonRegistration()
                {
                    Name = model.FullName,
                    Email = model.Email,
                    BirthDate = model.BirthDate.Value,
                    Address = model.Address,
                    Description = model.Description,
                    EGN = model.Uic
                };
                initFromEpepModel(epep, SourceTypeSelectVM.EpepUser, model.Id, method, model.Id);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AppendLawyerRegistration(EpepUser model, EpepConstants.ServiceMethod method)
        {

            if (model != null)
            {
                var epep = new Integration.Epep.LawyerRegistration()
                {
                    Email = model.Email,
                    BirthDate = model.BirthDate.Value,
                    Description = model.Description,
                };
                initFromEpepModel(epep, SourceTypeSelectVM.EpepUser, model.Id, method, model.Id);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AppendPersonAssignment(EpepUserAssignment model, EpepConstants.ServiceMethod method)
        {

            if (model != null)
            {
                var epep = new Integration.Epep.PersonAssignment()
                {
                    Date = model.DateFrom,
                    IsActive = model.DescriptionExpired == null,
                    PersonRegistrationId = getKeyGUID(SourceTypeSelectVM.EpepUser, model.EpepUserId) ?? Guid.Empty,
                    SideId = getKeyGUID(SourceTypeSelectVM.CasePerson, model.CasePersonId) ?? Guid.Empty
                };
                initFromEpepModel(epep, SourceTypeSelectVM.EpepUserAssignment, model.Id, method, model.EpepUserId);
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool AppendLawyerAssignment(EpepUserAssignment model, EpepConstants.ServiceMethod method)
        {
            if (model != null)
            {
                var epep = new Integration.Epep.LawyerAssignment()
                {
                    Date = model.DateFrom,
                    IsActive = model.DescriptionExpired == null,
                    LawyerRegistrationId = getKeyGUID(SourceTypeSelectVM.EpepUser, model.EpepUserId) ?? Guid.Empty,
                    SideId = getKeyGUID(SourceTypeSelectVM.CasePerson, model.CasePersonId) ?? Guid.Empty
                };
                initFromEpepModel(epep, SourceTypeSelectVM.EpepUserAssignment, model.Id, method, model.EpepUserId);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AppendEpepUserAssignment(EpepUserAssignment model, ServiceMethod method)
        {
            if (model != null)
            {
                var epepUser = GetById<EpepUser>(model.EpepUserId);
                switch (epepUser.EpepUserTypeId)
                {
                    case EpepConstants.UserTypes.Person:
                        {
                            var epep = new Integration.Epep.PersonAssignment()
                            {
                                Date = model.DateFrom,
                                IsActive = model.DescriptionExpired == null,
                                PersonRegistrationId = getKeyGUID(SourceTypeSelectVM.EpepUser, model.EpepUserId) ?? Guid.Empty,
                                SideId = getKeyGUID(SourceTypeSelectVM.CasePerson, model.CasePersonId) ?? Guid.Empty
                            };
                            initFromEpepModel(epep, SourceTypeSelectVM.EpepUserAssignment, model.Id, method, model.EpepUserId);
                        }
                        break;
                    case EpepConstants.UserTypes.Lawyer:
                        {
                            var epep = new Integration.Epep.LawyerAssignment()
                            {
                                Date = model.DateFrom,
                                IsActive = model.DescriptionExpired == null,
                                LawyerRegistrationId = getKeyGUID(SourceTypeSelectVM.EpepUser, model.EpepUserId) ?? Guid.Empty,
                                SideId = getKeyGUID(SourceTypeSelectVM.CasePerson, model.CasePersonId) ?? Guid.Empty
                            };
                            initFromEpepModel(epep, SourceTypeSelectVM.EpepUserAssignment, model.Id, method, model.EpepUserId);
                        }
                        break;
                    default:
                        break;
                }


                return true;
            }
            else
            {
                return false;
            }
        }


        public IQueryable<EpepUserVM> EpepUser_Select(EpepUserFilterVM filter)
        {
            filter.UpdateNullables();
            return repo.AllReadonly<EpepUser>()
                            .Include(x => x.LawyerLawUnit)
                            .Include(x => x.EpepUserType)
                            .Where(x => x.EpepUserTypeId == (filter.EpepUserTypeId ?? x.EpepUserTypeId))
                            .Where(FilterExpireInfo<EpepUser>(false))
                            .OrderBy(x => x.FullName)
                            .Select(x => new EpepUserVM
                            {
                                Id = x.Id,
                                UserTypeName = x.EpepUserType.Label,
                                Email = x.Email,
                                EpepUserTypeId = x.EpepUserTypeId,
                                FullName = (x.LawyerLawUnit != null) ? x.LawyerLawUnit.FullName : x.FullName,
                                LawyerNumber = x.LawyerNumber
                            })
                            .Where(x => EF.Functions.ILike(x.FullName, filter.FullName.ToPaternSearch()))
                            .Where(x => EF.Functions.ILike(x.Email, filter.Email.ToPaternSearch()))
                            .AsQueryable();
        }
        public string EpepUser_Validate(EpepUser model)
        {
            string result = string.Empty;

            if (model.EpepUserTypeId == EpepConstants.UserTypes.Lawyer)
            {
                if ((model.LawyerLawUnitId ?? 0) <= 0)
                {
                    return "Изберете адвокат.";
                }

                if (repo.AllReadonly<EpepUser>().Where(x => x.LawyerLawUnitId == model.LawyerLawUnitId && x.Id != model.Id).Any())
                {
                    return "За избрания адвокат вече съществува потребител в ЕПЕП.";
                }
            }
            else
            {
                if (string.IsNullOrEmpty(model.Uic) || string.IsNullOrEmpty(model.FullName))
                {
                    return "Въведете ЕГН и имена на лицето.";
                }

                if (repo.AllReadonly<EpepUser>().Where(x => x.Uic == model.Uic.Trim() && x.Id != model.Id && x.EpepUserTypeId == EpepConstants.UserTypes.Person).Any())
                {
                    return "За избраното лице вече съществува потребител в ЕПЕП.";
                }
            }

            if (string.IsNullOrEmpty(model.Email))
            {
                return "Въведете Електронна поща.";
            }

            model.Email = model.Email.Trim();

            if (repo.AllReadonly<EpepUser>().Where(x => x.Email == model.Email.Trim() && x.Id != model.Id).Any())
            {
                return "За избраната електронна поща вече съществува потребител в ЕПЕП.";
            }

            if (!model.BirthDate.HasValue)
            {
                return "Въведете Дата на раждане.";
            }

            return result;
        }
        public bool EpepUser_SaveData(EpepUser model)
        {
            if (model.EpepUserTypeId == EpepConstants.UserTypes.Lawyer)
            {
                var _lawyer = repo.AllReadonly<LawUnit>().Where(x => x.Id == model.LawyerLawUnitId).FirstOrDefault();
                model.LawyerNumber = _lawyer.Code;
                model.FullName = _lawyer.FullName;
            }
            else
            {
                model.LawyerNumber = null;
                model.LawyerLawUnitId = null;
            }
            if (model.Id > 0)
            {
                var saved = repo.GetById<EpepUser>(model.Id);
                saved.Uic = model.Uic;
                saved.FullName = model.FullName;
                saved.Email = model.Email;
                saved.BirthDate = model.BirthDate;
                saved.Address = model.Address;
                saved.Description = model.Description;
                saved.LawyerNumber = model.LawyerNumber;
                saved.LawyerLawUnitId = model.LawyerLawUnitId;

                SetUserDateWRT(saved);
                repo.Update(saved);
                repo.SaveChanges();
            }
            else
            {
                SetUserDateWRT(model);
                repo.Add(model);
                repo.SaveChanges();
            }
            switch (model.EpepUserTypeId)
            {
                case EpepConstants.UserTypes.Person:
                    AppendPersonRegistration(model, EpepConstants.ServiceMethod.Add);
                    break;
                case EpepConstants.UserTypes.Lawyer:
                    AppendLawyerRegistration(model, EpepConstants.ServiceMethod.Add);
                    break;
            }
            return true;
        }



        public IQueryable<EpepUserAssignmentVM> EpepUserAssignment_Select(int epepUserId)
        {
            return repo.AllReadonly<EpepUserAssignment>()
                                    .Include(x => x.Court)
                                    .Include(x => x.Case)
                                    .ThenInclude(x => x.CaseType)
                                    .Include(x => x.CasePerson)
                                    .ThenInclude(x => x.PersonRole)
                                    .Where(x => x.EpepUserId == epepUserId)
                                    .Where(FilterExpireInfo<EpepUserAssignment>(false))
                                    .OrderBy(x => x.Id)
                                    .Select(x => new EpepUserAssignmentVM
                                    {
                                        Id = x.Id,
                                        CaseId = x.CaseId,
                                        CanChange = (x.CourtId == userContext.CourtId),
                                        CourtName = x.Court.Label,
                                        CaseInfo = $"{x.Case.CaseType.Code} {x.Case.RegNumber}",
                                        SideInfo = $"{x.CasePerson.FullName} ({x.CasePerson.PersonRole.Label})",
                                        CanSummon = x.CanSummon ?? false
                                    }).AsQueryable();
        }

        public bool EpepUserAssignment_SaveData(EpepUserAssignment model)
        {
            var casePerson = repo.GetById<CasePerson>(model.CasePersonId);
            var epepUser = repo.GetById<EpepUser>(model.EpepUserId);
            model.DateFrom = casePerson.DateFrom;
            if (model.Id > 0)
            {
                var saved = repo.GetById<EpepUserAssignment>(model.Id);
                saved.CourtId = model.CourtId;
                saved.CaseId = model.CaseId;
                saved.CasePersonId = model.CasePersonId;
                saved.DateFrom = model.DateFrom;
                saved.CanSummon = model.CanSummon;

                SetUserDateWRT(saved);
                repo.Update(saved);
                repo.SaveChanges();
            }
            else
            {
                SetUserDateWRT(model);
                repo.Add(model);
                repo.SaveChanges();
            }

            switch (epepUser.EpepUserTypeId)
            {
                case EpepConstants.UserTypes.Person:
                    AppendPersonAssignment(model, EpepConstants.ServiceMethod.Add);
                    break;
                case EpepConstants.UserTypes.Lawyer:
                    AppendLawyerAssignment(model, EpepConstants.ServiceMethod.Add);
                    break;
            }

            return true;
        }

        public bool LegalActs_SendAct(int actId, ServiceMethod method)
        {
            InitMQ(IntegrationTypes.LegalActs, SourceTypeSelectVM.CaseSessionAct, actId, method, actId);
            return true;
        }
        #region ИСПН
        public bool ISPN_Case(int caseId, ServiceMethod method)
        {
            InitMQ(IntegrationTypes.ISPN, SourceTypeSelectVM.Case, caseId, method, caseId);
            return true;
        }
        public bool ISPN_CaseSession(int sessionId, ServiceMethod method, long? caseId)
        {
            InitMQ(IntegrationTypes.ISPN, SourceTypeSelectVM.CaseSession, sessionId, method, caseId);
            return true;
        }
        public bool ISPN_CaseSessionAct(int actId, ServiceMethod method, long? caseId)
        {
            InitMQ(IntegrationTypes.ISPN, SourceTypeSelectVM.CaseSessionAct, actId, method, caseId);
            return true;
        }
        public bool ISPN_CaseSessionResult(int resultId, ServiceMethod method, long? caseId)
        {
            InitMQ(IntegrationTypes.ISPN, SourceTypeSelectVM.CaseSessionResult, resultId, method, caseId);
            return true;
        }
        public bool ISPN_CaseSessionActComplain(int actComplainId, ServiceMethod method, long? caseId)
        {
            InitMQ(IntegrationTypes.ISPN, SourceTypeSelectVM.CaseSessionActComplain, actComplainId, method, caseId);
            return true;
        }
        public bool ISPN_Document(long documentId, ServiceMethod method, int? caseId)
        {
            InitMQ(IntegrationTypes.ISPN, SourceTypeSelectVM.Document, documentId, method, caseId);
            return true;
        }
        public bool ISPN_CasePerson(int personId, ServiceMethod method, long? caseId)
        {
            InitMQ(IntegrationTypes.ISPN, SourceTypeSelectVM.CasePerson, personId, method, caseId);
            return true;
        }
        public bool ISPN_CaseLawUnit(int caseLawUnitId, ServiceMethod method, long? caseId)
        {
            InitMQ(IntegrationTypes.ISPN, SourceTypeSelectVM.CaseLawUnit, caseLawUnitId, method, caseId);
            return true;
        }
        public bool ISPN_IsISPN(int caseId)
        {
            var aCase = repo.AllReadonly<Case>()
                            .Where(x => x.Id == caseId)
                            .FirstOrDefault();
            return (aCase.IsISPNcase == true);
        }
        public bool ISPN_IsISPN(Case _case)
        {
            return (_case.IsISPNcase == true);
        }
        public bool ISPN_IsISPN(Case _case, int caseId)
        {
            if (_case != null)
                return ISPN_IsISPN(_case);
            else
                return ISPN_IsISPN(caseId);
        }

        public IntegrationKey IntegrationKey_GetByOuterKey(int integrationType, string key)
        {
            return repo.AllReadonly<IntegrationKey>()
                            .Where(x => x.IntegrationTypeId == integrationType
                            && EF.Functions.ILike(x.OuterCode, key))
                            .OrderBy(x => x.Id)
                            .FirstOrDefault();
        }

        public List<IntegrationKey> IntegrationKey_SelectToCorrect(int sourceType)
        {
            DateTime dtFromCorrect = new DateTime(1899, 1, 1);
            DateTime dtToCorrect = new DateTime(1901, 12, 1);
            return repo.AllReadonly<IntegrationKey>()
                        .Where(x => x.IntegrationTypeId == NomenclatureConstants.IntegrationTypes.EPEP)
                        .Where(x => x.SourceType == sourceType)
                        .Where(x => x.DateTransferedDW >= dtFromCorrect && x.DateTransferedDW <= dtToCorrect)
                        .Where(x => !x.OuterCode.StartsWith("DEL", StringComparison.InvariantCultureIgnoreCase) && !x.OuterCode.StartsWith("ERR", StringComparison.InvariantCultureIgnoreCase))
                        .ToList();
        }

        public bool IntegrationKey_Correct(IntegrationKey model, bool withError)
        {
            var saved = repo.GetById<IntegrationKey>(model.Id);
            saved.OuterCode = $"DELETED{saved.OuterCode}";
            saved.DateTransferedDW = new DateTime(1902, 1, 1);

            if (withError)
            {
                saved.OuterCode = $"ERROR{saved.OuterCode}";
                saved.DateTransferedDW = new DateTime(1903, 1, 1);
            }
            repo.Update(saved);
            repo.SaveChanges();

            return true;
        }

        public IEnumerable<MQEpepVM> MQEpep_SelectISPN(long sourceId)
        {
            var result = repo.AllReadonly<MQEpep>()
                                .Where(x => x.ParentSourceId == sourceId)
                                .Where(x => x.IntegrationTypeId == IntegrationTypes.ISPN)
                                .OrderBy(x => x.Id)
                                .Select(x => new MQEpepVM
                                {
                                    Id = x.Id,
                                    StateId = x.IntegrationStateId ?? 0,
                                    OperName = (x.MethodName == "add") ? "Добавяне" : "Редакция",
                                    DateWrt = x.DateWrt,
                                    DateTransfered = x.DateTransfered,
                                    ErrorDescription = x.ErrorDescription
                                }).ToList();
            if (result.Any(x => x.StateId != IntegrationStates.TransferOK))
            {
                var casePersons = repo.AllReadonly<CasePerson>()
                                      .Where(x => x.CaseId == sourceId &&
                                                  x.DateExpired == null &&
                                                  x.CaseSessionId == null)
                                      .ToList();
                var caseError = string.Empty;
                foreach (var person in casePersons)
                {
                    if (!person.IsPerson)
                        continue;
                    string personErr = string.Empty;
                    if (person.FirstName?.Length > 22)
                        personErr += " Първото име е над 22 символа";
                    if (string.IsNullOrEmpty(person.MiddleName) && string.IsNullOrEmpty(person.FamilyName))
                        personErr += " Трябва да въведете презиме или фамилия";
                    if (!string.IsNullOrEmpty(personErr))
                    {
                        caseError += $"{person.Uic} {person.FullName} " + personErr + "<br>";
                    }
                }
                foreach (var item in result.Where(x => x.StateId != IntegrationStates.TransferOK))
                {
                    item.ErrorDescription = caseError + item.ErrorDescription;
                }
            }
            return result;
        }
        public IEnumerable<MQEpepVM> MQEpep_Select(int integrationType, int sourceType, long sourceId)
        {
            if (integrationType == IntegrationTypes.ISPN)
                return MQEpep_SelectISPN(sourceId);

            Expression<Func<MQEpep, bool>> whereClause = x => x.SourceType == sourceType && x.SourceId == sourceId;


            if (sourceType == SourceTypeSelectVM.DocumentFiles)
            {

                whereClause = x => x.SourceType == SourceTypeSelectVM.Files && x.ParentSourceId == sourceId;
            }

            return repo.AllReadonly<MQEpep>()
                                .Where(whereClause)
                                .Where(x => x.IntegrationTypeId == integrationType)
                                .OrderBy(x => x.Id)
                                .Select(x => new MQEpepVM
                                {
                                    Id = x.Id,
                                    StateId = x.IntegrationStateId ?? 0,
                                    OperName = (x.MethodName == "add") ? "Добавяне" : "Редакция",
                                    DateWrt = x.DateWrt,
                                    DateTransfered = x.DateTransfered,
                                    ErrorDescription = x.ErrorDescription
                                }).ToList();
        }

        public string RecoverData(object client)
        {
            var ids = repo.All<ID_List>().ToList();
            int saved = 0;
            this.mqID = "scanedFiles";
            //var epepClient = (Integration.Epep.IeCaseServiceClient)client;
            foreach (var item in ids)
            {
                if (saved % 500 == 0)
                {
                    repo.RefreshDbContext(configuration.GetConnectionString("DefaultConnection"));
                }
                saved++;

                var _file = repo.GetById<MongoFile>((int)item.Id);

                AppendFile(new CdnUploadRequest()
                {
                    SourceType = SourceTypeSelectVM.Document,
                    SourceId = _file.SourceId,
                    FileId = _file.FileId
                }, ServiceMethod.Add);

            }

            return $"Saved {saved}/{ids.Count} items.";
        }

        public EpepUser EpepUser_GetByDocument(long documentId)
        {
            EpepUser result = null;

            result = repo.AllReadonly<EpepUser>()
                                .Where(x => x.DocumentId == documentId)
                                .FirstOrDefault();

            if (result != null)
            {
                return result;
            }

            var docPerson = repo.AllReadonly<DocumentPerson>()
                                    .Where(x => x.DocumentId == documentId)
                                    .Select(x => new
                                    {
                                        x.Uic,
                                        x.Person_SourceType,
                                        x.Person_SourceId
                                    }).FirstOrDefault();

            if (docPerson == null)
            {
                return null;
            }

            if (docPerson.Person_SourceType == SourceTypeSelectVM.LawUnit && docPerson.Person_SourceId > 0)
            {
                return repo.AllReadonly<EpepUser>()
                                   .Where(x => x.LawyerLawUnitId == docPerson.Person_SourceId)
                                   .FirstOrDefault();
            }
            if (!string.IsNullOrEmpty(docPerson.Uic))
            {
                return repo.AllReadonly<EpepUser>()
                              .Where(x => x.Uic == docPerson.Uic)
                              .FirstOrDefault();
            }

            return null;
        }

        public EpepUser EpepUser_InitFromDocument(long? documentId)
        {
            var model = new EpepUser()
            {
                EpepUserTypeId = EpepConstants.UserTypes.Person
            };
            if (!documentId.HasValue)
            {
                return model;
            }
            model.DocumentId = documentId;
            var docPerson = repo.AllReadonly<DocumentPerson>()
                                  .Where(x => x.DocumentId == documentId)
                                  .Select(x => new
                                  {
                                      x.FullName,
                                      x.Uic,
                                      x.Person_SourceType,
                                      x.Person_SourceId
                                  }).FirstOrDefault();

            if (docPerson == null)
            {
                return model;
            }

            if (docPerson.Person_SourceType == SourceTypeSelectVM.LawUnit && docPerson.Person_SourceId.HasValue)
            {
                var lawyer = repo.GetById<LawUnit>((int)docPerson.Person_SourceId);
                model.LawyerLawUnitId = lawyer.Id;
                model.FullName = lawyer.FullName;
                model.LawyerNumber = lawyer.Code;
                model.EpepUserTypeId = EpepConstants.UserTypes.Lawyer;
            }
            else
            {
                model.FullName = docPerson.FullName;
                model.Uic = docPerson.Uic;
            }

            return model;
        }

        public EpepDocumentInfoVM EpepUser_DocumentInfo(long? documentId)
        {
            if (documentId == null)
            {
                return null;
            }

            return repo.AllReadonly<Document>()
                            .Include(x => x.Court)
                            .Include(x => x.DocumentType)
                            .Where(x => x.Id == documentId)
                            .Select(x => new EpepDocumentInfoVM
                            {
                                CourtId = x.CourtId,
                                CourtName = x.Court.Label,
                                DocumentId = x.Id,
                                DocumentInfo = $"{x.DocumentType.Label} {x.DocumentNumber}/{x.DocumentDate:dd.MM.yyyy}"
                            }).FirstOrDefault();
        }

        public void ResendDataToEPEP(int sourceType, long sourceId, bool appendChild)
        {
            switch (sourceType)
            {
                case SourceTypeSelectVM.Document:
                    {
                        var model = repo.GetById<Document>(sourceId);
                        if (model != null)
                        {
                            AppendDocument(model, ServiceMethod.Add);
                            ResendDataToEPEP_Files(sourceType, sourceId);
                        }
                    }
                    break;
                case SourceTypeSelectVM.Case:
                    {
                        var model = repo.GetById<Case>((int)sourceId);
                        if (model != null)
                        {
                            AppendCase(model, ServiceMethod.Add);

                            if (appendChild)
                            {
                                var sessions = repo.AllReadonly<CaseSession>()
                                                    .Where(FilterExpireInfo<CaseSession>(false))
                                                    .Where(x => x.CaseId == model.Id)
                                                    .Select(x => x.Id)
                                                    .ToList();

                                foreach (var item in sessions)
                                {
                                    ResendDataToEPEP(SourceTypeSelectVM.CaseSession, item, appendChild);
                                }
                            }
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseSession:
                    {
                        var model = repo.GetById<CaseSession>((int)sourceId);
                        if (model != null)
                        {
                            AppendCaseSession(model, ServiceMethod.Add);

                            if (appendChild)
                            {
                                //всички постановени актове
                                var acts = repo.AllReadonly<CaseSessionAct>()
                                                    .Where(FilterExpireInfo<CaseSessionAct>(false))
                                                    .Where(x => x.CaseSessionId == model.Id)
                                                    .Where(x => x.ActDeclaredDate != null)
                                                    .Select(x => x.Id)
                                                    .ToList();

                                foreach (var item in acts)
                                {
                                    ResendDataToEPEP(SourceTypeSelectVM.CaseSessionAct, item, appendChild);
                                }
                            }
                        }
                    }
                    break;
                case SourceTypeSelectVM.CaseSessionAct:
                    {
                        var model = repo.GetById<CaseSessionAct>((int)sourceId);
                        if (model != null)
                        {
                            AppendCaseSessionAct(model, ServiceMethod.Add);
                            if (model.ActDeclaredDate != null)
                            {
                                AppendCaseSessionAct_Private(model.Id, ServiceMethod.Add);
                            }
                            if (model.DepersonalizeEndDate != null)
                            {
                                AppendCaseSessionAct_Public(model.Id, ServiceMethod.Add);
                            }
                        }
                    }
                    break;
            }
        }
        private void ResendDataToEPEP_Files(int sourceType, long sourceId)
        {
            var files = repo.AllReadonly<MongoFile>()
                                                    .Where(x => x.SourceType == sourceType && x.SourceId == sourceId.ToString())
                                                    .Where(FilterExpireInfo<MongoFile>(false))
                                                    .Select(x => new CdnUploadRequest
                                                    {
                                                        FileId = x.FileId,
                                                        SourceType = x.SourceType,
                                                        SourceId = x.SourceId
                                                    }).ToList();
            foreach (var item in files)
            {
                AppendFile(item, ServiceMethod.Add);
            }
        }

        public void MQEpep_ResetError(int integrationType, int sourceType, long sourceId)
        {
            List<MQEpep> requests;
            if (integrationType == IntegrationTypes.ISPN)
            {
                requests = repo.All<MQEpep>()
                                .Where(x => x.SourceType == sourceType && x.ParentSourceId == sourceId)
                                .Where(x => x.IntegrationTypeId == integrationType)
                                .OrderBy(x => x.Id)
                                .ToList();
            }
            else
            {
                requests = repo.All<MQEpep>()
                                               .Where(x => x.SourceType == sourceType && x.SourceId == sourceId)
                                               .Where(x => x.IntegrationTypeId == integrationType)
                                               .OrderBy(x => x.Id)
                                               .ToList();
            }

            if (requests == null)
            {
                return;
            }

            foreach (var item in requests)
            {
                if (IntegrationStates.ResetMQErrorStates.Contains(item.IntegrationStateId ?? 0))
                {
                    item.ErrorDescription = null;
                    item.ErrorCount = 0;
                    item.IntegrationStateId = IntegrationStates.New;
                    repo.Update(item);
                    repo.SaveChanges();
                }

            }
        }

        public LawUnit GetLawyerByNumber(string lawyerNumber)
        {
            return repo.All<LawUnit>()
                            .Where(x => x.LawUnitTypeId == LawUnitTypes.Lawyer && x.Code == lawyerNumber && x.DateTo == null)
                            .FirstOrDefault();
        }

        public void EPRO_AppendDismissal(int caseDismissalId, int dismissalTypeId)
        {
            if (!userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.EproDismissal))
            {
                return;
            }
            //В ЕПРО се изпращат само Отвод и самоотвод
            if (!NomenclatureConstants.DismisalType.EproDismissalTypes.Contains(dismissalTypeId))
            {
                return;
            }
            InitMQ(NomenclatureConstants.IntegrationTypes.EPRO, SourceTypeSelectVM.CaseLawUnitDismisal, caseDismissalId, EpepConstants.ServiceMethod.Add);
        }

        public void EPRO_AppendReplace(int caseSelectionProtocolId, int caseDismissalId)
        {
            if (!userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.EproDismissal))
            {
                return;
            }
            var dismissalTypeId = repo.AllReadonly<CaseLawUnitDismisal>().Where(x => x.Id == caseDismissalId).Select(x => x.DismisalTypeId).FirstOrDefault();
            //В ЕПРО се изпращат само Отвод и самоотвод
            if (!NomenclatureConstants.DismisalType.EproDismissalTypes.Contains(dismissalTypeId))
            {
                return;
            }
            InitMQ(NomenclatureConstants.IntegrationTypes.EPRO, SourceTypeSelectVM.CaseSelectionProtokol, caseSelectionProtocolId, EpepConstants.ServiceMethod.Add, caseDismissalId);
        }

        public void EPRO_AppendActFile(CaseSessionAct actModel)
        {
            if (!userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.EproDismissal))
            {
                return;
            }
            var dismissalsByActId = repo.AllReadonly<CaseLawUnitDismisal>()
                                            .Where(x => x.CaseSessionActId == actModel.Id && x.CaseId == actModel.CaseId)
                                            .Select(x => x.Id)
                                            .ToList();
            foreach (var dismissalId in dismissalsByActId)
            {
                InitMQ(NomenclatureConstants.IntegrationTypes.EPRO, SourceTypeSelectVM.CaseSessionActDepersonalized, actModel.Id, EpepConstants.ServiceMethod.Add, dismissalId);
            }
        }

        public string EpepUserAssignment_Validate(EpepUserAssignment model)
        {

            if (repo.AllReadonly<EpepUserAssignment>()
                        .Where(x => x.CaseId == model.CaseId && x.CasePersonId == model.CasePersonId && x.Id != model.Id)
                        .Where(x => x.EpepUserId == model.EpepUserId)
                        .Where(x => x.DateExpired == null)
                        .Any())
            {
                return "Вече съществува достъп до избраното дело и лице";
            }

            return null;
        }


        #endregion ИСПН
    }
}
