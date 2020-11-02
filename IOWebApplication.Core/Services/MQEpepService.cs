// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

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
using iText.Kernel.XMP.Impl;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NPOI.OpenXmlFormats.Dml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Xml.Linq;
using static IOWebApplication.Infrastructure.Constants.EpepConstants;
using static IOWebApplication.Infrastructure.Constants.NomenclatureConstants;

namespace IOWebApplication.Core.Services
{
    public class MQEpepService : BaseIntegrationService, IMQEpepService
    {
        private readonly ICdnService cdnService;

        public MQEpepService(
            ILogger<DocumentService> _logger,
            ICdnService _cdnService,
            IRepository _repo,
            IUserContext _userContext
        )
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            cdnService = _cdnService;
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
                MQId = Guid.NewGuid().ToString(),
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
                MQId = Guid.NewGuid().ToString(),
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
                    return appendInDocument(model, method);
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
                int caseId = 0;
                if (model.DocumentCaseInfo.Count > 0)
                {
                    caseId = model.DocumentCaseInfo.First().CaseId ?? 0;
                    epep.CaseId = getKeyGUID(SourceTypeSelectVM.Case, caseId);
                }

                var firstPerson = model.DocumentPersons.FirstOrDefault();
                epep.Person = GetPersonFromModel(firstPerson);
                epep.Entity = GetEntityFromModel(firstPerson);

                if (method != EpepConstants.ServiceMethod.Add)
                {
                    epep.IncomingDocumentId = getKeyGUID(SourceTypeSelectVM.Document, model.Id) ?? Guid.Empty;
                }
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
                int caseId = 0;
                if (model.DocumentCaseInfo.Count > 0)
                {
                    caseId = model.DocumentCaseInfo.First().CaseId ?? 0;
                    epep.CaseId = getKeyGUID(SourceTypeSelectVM.Case, caseId);
                }

                var firstPerson = model.DocumentPersons.FirstOrDefault();
                epep.Person = GetPersonFromModel(firstPerson);
                epep.Entity = GetEntityFromModel(firstPerson);
                if (method != EpepConstants.ServiceMethod.Add)
                {
                    epep.OutgoingDocumentId = getKeyGUID(SourceTypeSelectVM.Document, model.Id) ?? Guid.Empty;
                }
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
                                .Include(x => x.CourtHall)
                                .Where(x => x.Id == model.Id)
                                .Select(x => new
                                {
                                    SessionTypeName = (x.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession) ? "Открито" : "Закрито",
                                    HallName = (x.CourtHall != null) ? x.CourtHall.Name : ""
                                }).FirstOrDefault();
            try
            {
                var epep = new Integration.Epep.Hearing()
                {
                    HearingId = getKeyGUID(SourceTypeSelectVM.CaseSession, model.Id) ?? Guid.Empty,
                    CaseId = getKeyGUID(SourceTypeSelectVM.Case, model.CaseId) ?? Guid.Empty,
                    HearingType = info.SessionTypeName,
                    CourtRoom = info.HallName,
                    Date = model.DateFrom
                };

                initFromEpepModel(epep, SourceTypeSelectVM.CaseSession, model.Id, method, model.CaseId);
                if (ISPN_IsISPN(model.Case, model.CaseId))
                {
                    ISPN_CaseSession(model.Id, method, model.CaseId);
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
            if (model.CasePersonId == null)
            {
                return false;
            }
            try
            {
                var notificationInfo = repo.AllReadonly<CaseNotification>()
                                        .Include(x => x.CasePerson)
                                        .Include(x => x.HtmlTemplate)
                                        .Where(x => x.Id == model.CasePersonId)
                                        .Select(x => new
                                        {
                                            CaseId = x.CaseId,
                                            CasePersonIdentificator = x.CasePerson.CasePersonIdentificator,
                                            FullName = x.CasePerson.FullName,
                                            BlankName = x.HtmlTemplate.Label
                                        }).FirstOrDefault();

                var casePersonId = repo.AllReadonly<CasePerson>()
                             .Where(x => x.CasePersonIdentificator == notificationInfo.CasePersonIdentificator && x.CaseSessionId == null)
                             .Select(x => x.Id)
                             .FirstOrDefault();

                var epepUserId = repo.AllReadonly<EpepUserAssignment>()
                                       .Where(x => x.CaseId == notificationInfo.CaseId && x.CasePersonId == casePersonId)
                                       .Select(x => x.EpepUserId)
                                       .FirstOrDefault();
                if (epepUserId == 0)
                {
                    return false;
                }

                var epep = new Integration.Epep.Summon()
                {
                    SummonId = getKeyGUID(SourceTypeSelectVM.CaseNotification, model.Id),
                    SummonTypeCode = SummonTypeCode_Prizovka,//Призовка
                    SummonKind = SummonKind_Generic,//Не е ясно
                    SideId = getKeyGUID(SourceTypeSelectVM.CasePerson, casePersonId) ?? Guid.Empty,
                    Addressee = notificationInfo.FullName,
                    Subject = notificationInfo.BlankName,
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

        public bool AppendCaseSessionAct(CaseSessionAct model, ServiceMethod method)
        {

            if (!model.ActDeclaredDate.HasValue)
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
            return true;

        }

        public bool AppendCaseSessionAct_PrivateMotive(int actId, ServiceMethod method)
        {
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

        public IQueryable<EpepUserVM> EpepUser_Select(int? userType, string search)
        {
            return repo.AllReadonly<EpepUser>()
                            .Include(x => x.LawyerLawUnit)
                            .Include(x => x.EpepUserType)
                            .Where(x => x.EpepUserTypeId == (userType ?? x.EpepUserTypeId))
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
                            .Where(x => EF.Functions.ILike(x.FullName, search.ToPaternSearch()) || x.Email.Contains((search ?? x.Email), StringComparison.InvariantCultureIgnoreCase))
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
                saved.FullName = model.FullName;
                saved.Email = model.Email;
                saved.BirthDate = model.BirthDate;
                saved.Address = model.Address;
                saved.Description = model.Description;
                saved.LawyerNumber = model.LawyerNumber;

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
                                    .OrderBy(x => x.Id)
                                    .Select(x => new EpepUserAssignmentVM
                                    {
                                        Id = x.Id,
                                        CaseId = x.CaseId,
                                        CourtName = x.Court.Label,
                                        CaseInfo = $"{x.Case.CaseType.Code} {x.Case.RegNumber}",
                                        SideInfo = $"{x.CasePerson.FullName} ({x.CasePerson.PersonRole.Label})"
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
                            && string.Compare(x.OuterCode, key, StringComparison.InvariantCultureIgnoreCase) == 0)
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

        public IEnumerable<MQEpepVM> MQEpep_Select(int integrationType, int sourceType, long sourceId)
        {
            return repo.AllReadonly<MQEpep>()
                                .Where(x => x.SourceType == sourceType && x.SourceId == sourceId)
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
            var ids = repo.AllReadonly<ID_List>().Select(x => x.Id).ToList();
            var epepClient = (Integration.Epep.IeCaseServiceClient)client;
            int saved = 0;
            foreach (var id in ids)
            {
                //Дела
                var model = repo.AllReadonly<Case>()
                                    .Include(x => x.Document)
                                    .Include(x => x.Court)
                                    .Where(x => x.Id == id)
                                    .Select(x => new
                                    {
                                        DocNumber = x.Document.DocumentNumberValue.Value,
                                        DocYear = x.Document.DocumentDate.Year,
                                        CourtCode = x.Court.Code
                                    })
                                    .FirstOrDefault();
                if (model != null)
                {
                    try
                    {
                        var caseId = epepClient.GetCaseId(model.DocNumber, model.DocYear, model.CourtCode);

                        if (caseId.HasValue && caseId != Guid.Empty)
                        {
                            var mq = repo.All<MQEpep>()
                                         .Where(x => x.SourceType == 2 && x.SourceId == id && x.IntegrationTypeId == 2)
                                         .Where(x => x.MethodName == "add")
                                         .FirstOrDefault();
                            if (mq != null)
                            {
                                mq.ReturnGuidId = caseId.ToString().ToLower();
                                mq.DateTransfered = DateTime.Now;
                                mq.IntegrationStateId = IntegrationStates.TransferOK;
                                repo.Update(mq);

                                var newIK = new IntegrationKey()
                                {
                                    IntegrationTypeId = IntegrationTypes.EPEP,
                                    SourceType = SourceTypeSelectVM.Case,
                                    SourceId = id,
                                    OuterCode = mq.ReturnGuidId,
                                    DateWrt = DateTime.Now
                                };

                                repo.Add(newIK);
                                repo.SaveChanges();

                                saved++;
                            }
                        }
                    }
                    catch (FaultException fex)
                    {
                        var _error = fex.GetMessageFault();                        
                    }
                    catch (Exception ex)
                    {

                    }
                }

                ////Заседания
                //var model = repo.All<CaseSession>()
                //                    .Where(x => x.Id == id).FirstOrDefault();
                //if (model != null)
                //{
                //    AppendCaseSession(model, ServiceMethod.Add);
                //}
            }
            return $"Saved {saved}/{ids.Count} items.";
        }


        #endregion ИСПН
    }
}
