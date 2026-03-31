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
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel;
using System.Threading.Tasks;
using static IOWebApplication.Infrastructure.Constants.EpepConstants;
using static IOWebApplication.Infrastructure.Constants.NomenclatureConstants;
using IO.LogOperation.Models;
using IOWebApplication.Infrastructure.Data.Models.Money;
using IOWebApplication.Infrastructure.Utils;
using EpepRestModels = IOWebApplication.Infrastructure.Models.Integrations.EpepRest;
using IOWebApplication.Infrastructure.Models.Integrations.RNFL;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;

namespace IOWebApplication.Core.Services
{
    public class MQEpepService : BaseIntegrationService, IMQEpepService
    {
        private readonly ICdnService cdnService;
        private readonly IConfiguration configuration;
        private readonly IProxyEissService proxyEissService;
        public string mqID = null;
        private bool AUTO_SAVECHANGES = true;

        public MQEpepService(
            ILogger<MQEpepService> _logger,
            IProxyEissService _proxyEissService,
            ICdnService _cdnService,
            IConfiguration _configuration,
            IRepository _repo,
            IUserContext _userContext
        )
        {
            logger = _logger;
            proxyEissService = _proxyEissService;
            repo = _repo;
            userContext = _userContext;
            cdnService = _cdnService;
            configuration = _configuration;
        }

        public void Set_MqID(string value)
        {
            mqID = value;
        }

        public void Set_AUTOSAVECHANGES(bool value)
        {
            AUTO_SAVECHANGES = value;
        }

        public SaveResultVM MqRestartCase(int caseId)
        {
            var result = repo.ExecuteProc<ReportCourtGenericVM>("public.mq_restart_case({0})", caseId).FirstOrDefault();

            if (result.Count > 0)
            {
                return new SaveResultVM()
                {
                    Result = true,
                    Content = $"Успешно рестартирани {result.Count}бр. заявки."
                };
            }
            else
            {
                return new SaveResultVM()
                {
                    Result = false,
                    Content = $"Няма намерени заявки, подлежащи на рестартиране."
                };
            }
        }


        #region Общи методи за създаване на заявките за изпращане към ЕПЕП

        private void initFromEpepModel(object epepModel, int sourceType, long sourceId, EpepConstants.ServiceMethod method, long? parentSourceId = null, string targetClassName = null)
        {
            var className = targetClassName ?? epepModel.GetType().Name;
            var mq = new MQEpep()
            {
                MQId = mqID ?? Guid.NewGuid().ToString(),
                IntegrationTypeId = NomenclatureConstants.IntegrationTypes.EPEP,
                SourceType = sourceType,
                SourceId = sourceId,
                ParentSourceId = parentSourceId,
                TargetClassName = className,
                DateWrt = DateTime.Now,
                UserId = ImpersonatedUserId ?? userContext.UserId,
                MethodName = EpepConstants.Methods.GetMethod(method),
                ErrorCount = 0,
                IntegrationStateId = IntegrationStates.New
            };

            if (epepModel != null)
            {
                mq.Content = System.Text.Encoding.UTF8.GetBytes(JsonTextSerializer.Serialize(epepModel));
            }

            repo.Add(mq);
            if (AUTO_SAVECHANGES)
            {
                repo.SaveChanges();
            }
        }
        private async Task<bool> checkExistingEpepNewTask(string targetName, int sourceType, long sourceId, string methodName, long? parentId = null)
        {
            return await repo.AllReadonly<MQEpep>()
                            .Where(x => x.TargetClassName == targetName)
                            .Where(x => x.SourceType == sourceType)
                            .Where(x => x.SourceId == sourceId)
                            .Where(x => x.ParentSourceId == (parentId ?? x.ParentSourceId))
                            .Where(x => x.MethodName == methodName)
                            .Where(x => x.IntegrationStateId == IntegrationStates.New)
                            .AnyAsync();
        }

        private async Task initFromEpepModelAsync(object epepModel, int sourceType, long sourceId, EpepConstants.ServiceMethod method, long? parentSourceId = null, string targetClassName = null)
        {
            var className = targetClassName ?? epepModel.GetType().Name;
            var mq = new MQEpep()
            {
                MQId = mqID ?? Guid.NewGuid().ToString(),
                IntegrationTypeId = NomenclatureConstants.IntegrationTypes.EPEP,
                SourceType = sourceType,
                SourceId = sourceId,
                ParentSourceId = parentSourceId,
                TargetClassName = className,
                Content = System.Text.Encoding.UTF8.GetBytes(JsonTextSerializer.Serialize(epepModel)),
                DateWrt = DateTime.Now,
                UserId = ImpersonatedUserId ?? userContext.UserId,
                MethodName = EpepConstants.Methods.GetMethod(method),
                ErrorCount = 0,
                IntegrationStateId = IntegrationStates.New
            };

            await repo.AddAsync(mq);
            if (AUTO_SAVECHANGES)
            {
                await repo.SaveChangesAsync();
            }
        }

        public void InitMQ(int integrationTypeId, int sourceType, long sourceId, EpepConstants.ServiceMethod method, long? parentSourceId = null, object model = null)
        {
            string message = null;
            if (model != null)
            {
                message = JsonTextSerializer.Serialize(model);
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
                UserId = ImpersonatedUserId ?? userContext.UserId,
                MethodName = EpepConstants.Methods.GetMethod(method),
                ErrorCount = 0,
                IntegrationStateId = IntegrationStates.New
            };
            if (message != null)
            {
                mq.Content = System.Text.Encoding.UTF8.GetBytes(message);
            }
            repo.Add(mq);
            if (AUTO_SAVECHANGES)
            {
                repo.SaveChanges();
            }
            return mq.Id;
        }

        private long InitMQWithTarget(int integrationTypeId, string targetClassName, int sourceType, long sourceId, ServiceMethod method, long? parentSourceId = null)
        {
            var mq = new MQEpep()
            {
                MQId = mqID ?? Guid.NewGuid().ToString(),
                TargetClassName = targetClassName,
                IntegrationTypeId = integrationTypeId,
                SourceType = sourceType,
                SourceId = sourceId,
                ParentSourceId = parentSourceId,
                DateWrt = DateTime.Now,
                UserId = ImpersonatedUserId ?? userContext.UserId,
                MethodName = EpepConstants.Methods.GetMethod(method),
                ErrorCount = 0,
                IntegrationStateId = IntegrationStates.New
            };
            repo.Add(mq);
            if (AUTO_SAVECHANGES)
            {
                repo.SaveChanges();
            }
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

        public async Task<bool> AppendDocument(Document model, EpepConstants.ServiceMethod method)
        {
            switch (model.DocumentDirectionId)
            {
                case DocumentConstants.DocumentDirection.Incoming:
                    {
                        int documentKindId = await repo.GetPropByIdAsync<IOWebApplication.Infrastructure.Data.Models.Nomenclatures.DocumentGroup, int>(x => x.Id == model.DocumentGroupId, x => x.DocumentKindId);
                        if (DocumentConstants.DocumentKind.InDocsForEPEP.Contains(documentKindId))
                        {
                            return await appendInDocument(model, method, documentKindId);
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
        private async Task<bool> appendInDocument(Document model, EpepConstants.ServiceMethod method, int documentKindId)
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
                if (string.IsNullOrEmpty(epep.IncomingDocumentTypeCode) || epep.IncomingDocumentTypeCode.StartsWith("!", StringComparison.InvariantCultureIgnoreCase))
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

                if ((model.ElectronicDocumentId ?? 0) == 0)
                    if (caseId == 0 && documentKindId != DocumentConstants.DocumentKind.InitialDocument)
                    {
                        //Ако документа не е свързан с дело да не се изпраща към ЕПЕП и не е иницииращ документ
                        return true;
                    }

                var firstPerson = model.DocumentPersons.FirstOrDefault();
                epep.Person = GetPersonFromModel(firstPerson);
                epep.Entity = GetEntityFromModel(firstPerson);

                if (model.ElectronicDocumentId > 0)
                {
                    var elDocGid = repo.GetPropById<ElectronicDocument, Guid>(x => x.Id == model.ElectronicDocumentId, x => x.EpepId);
                    if (elDocGid != Guid.Empty)
                    {
                        epep.ElectronicDocumentId = elDocGid;
                    }
                }

                initFromEpepModel(epep, SourceTypeSelectVM.Document, model.Id, method);
                if (caseId > 0 && !string.IsNullOrEmpty(model.DocumentNumber))
                {
                    if (ISPN_IsISPN(caseId))
                        ISPN_Document(model.Id, method, caseId);
                }
                if (caseId > 0)
                {
                    await RNFL_SendDocument(model.Id, caseId);
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
        private bool appendOutDocument(Document model, EpepConstants.ServiceMethod method)
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
                else
                {
                    return false;
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

        //Проверява за допустимо изпращането на изходящ документ към външни системи
        //При документ по бланка - се изчаква подписването от всички лица
        public bool CheckOutDocumentForSend(long documentId)
        {
            //Ако по изходящия документ има неприключили задачи за подпис - не се изпраща
            if (repo.AllReadonly<WorkTask>()
                            .Where(x => x.SourceType == SourceTypeSelectVM.Document && x.SourceId == documentId)
                            .Where(x => x.TaskTypeId == WorkTaskConstants.Types.Document_Sign)
                            .Where(x => WorkTaskConstants.States.NotFinished.Contains(x.TaskStateId))
                            .Any())
            {
                return false;
            }

            return true;
        }

        public bool AppendFile(CdnUploadRequest model, EpepConstants.ServiceMethod method)
        {
            int fileId = model.MongoFileId;

            if (fileId == 0)
            {
                fileId = repo.AllReadonly<MongoFile>()
                                .Where(x => x.FileId == model.FileId)
                                .Select(x => x.Id)
                                .FirstOrDefault();
            }
            switch (model.SourceType)
            {
                case SourceTypeSelectVM.Document:
                case SourceTypeSelectVM.DocumentPdf:
                case SourceTypeSelectVM.DocumentFileFromAPI:
                case SourceTypeSelectVM.DocumentFromElectronicDocument:
                    long docId = long.Parse(model.SourceId);
                    var _doc = repo.AllReadonly<Document>().Where(x => x.Id == docId)
                                    .Select(x => new
                                    {
                                        x.Id,
                                        x.DocumentTypeId,
                                        x.DocumentDirectionId,
                                        x.CourtId
                                    }).FirstOrDefault();

                    if (_doc.CourtId == NomenclatureConstants.Courts.RandomAssignment)
                    {
                        return true;
                    }

                    switch (_doc.DocumentDirectionId)
                    {
                        case DocumentConstants.DocumentDirection.Incoming:
                            var epepIn = new Integration.Epep.IncomingDocumentFile()
                            {
                                IncomingDocumentId = getKeyGUID(SourceTypeSelectVM.Document, _doc.Id) ?? Guid.Empty
                            };
                            initFromEpepModel(epepIn, SourceTypeSelectVM.Files, fileId, method, _doc.Id);

                            //Ако е съпровождащ документ към стартирало РНФЛ дело
                            int rnflCaseId = repo.AllReadonly<DocumentCaseInfo>()
                                                    .Where(x => x.DocumentId == docId)
                                                    .Where(x => x.CaseId > 0)
                                                    .Where(x => x.Case.IspnKind == NomenclatureConstants.IspnKinds.Rnfl && x.Case.TransferStartDate != null)
                                                    .Select(x => x.CaseId ?? 0)
                                                    .FirstOrDefault();

                            bool rnflInitialDoc = false;
                            if (rnflCaseId == 0)
                            {
                                //Ако е иницииращия документ на стартирало РНФЛ дело
                                rnflCaseId = repo.AllReadonly<Case>()
                                                        .Where(x => x.DocumentId == docId)
                                                        .Where(x => x.IspnKind == NomenclatureConstants.IspnKinds.Rnfl && x.TransferStartDate != null)
                                                        .Select(x => x.Id)
                                                        .FirstOrDefault();
                                rnflInitialDoc = rnflCaseId > 0;
                            }

                            if (rnflCaseId > 0)
                            {
                                string docTypeCode = getNomValue(RnflConstants.CodeMapping.DocumentTypes, _doc.DocumentTypeId);

                                if (!string.IsNullOrEmpty(docTypeCode))
                                {
                                    InitMQWithTarget(IntegrationTypes.Rnfl, RnflConstants.TargetMethods.DocumentFile, SourceTypeSelectVM.Files, fileId, method, docId);
                                }
                                else
                                {
                                    if (rnflInitialDoc)
                                    {
                                        InitMQWithTarget(IntegrationTypes.Rnfl, RnflConstants.TargetMethods.DocumentFile, SourceTypeSelectVM.Files, fileId, method, docId);
                                    }
                                }


                                int rnflAppealId = repo.AllReadonly<CaseSessionActComplain>()
                                                        .Where(x => x.ComplainDocumentId == docId)
                                                        .Where(x => x.CaseId == rnflCaseId)
                                                        .Where(x => x.DateExpired == null)
                                                        .Select(x => x.Id)
                                                        .FirstOrDefault();
                                if (rnflAppealId > 0)
                                {
                                    InitMQWithTarget(IntegrationTypes.Rnfl, RnflConstants.TargetMethods.AppealFile, SourceTypeSelectVM.Files, fileId, method, docId);
                                }
                            }

                            return true;
                        case DocumentConstants.DocumentDirection.OutGoing:
                            var epepOut = new Integration.Epep.OutgoingDocumentFile()
                            {
                                OutgoingDocumentId = getKeyGUID(SourceTypeSelectVM.Document, _doc.Id) ?? Guid.Empty
                            };
                            initFromEpepModel(epepOut, SourceTypeSelectVM.Files, fileId, method, _doc.Id);
                            //appendFile_ProcessOutgoingFile(_doc.Id);
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
                case SourceTypeSelectVM.CaseSessionActManualUpload:
                    try
                    {
                        int souceIdInt = int.Parse(model.SourceId);
                        if (checkSessionActCanSent(souceIdInt))
                        {
                            AppendAttachedDocument(SourceTypeSelectVM.Files, fileId, souceIdInt, method);
                        }
                    }
                    catch { }
                    return true;
                case SourceTypeSelectVM.CaseSessionFastDocument:
                    try
                    {
                        int souceIdInt = int.Parse(model.SourceId);
                        AppendAttachedDocument(SourceTypeSelectVM.Files, fileId, souceIdInt, method);
                    }
                    catch { }
                    return true;
                default:
                    return true;

            }
        }

        //private void appendFile_ProcessOutgoingFile(long docId)
        //{
        //    var docTemplate = repo.AllReadonly<DocumentTemplate>()
        //                                .Where(x => x.DocumentId == docId)
        //                                .Select(x => new
        //                                {
        //                                    x.SourceType,
        //                                    x.SourceId
        //                                })
        //                                .FirstOrDefault();
        //    if (docTemplate == null)
        //    {
        //        return;
        //    }
        //    switch (docTemplate.SourceType)
        //    {
        //        case SourceTypeSelectVM.CaseLawyerHelp:
        //            EESPP_LawyerHelp(ServiceMethod.Add, (int)docTemplate.SourceId);
        //            break;
        //        default:
        //            break;
        //    }
        //}

        public bool AppendCaseDataChange(int caseId)
        {
            var mq = new MQEpep()
            {
                MQId = mqID ?? Guid.NewGuid().ToString(),
                IntegrationTypeId = NomenclatureConstants.IntegrationTypes.EPEP,
                SourceType = SourceTypeSelectVM.Case,
                SourceId = caseId,
                //ParentSourceId = parentSourceId,
                TargetClassName = nameof(Integration.Epep.Case),
                //Content = System.Text.Encoding.UTF8.GetBytes(JsonTextSerializer.Serialize(epepModel)),
                DateWrt = DateTime.Now,
                UserId = userContext.UserId,
                MethodName = EpepConstants.Methods.DataChange,
                ErrorCount = 0,
                IntegrationStateId = IntegrationStates.New
            };

            repo.Add(mq);
            if (AUTO_SAVECHANGES)
            {
                repo.SaveChanges();
            }
            return true;
        }

        public async Task<bool> AppendCase(Case model, EpepConstants.ServiceMethod method)
        {
            var info = await repo.AllReadonly<Case>()
                                .Where(x => x.Id == model.Id)
                                .Select(x => new
                                {
                                    DocumentId = x.DocumentId,
                                    DocumentNumber = x.Document.DocumentNumber,
                                    ElectronicDocumentId = x.Document.ElectronicDocumentId,
                                    StateName = x.CaseState.Label,
                                    CaseCode = x.CaseCode.Code,
                                    IsRestricted = x.CaseClassifications.Any(c => c.DateTo == null && NomenclatureConstants.CaseClassifications.RestictedAccess.Contains(c.ClassificationId))
                                }).FirstOrDefaultAsync();
            var courtDepartment = await repo.AllReadonly<CaseLawUnit>()
                                    .Where(x => x.CaseId == model.Id && x.CaseSessionId == null)
                                    .Where(x => x.DateFrom <= DateTime.Now && (x.DateTo ?? DateTime.MaxValue) >= DateTime.Now)
                                    .Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                    .Select(x => x.CourtDepartment)
                                    .FirstOrDefaultAsync();



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
                    FormationDate = model.RegDate,
                    RestrictedAccess = info.IsRestricted
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
                    AUTO_SAVECHANGES = false;
                    foreach (var casePerson in model.CasePersons)
                    {
                        await AppendCasePerson(casePerson, EpepConstants.ServiceMethod.Add);
                    }
                    AUTO_SAVECHANGES = true;
                    if (model.CasePersons.Any())
                    {
                        await repo.SaveChangesAsync();
                    }

                    int elDocEpepUserId = 0;
                    if (info.ElectronicDocumentId > 0)
                    {
                        var elDocInfo = await repo.AllReadonly<ElectronicDocument>()
                                                    .Where(x => x.Id == info.ElectronicDocumentId.Value)
                                                    .Select(x => new
                                                    {
                                                        x.EpepUserId,
                                                        x.EpepUser.Uic,
                                                        x.EpepUser.LawyerNumber,
                                                        x.EpepUser.EpepUserTypeId,
                                                        x.RequestTypeCode,
                                                    })
                                                    .FirstOrDefaultAsync();

                        if (!string.IsNullOrEmpty(elDocInfo.RequestTypeCode))
                        {
                            var userAssignment = new EpepUserAssignment()
                            {
                                EpepUserId = elDocInfo.EpepUserId,
                                CourtId = model.CourtId,
                                CaseId = model.Id,
                                DateFrom = model.RegDate,
                                CanSummon = true,
                                //Ако има адвокатски номер на епеп потребителя, се прави достъп от тип адвокат
                                AssignmentRole = !string.IsNullOrEmpty(elDocInfo.LawyerNumber) ? EpepConstants.AssignmentRoles.Lawyer : EpepConstants.AssignmentRoles.Side
                            };
                            if (!string.IsNullOrEmpty(elDocInfo.Uic))
                            {
                                userAssignment.CasePersonId = model.CasePersons.Where(p => p.Uic == elDocInfo.Uic).Select(p => p.Id).FirstOrDefault();
                            }
                            if (userAssignment.CasePersonId == 0 && !string.IsNullOrEmpty(elDocInfo.LawyerNumber))
                            {
                                string[] lawyerNumbers = elDocInfo.LawyerNumber.Split(";", StringSplitOptions.RemoveEmptyEntries);
                                var lawyerId = await repo.AllReadonly<LawUnit>()
                                                        .Where(x => x.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Lawyer)
                                                        .Where(x => lawyerNumbers.Contains(x.Code))
                                                        .Where(x => x.DateTo == null)
                                                        .Select(x => x.Id)
                                                        .FirstOrDefaultAsync();

                                if (lawyerId > 0)
                                {
                                    userAssignment.CasePersonId = model.CasePersons
                                                                        .Where(p => p.Person_SourceType == SourceTypeSelectVM.LawUnit)
                                                                        .Where(p => p.Person_SourceId == lawyerId)
                                                                        .Select(p => p.Id)
                                                                        .FirstOrDefault();
                                }
                            }
                            if (userAssignment.CasePersonId > 0)
                            {
                                repo.Add(userAssignment);
                                await repo.SaveChangesAsync();
                                var logOper = new LogOperation()
                                {
                                    Controller = "epep",
                                    ActionName = "epepuserassignment_edit",
                                    ObjectKey = userAssignment.Id.ToString(),
                                    OperationTypeID = 1,
                                    OperationDate = model.RegDate,
                                    UserData = "Автоматично предоставен достъп",
                                    OperationUser = "ЕИСС"
                                };
                                repo.Add(logOper);
                                await repo.SaveChangesAsync();
                                AppendUserAssignment(userAssignment, EpepConstants.ServiceMethod.Add);

                                elDocEpepUserId = userAssignment.EpepUserId;
                            }
                        }


                    }
                    try
                    {
                        await AppendAutomaticEpepAccessForCase(model, elDocEpepUserId);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"AppendAutomaticEpepAccess; CaseId:{model.Id}");
                    }
                }
                if (ISPN_IsISPN(model) && !string.IsNullOrEmpty(model.RegNumber))
                {
                    ISPN_Case(model.Id, method);
                }

                await RNFL_SendCase(model.Id);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AppendCase");
                return false;
            }
        }

        /// <summary>
        /// Добавя автоматичен достъп до дело, в което има добавена прокуратура/фирма с ЕИК, съвпадащ с този на активен ЕПЕП потребител
        /// </summary>
        /// <param name="caseModel"></param>
        /// <param name="selectedEpepUserId">Id на ЕПЕП потребител, ако вече е добавен по ел.документ</param>
        /// <returns></returns>
        async Task AppendAutomaticEpepAccessForCase(Case caseModel, int? selectedEpepUserId)
        {
            var casePersonUics = await repo.AllReadonly<CasePerson>(x => x.CaseId == caseModel.Id && x.CaseSessionId == null)
                                            .Where(x => x.Uic != null)
                                            .Select(x => new
                                            {
                                                CasePersonId = x.Id,
                                                x.Uic
                                            }).ToListAsync();

            var uics = casePersonUics.Select(x => x.Uic).ToArray();
            var epepUser = await repo.AllReadonly<EpepUser>()
                                        .Where(x => EpepConstants.UserTypes.AutoEpepUserAccess.Contains(x.EpepUserTypeId) && uics.Contains(x.Uic))
                                        .Where(x => x.DateExpired == null)
                                        .Select(x => new
                                        {
                                            x.Id,
                                            x.Uic
                                        })
                                        .FirstOrDefaultAsync();

            if (epepUser == null || epepUser.Id == selectedEpepUserId)
            {
                return;
            }
            var userAssignment = new EpepUserAssignment()
            {
                EpepUserId = epepUser.Id,
                CourtId = caseModel.CourtId,
                CaseId = caseModel.Id,
                CasePersonId = casePersonUics.Where(x => x.Uic == epepUser.Uic).Select(x => x.CasePersonId).FirstOrDefault(),
                DateFrom = caseModel.RegDate,
                CanSummon = false,//За сега всички прокуратури не могат да бъдат призовани през ЕПЕП
                AssignmentRole = EpepConstants.AssignmentRoles.Side
            };
            repo.Add(userAssignment);
            await repo.SaveChangesAsync();
            var logOper = new LogOperation()
            {
                Controller = "epep",
                ActionName = "epepuserassignment_edit",
                ObjectKey = userAssignment.Id.ToString(),
                OperationTypeID = 1,
                OperationDate = caseModel.RegDate,
                UserData = "Автоматично предоставен достъп",
                OperationUser = "ЕИСС"
            };
            repo.Add(logOper);
            await repo.SaveChangesAsync();
            AppendUserAssignment(userAssignment, EpepConstants.ServiceMethod.Add);
        }

        /// <summary>
        /// Добавя автоматичен достъп до дело, в което има добавена прокуратура/фирма с ЕИК, съвпадащ с този на активен ЕПЕП потребител
        /// </summary>
        /// <param name="personModel"></param>
        /// <returns></returns>
        public async Task AppendAutomaticEpepAccessForPerson(CasePerson personModel)
        {
            if (string.IsNullOrEmpty(personModel.Uic))
            {
                return;
            }
            var epepUser = await repo.AllReadonly<EpepUser>()
                                        .Where(x => EpepConstants.UserTypes.AutoEpepUserAccess.Contains(x.EpepUserTypeId)
                                                && x.Uic == personModel.Uic)
                                        .Where(x => x.DateExpired == null)
                                        .Select(x => new
                                        {
                                            x.Id,
                                            x.Uic
                                        })
                                        .FirstOrDefaultAsync();

            if (epepUser == null)
            {
                return;
            }
            var caseInfo = await repo.AllReadonly<Case>()
                                    .Where(x => x.Id == personModel.CaseId)
                                    .Select(x => new
                                    {
                                        x.CourtId,
                                        x.Id
                                    }).FirstOrDefaultAsync();

            var userAssignment = new EpepUserAssignment()
            {
                EpepUserId = epepUser.Id,
                CourtId = caseInfo.CourtId,
                CaseId = caseInfo.Id,
                CasePersonId = personModel.Id,
                DateFrom = personModel.DateFrom,
                CanSummon = false,//За сега всички прокуратури не могат да бъдат призовани през ЕПЕП
                AssignmentRole = EpepConstants.AssignmentRoles.Side
            };
            repo.Add(userAssignment);
            await repo.SaveChangesAsync();
            var logOper = new LogOperation()
            {
                Controller = "epep",
                ActionName = "epepuserassignment_edit",
                ObjectKey = userAssignment.Id.ToString(),
                OperationTypeID = 1,
                OperationDate = personModel.DateFrom,
                UserData = $"Автоматично предоставен достъп",
                OperationUser = "ЕИСС"
            };
            repo.Add(logOper);
            await repo.SaveChangesAsync();
            AppendUserAssignment(userAssignment, EpepConstants.ServiceMethod.Add);
        }

        public async Task<bool> AppendCasePerson(CasePerson model, EpepConstants.ServiceMethod method)
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

                await RNFL_SendSide(model.Id, model.CaseId);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AppendCasePerson");
                return false;
            }
        }

        public bool AppendCaseSelectionProtocol(CaseSelectionProtokol model, EpepConstants.ServiceMethod method, string assignorName = null)
        {
            var info = repo.AllReadonly<CaseSelectionProtokol>()
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
                    Assignor = assignorName ?? info.ProtocolUserName
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

        public bool AppendCaseSelectionSubstitution(int caseSelectionSubstitutionId)
        {
            var info = repo.AllReadonly<CaseSelectionSubstitution>()
                               .Where(x => x.Id == caseSelectionSubstitutionId)
                               .Select(x => new
                               {
                                   x.Id,
                                   DocumentId = x.Case.DocumentId,
                                   CaseId = x.CaseId,
                                   JudgeName = x.CaseSelectionProtokolSubstitution.SelectedLawUnit.FullName,
                                   x.CaseSelectionProtokolSubstitution.SelectionDate,
                                   ProtocolUserName = x.CaseSelectionProtokolSubstitution.User.LawUnit.FullName,
                                   ProtocolId = x.CaseSelectionProtokolSubstitutionId
                               }).FirstOrDefault();
            try
            {
                var epep = new Integration.Epep.Assignment()
                {
                    CaseId = getKeyGUID(SourceTypeSelectVM.Case, info.CaseId) ?? Guid.Empty,
                    IncomingDocumentId = getKeyGUID(SourceTypeSelectVM.Document, info.DocumentId) ?? Guid.Empty,
                    Type = "Заместване",
                    Date = info.SelectionDate,
                    JudgeName = info.JudgeName,
                    Assignor = info.ProtocolUserName
                };

                initFromEpepModel(epep, SourceTypeSelectVM.CaseSelectionSubstitution, info.Id, EpepConstants.ServiceMethod.Add, info.CaseId);


                var epepFile = new Integration.Epep.AssignmentFile()
                {
                    AssignmentId = Guid.Empty
                };
                initFromEpepModel(epepFile, SourceTypeSelectVM.CaseSelectionProtokolSubstitution, info.ProtocolId, EpepConstants.ServiceMethod.Add, info.Id);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AppendCaseSelectionSubstitution");
                return false;
            }
        }

        public bool AppendCaseSession(CaseSession model, EpepConstants.ServiceMethod method)
        {
            var info = repo.AllReadonly<CaseSession>()
                                .Where(x => x.Id == model.Id)
                                .Select(x => new
                                {
                                    SessionTypeName = (x.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession) ? "Открито" : "Закрито",
                                    HallName = (x.CourtHallId != null) ? $"{x.CourtHall.Name} {x.CourtHall.Location}" : "",
                                    SessionState = x.SessionState.Label,
                                    x.VideoUrl,
                                    x.SessionStateId
                                }).FirstOrDefault();
            var sessionResult = repo.AllReadonly<Infrastructure.Data.Models.Cases.CaseSessionResult>()
                                        .Where(x => x.CaseSessionId == model.Id && x.IsMain && x.IsActive)
                                        .Select(x => x.SessionResult.Label)
                                        .FirstOrDefault();

            var prosecutorName = repo.AllReadonly<CasePerson>()
                                        .Where(x => x.CaseSessionId == model.Id)
                                        .Where(x => (x.DateTo ?? DateTime.MaxValue) >= model.DateFrom)
                                        .Where(x => x.PersonRoleId == NomenclatureConstants.PersonRole.Prokuror)
                                        .Where(x => x.DateExpired == null)
                                        .FirstOrDefault()?.FullName_MiddleNameInitials;

            var secretaryName = repo.AllReadonly<CaseLawUnit>()
                                       .Where(x => x.CaseSessionId == model.Id)
                                       .Where(x => (x.DateTo ?? DateTime.MaxValue) >= model.DateFrom)
                                       .Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.Secretary)
                                       .Select(x => x.LawUnit)
                                       .FirstOrDefault()?.FullName_MiddleNameInitials;

            try
            {
                var epep = new Integration.Epep.Hearing()
                {
                    HearingId = getKeyGUID(SourceTypeSelectVM.CaseSession, model.Id) ?? Guid.Empty,
                    CaseId = getKeyGUID(SourceTypeSelectVM.Case, model.CaseId) ?? Guid.Empty,
                    HearingType = info.SessionTypeName,
                    CourtRoom = info.HallName,
                    ProsecutorName = prosecutorName,
                    SecretaryName = secretaryName,
                    Date = model.DateFrom,
                    HearingResult = sessionResult,
                    VideoUrl = info.VideoUrl,
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

        public bool AppendCaseSessionFastDocument(CaseSessionFastDocument model, EpepConstants.ServiceMethod method)
        {
            var info = repo.AllReadonly<CaseSessionFastDocument>()

                                .Where(x => x.Id == model.Id)
                                .Select(x => new
                                {
                                    HearingDocumentKind = x.SessionDocType.Label
                                }).FirstOrDefault();

            try
            {
                var epep = new Integration.Epep.HearingDocument()
                {
                    HearingDocumentKind = info.HearingDocumentKind
                };

                initFromEpepModel(epep, SourceTypeSelectVM.CaseSessionFastDocument, model.Id, method, model.CaseSessionId);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AppendCaseSessionFastDocument");
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
                    CaseId = getKeyGUID(SourceTypeSelectVM.Case, info.CaseId) ?? Guid.Empty,
                    JudgeName = info.JudgeName,
                    DateAssigned = info.DateFrom
                };

                if (method == EpepConstants.ServiceMethod.Update || method == EpepConstants.ServiceMethod.Delete)
                {
                    epep.ReporterId = getKeyGUID(SourceTypeSelectVM.CaseReporter, info.CaseId) ?? Guid.Empty;
                }



                initFromEpepModel(epep, SourceTypeSelectVM.CaseReporter, info.CaseId, method, info.CaseId);
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

        public bool AppendCaseNotification(CaseNotification model, EpepSummonInfoVM notificationInfo, ServiceMethod method)
        {
            //Изпращат се само призовки за страни, с начин на доставка през ЕПЕП
            if (model.CasePersonId == null || notificationInfo == null)
            {
                return false;
            }

            try
            {
                var _info = repo.AllReadonly<CaseNotification>()
                                        .Include(x => x.HtmlTemplate)
                                        .Where(x => x.Id == model.Id)
                                        .Select(x => new
                                        {
                                            BlankName = x.HtmlTemplate.Label,
                                            DeliveryGroupId = x.NotificationDeliveryGroupId,
                                            DeliveryGroupLabel = (x.NotificationDeliveryGroupId > 0) ? x.GetNotificationDeliveryGroup.Label : "",
                                            Description = x.Description,
                                            x.CaseSessionActId,
                                            ComplainDocumentId = x.CaseNotificationComplains.Select(x => x.CaseSessionActComplain.ComplainDocumentId).FirstOrDefault(),
                                        }).FirstOrDefault();

                var epep = new Integration.Epep.Summon()
                {
                    SummonId = getKeyGUID(SourceTypeSelectVM.CaseNotification, model.Id),
                    SummonTypeCode = SummonTypeCode_CaseSession,//Призовка
                    SummonKind = _info.BlankName,//Не е ясно
                    SideId = getKeyGUID(SourceTypeSelectVM.CasePerson, notificationInfo.CasePersonId) ?? Guid.Empty,
                    Addressee = notificationInfo.AddresseeName,
                    //коригирано 29.09.2021, Пращаше се description - при над 500 символа - неиздентифицирана грешка
                    Subject = _info.BlankName,
                    DateCreated = model.RegDate,
                    Number = model.RegNumber
                };

                if ((_info.DeliveryGroupId ?? NomenclatureConstants.NotificationDeliveryGroup.ByEPEP) != NomenclatureConstants.NotificationDeliveryGroup.ByEPEP)
                {
                    epep.SummonKind += $" - {_info.DeliveryGroupLabel}";

                    if (!userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.SendPаperNotifications))
                    {
                        return false;
                    }
                }

                if (_info.ComplainDocumentId > 0)
                {
                    epep.IncommingDocumentId = getKeyGUID(SourceTypeSelectVM.Document, _info.ComplainDocumentId);
                }

                if (_info.CaseSessionActId > 0)
                {
                    epep.SummonTypeCode = SummonTypeCode_CasesessionAct;
                    epep.ParentId = getKeyGUID(SourceTypeSelectVM.CaseSessionAct, _info.CaseSessionActId) ?? Guid.Empty;
                }

                if (method == EpepConstants.ServiceMethod.Add && epep.SummonId != null)
                {
                    method = EpepConstants.ServiceMethod.Update;
                }

                initFromEpepModel(epep, SourceTypeSelectVM.CaseNotification, model.Id, method, notificationInfo.EpepUserId);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AppendCaseNotification");
                return false;
            }
        }

        public bool AppendCaseNotificationFile(int caseNotificationId)
        {
            var epep = new Integration.Epep.SummonFile();
            initFromEpepModel(epep, SourceTypeSelectVM.CaseNotificationPrint, caseNotificationId, EpepConstants.ServiceMethod.Add, caseNotificationId);
            return true;
        }

        public bool AppendCaseNotificationSummonReport(int caseNotificationId)
        {
            if (!cdnService.Select(SourceTypeSelectVM.CaseNotificationReturn, caseNotificationId.ToString()).Any())
            {
                return false;
            }


            var epep = new Integration.Epep.SummonFile();
            initFromEpepModel(epep, SourceTypeSelectVM.CaseNotificationReturn, caseNotificationId, EpepConstants.ServiceMethod.Add, caseNotificationId, "SummonReport");
            return true;
        }

        public bool AppendCaseMigration(CaseMigration caseMigration)
        {
            var epep = new Integration.Epep.ConnectedCase();
            initFromEpepModel(epep, SourceTypeSelectVM.CaseMigration, caseMigration.Id, EpepConstants.ServiceMethod.Add, caseMigration.CaseId);
            return true;
        }

        public bool AppendCaseMigrationFull(int caseMigrationId)
        {
            CaseMigration caseMigration = repo.GetById<CaseMigration>(caseMigrationId);
            if ((caseMigration.SendToCourtId ?? 0) == 0)
            {
                return false;
            }

            var isInCount = getNomValue(EpepConstants.Nomenclatures.CaseMigrationCourts, caseMigration.SendToCourtId);
            if (isInCount == "yes")
            {
                return false;
            }

            var caseMigrationType = getNomValue(EpepConstants.Nomenclatures.CaseMigrationType, caseMigration.CaseMigrationTypeId);
            if (string.IsNullOrEmpty(caseMigrationType))
            {
                return false;
            }

            var mq = new MQEpep()
            {
                MQId = Guid.NewGuid().ToString(),
                IntegrationTypeId = NomenclatureConstants.IntegrationTypes.EPEP,
                SourceType = SourceTypeSelectVM.CaseMigrationRegistration,
                SourceId = caseMigration.Id,
                DateWrt = DateTime.Now,
                UserId = userContext.UserId,
                MethodName = EpepConstants.Methods.Add,
                ErrorCount = 0,
                IntegrationStateId = IntegrationStates.New,
                TargetClassName = nameof(Integration.Epep.CaseMigrationRegistration)
            };
            repo.Add(mq);
            return repo.SaveChanges() > 0;
        }

        bool checkSessionActCanSent(int actId)
        {
            var info = repo.AllReadonly<CaseSessionAct>()
                            .Where(x => x.Id == actId)
                            .Where(x => x.DateExpired == null)
                            .Where(x => x.ActDeclaredDate != null)
                            .Select(x => new
                            {
                                x.RegNumber,
                                x.CaseSession.SessionStateId,
                                x.ActTypeId
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
            //Ако няма мапинг за вида акт, да не прави нова заявка за файл към него
            if (getNomValue(EpepConstants.Nomenclatures.ActTypes, info.ActTypeId) == null)
            {
                return false;
            }
            return true;
        }

        bool checkDocumentResolutionCanSent(long documentResolutionId)
        {
            var info = repo.AllReadonly<DocumentResolution>()
                            .Where(x => x.Id == documentResolutionId)
                            .Where(x => x.DateExpired == null)
                            .Where(x => x.DeclaredDate != null)
                            .Select(x => new
                            {
                                x.RegNumber,
                            }).FirstOrDefault();
            if (info == null)
            {
                return false;
            }
            if (string.IsNullOrEmpty(info.RegNumber))
            {
                return false;
            }

            return true;
        }

        public async Task<bool> AppendCaseSessionAct(CaseSessionAct model, ServiceMethod method)
        {
            if (!checkSessionActCanSent(model.Id))
            {
                return true;
            }
            try
            {
                var epep = new EpepRestModels.Act()
                {
                    ActId = getKeyGUID(SourceTypeSelectVM.CaseSessionAct, model.Id),
                    HearingId = getKeyGUID(SourceTypeSelectVM.CaseSession, model.CaseSessionId) ?? Guid.Empty,
                    Number = int.Parse(model.RegNumber),
                    //Finishing = model.IsFinalDoc,
                    //CanBeSubjectToAppeal = model.CanAppeal,
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

                if (method == ServiceMethod.Add)
                {
                    var manualFiles = cdnService.Select(SourceTypeSelectVM.CaseSessionActManualUpload, model.Id.ToString()).ToList();
                    foreach (var file in manualFiles)
                    {
                        AppendAttachedDocument(SourceTypeSelectVM.CaseSessionActManualUpload, file.MongoFileId, model.Id, method);
                    }
                }
                if (model.IsFinalDoc)
                {
                    LegalActs_SendAct(model.Id, ServiceMethod.Add);
                }

                await RNFL_SendAct(model.Id, model.CaseId ?? 0);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AppendCaseSessionAct");
                return false;
            }
        }

        public async Task<bool> AppendExecList(ExecList model, ServiceMethod method)
        {
            if ((model.GenerateExecProcess ?? false) == false)
            {
                return false;
            }
            try
            {
                var execListCaseId = await repo.AllReadonly<ExecListObligation>()
                                                .Where(x => x.ExecListId == model.Id)
                                                .Select(x => x.Obligation.CaseId)
                                                .FirstOrDefaultAsync() ?? 0;

                if (execListCaseId == 0)
                {
                    return false;
                }

                var caseRegDate = await GetPropByIdAsync<Case, DateTime>(execListCaseId, x => x.RegDate);
                if (!isNewZPCase(caseRegDate))
                {
                    return false;
                }



                var epep = new Integration.Epep.Act()
                {
                    ActId = getKeyGUID(SourceTypeSelectVM.ExecList, model.Id),
                    HearingId = null,
                    Number = int.Parse(model.RegNumber),
                    ActKindCode = "5014",
                    CaseId = getKeyGUID(SourceTypeSelectVM.Case, execListCaseId) ?? Guid.Empty,
                    DateInPower = model.DateSigned,
                    DateSigned = model.RegDate.Value
                };

                await initFromEpepModelAsync(epep, SourceTypeSelectVM.ExecList, model.Id, method, execListCaseId, "ExecList");

                initFromEpepModel(null, SourceTypeSelectVM.ExecListPdf, model.Id, method, null, nameof(SourceTypeSelectVM.ExecListPdf));

                await AppendExecProcess(0, model.Id);


                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AppendExecList");
                return false;
            }
        }

        public async Task<bool> AppendExecProcess(int caseSessionActId, int execListId)
        {

            DateTime caseRegDate = DateTime.MinValue;
            int sourceType = 0;
            int parentId = 0;

            int processKind = 0;
            if (execListId > 0)
            {
                var execListCaseId = await repo.AllReadonly<ExecList>()
                                                    .Where(x => x.Id == execListId)
                                                    .Select(x => x.CaseId)
                                                    .FirstOrDefaultAsync() ?? 0;
                if (execListCaseId == 0)
                {
                    return false;
                }
                caseRegDate = await GetPropByIdAsync<Case, DateTime>(execListCaseId, x => x.RegDate);
                sourceType = SourceTypeSelectVM.ExecList;
                parentId = execListId;
                processKind = EpepConstants.ExecProcessKinds.FromExecList;
            }

            if (caseSessionActId > 0)
            {
                var actCaseId = await GetPropByIdAsync<CaseSessionAct, int?>(caseSessionActId, x => x.CaseId);
                caseRegDate = await GetPropByIdAsync<Case, DateTime>(actCaseId ?? 0, x => x.RegDate);
                sourceType = SourceTypeSelectVM.CaseSessionAct;
                parentId = caseSessionActId;
                processKind = 2;
            }

            if (!isNewZPCase(caseRegDate))
            {
                return false;
            }


            try
            {
                var epep = new EpepRestModels.ExecProcess()
                {
                    CodeCreator = userContext.FullName,
                    ProcessKind = processKind
                };

                await initFromEpepModelAsync(epep, sourceType, 0, ServiceMethod.Add, parentId);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AppendExecProcess");
                return false;
            }
        }

        public async Task<SaveResultVM> AppendExecProcessAccess(int caseSessionActId, int execListId, Guid execProcessGid)
        {
            try
            {
                int parentId = caseSessionActId;
                int sourceType = SourceTypeSelectVM.ExecProcessCaseSessionAct;
                if (execListId > 0)
                {
                    parentId = execListId;
                    sourceType = SourceTypeSelectVM.ExecProcessExecList;
                }
                var epep = new EpepRestModels.ExecProcessAccessChange()
                {
                    UserName = userContext.FullName,
                    ExecProcessGid = execProcessGid
                };
                bool hasTask = await checkExistingEpepNewTask(epep.GetType().Name, sourceType, 0, EpepConstants.Methods.GetMethod(ServiceMethod.Add), parentId);
                if (hasTask)
                {
                    return new SaveResultVM(false, "Съществува нова задача за създаване на достъп. Моля изтеглете на ново данни за партида!");
                }

                await initFromEpepModelAsync(epep, sourceType, 0, ServiceMethod.Add, parentId);

                return new SaveResultVM(true, "Задачата за създаване на нов достъп е записана успешно.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, nameof(AppendExecProcessAccess));
                return new SaveResultVM(false);
            }
        }
        public async Task<SaveResultVM> DeleteExecProcessAccess(int caseSessionActId, int execListId, Guid accessGid)
        {
            try
            {
                int parentId = caseSessionActId;
                int sourceType = SourceTypeSelectVM.ExecProcessCaseSessionAct;
                if (execListId > 0)
                {
                    parentId = execListId;
                    sourceType = SourceTypeSelectVM.ExecProcessExecList;
                }
                var epep = new EpepRestModels.ExecProcessAccessChange()
                {
                    ExecProcessGid = getKeyGUID(sourceType, parentId) ?? Guid.NewGuid(),
                    UserName = userContext.FullName,
                    Gid = accessGid
                };
                bool hasTask = await checkExistingEpepNewTask(epep.GetType().Name, sourceType, 0, EpepConstants.Methods.GetMethod(ServiceMethod.Delete), parentId);
                if (hasTask)
                {
                    return new SaveResultVM(false, "Съществува нова задача за премахване на достъп. Моля изтеглете на ново данни за партида!");
                }

                await initFromEpepModelAsync(epep, sourceType, 0, ServiceMethod.Delete, parentId);

                return new SaveResultVM(true, "Задачата за премахване на достъп е записана успешно.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, nameof(DeleteExecProcessAccess));
                return new SaveResultVM(false);
            }
        }


        private bool isNewZPCase(DateTime regDate)
        {
            var newZPCaseRegDateFrom = repo.AllReadonly<Infrastructure.Data.Models.Nomenclatures.SystemParam>()
                                            .Where(x => x.ParamName == NomenclatureConstants.SystemParamName.ZP_StartRegDate)
                                            .Select(x => x.ParamValue)
                                            .FirstOrDefault() ?? "01.07.2025";
            try
            {
                DateTime date;
                if (DateTime.TryParseExact(newZPCaseRegDateFrom, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date))
                {
                    return regDate > date;
                }
            }
            catch (Exception)
            {

            }

            return false;

        }


        public bool AppendDocumentResolution(DocumentResolution model, ServiceMethod method)
        {
            if (!checkDocumentResolutionCanSent(model.Id))
            {
                return true;
            }
            var resolutionInfo = repo.AllReadonly<DocumentResolution>()
                                        .Where(x => x.Id == model.Id)
                                        .Select(x => new
                                        {
                                            ResolutionTypeCode = x.ResolutionType.Code,
                                            InitCaseId = x.Document.Cases.Select(c => c.Id).FirstOrDefault(),
                                            CompliantCaseId = x.Document.DocumentCaseInfo.Select(x => x.CaseId).FirstOrDefault() ?? 0
                                        }).FirstOrDefault();

            if (resolutionInfo.InitCaseId == 0 && resolutionInfo.CompliantCaseId == 0)
            {
                return false;
            }

            var resolutionCaseId = resolutionInfo.InitCaseId;
            if (resolutionInfo.InitCaseId == 0)
            {
                resolutionCaseId = resolutionInfo.CompliantCaseId;
            }

            try
            {
                var epep = new Integration.Epep.Act()
                {
                    ActId = getKeyGUID(SourceTypeSelectVM.CaseSessionAct, model.Id),
                    CaseId = getKeyGUID(SourceTypeSelectVM.Case, resolutionCaseId) ?? Guid.Empty,
                    Number = int.Parse(model.RegNumber),
                    ActKindCode = resolutionInfo.ResolutionTypeCode,
                    DateInPower = model.DeclaredDate.Value,
                    DateSigned = model.DeclaredDate.Value
                };

                initFromEpepModel(epep, SourceTypeSelectVM.CaseSessionAct, model.Id, method, resolutionCaseId, "DocumentResolution");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AppendDocumentResolution");
                return false;
            }
        }

        public async Task<bool> AppendActsFromSession(int sessionId)
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

                await AppendCaseSessionAct(act, ServiceMethod.Add);
                await AppendCaseSessionAct_Public(act.Id, ServiceMethod.Add);
                var hasSignTasks = repo.AllReadonly<WorkTask>()
                                            .Where(x => x.SourceType == SourceTypeSelectVM.CaseSessionAct && x.SourceId == (long)act.Id)
                                            .Where(x => x.TaskTypeId == WorkTaskConstants.Types.CaseSessionAct_Sign)
                                            .Where(x => WorkTaskConstants.States.NotFinished.Contains(x.TaskStateId))
                                            .Any();
                if (!hasSignTasks)
                {
                    await AppendCaseSessionAct_Private(act.Id, ServiceMethod.Add);
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
        public async Task<bool> AppendCaseSessionAct_Private(int actId, ServiceMethod method)
        {
            if (!checkSessionActCanSent(actId))
            {
                return true;
            }
            var actFile = cdnService.Select(SourceTypeSelectVM.CaseSessionActPdf, actId.ToString()).FirstOrDefault();
            if (actFile != null || method == ServiceMethod.Delete)
            {
                var epep = new Integration.Epep.PrivateActFile()
                {
                    PrivateActFileId = getKeyGUID(SourceTypeSelectVM.CaseSessionActPdf, actId),
                    ActId = getKeyGUID(SourceTypeSelectVM.CaseSessionAct, actId) ?? Guid.Empty
                };
                initFromEpepModel(epep, SourceTypeSelectVM.CaseSessionActPdf, actId, method, actId);

                await RNFL_SendActPrivateFile(actId);

                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> AppendCaseSessionAct_Public(int actId, ServiceMethod method)
        {
            if (!checkSessionActCanSent(actId))
            {
                return true;
            }
            var actFile = cdnService.Select(SourceTypeSelectVM.CaseSessionActDepersonalized, actId.ToString()).FirstOrDefault();
            if (actFile == null && method == ServiceMethod.Add)
            {
                return false;
            }
            var epep = new Integration.Epep.PublicActFile()
            {
                PublicActFileId = getKeyGUID(SourceTypeSelectVM.CaseSessionActDepersonalized, actId),
                ActId = getKeyGUID(SourceTypeSelectVM.CaseSessionAct, actId) ?? Guid.Empty
            };
            initFromEpepModel(epep, SourceTypeSelectVM.CaseSessionActDepersonalized, actId, method, actId);
            var actModel = await GetReadonlyAsync<CaseSessionAct>(actId);
            if (actModel.IsFinalDoc)
            {
                LegalActs_SendAct(actId, ServiceMethod.Add);
            }
            if (method == ServiceMethod.Add)
            {
                EPRO_AppendActFile(actModel);
                EESPP_AppendLawyerAssignmentByAct(actId);
            }
            Elastic_AppendActFile(actModel, method);
            if (ISPN_IsISPN(null, actModel.CaseId ?? 0))
            {
                ISPN_CaseSessionAct(actModel.Id, method, actModel.CaseId ?? 0);
            }
            await RNFL_SendActPublicFile(actId);
            return true;

        }

        public bool AppendCaseSessionAct_PrivateMotive(int actId, ServiceMethod method)
        {
            if (!checkSessionActCanSent(actId))
            {
                return true;
            }
            var actFile = cdnService.Select(SourceTypeSelectVM.CaseSessionActMotivePdf, actId.ToString()).FirstOrDefault();
            if (actFile != null || method == ServiceMethod.Delete)
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
            if (actFile == null && method == ServiceMethod.Add)
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
                LegalActs_SendAct(actId, ServiceMethod.Add);
            }
            return true;

        }

        public bool AppendAttachedDocument(int sourceType, int sourceId, long parentId, ServiceMethod method)
        {
            //if (!checkSessionActCanSent(actId))
            //{
            //    return true;
            //}
            //var actFile = cdnService.Select(SourceTypeSelectVM.CaseSessionActMotiveDepersonalized, actId.ToString()).FirstOrDefault();
            //if (actFile == null)
            //{
            //    return false;
            //}
            var epep = new Integration.Epep.AttachedDocument();

            initFromEpepModel(epep, sourceType, sourceId, method, parentId);
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

        //public bool AppendPersonAssignment(EpepUserAssignment model, EpepConstants.ServiceMethod method)
        //{

        //    if (model != null)
        //    {
        //        var epep = new Integration.Epep.PersonAssignment()
        //        {
        //            Date = model.DateFrom,
        //            IsActive = model.DescriptionExpired == null,
        //            PersonRegistrationId = getKeyGUID(SourceTypeSelectVM.EpepUser, model.EpepUserId) ?? Guid.Empty,
        //            SideId = getKeyGUID(SourceTypeSelectVM.CasePerson, model.CasePersonId) ?? Guid.Empty
        //        };
        //        initFromEpepModel(epep, SourceTypeSelectVM.EpepUserAssignment, model.Id, method, model.EpepUserId);
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}
        //public bool AppendLawyerAssignment(EpepUserAssignment model, EpepConstants.ServiceMethod method)
        //{
        //    if (model != null)
        //    {
        //        var epep = new Integration.Epep.LawyerAssignment()
        //        {
        //            Date = model.DateFrom,
        //            IsActive = model.DescriptionExpired == null,
        //            LawyerRegistrationId = getKeyGUID(SourceTypeSelectVM.EpepUser, model.EpepUserId) ?? Guid.Empty,
        //            SideId = getKeyGUID(SourceTypeSelectVM.CasePerson, model.CasePersonId) ?? Guid.Empty
        //        };
        //        initFromEpepModel(epep, SourceTypeSelectVM.EpepUserAssignment, model.Id, method, model.EpepUserId);
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}
        public bool AppendUserAssignment(EpepUserAssignment model, EpepConstants.ServiceMethod method)
        {

            if (model != null)
            {
                var epep = new Integration.Epep.UserAssignment()
                {
                    Date = model.DateFrom,
                    IsActive = model.DescriptionExpired == null,
                    AssignmentRole = model.AssignmentRole ?? 0
                    //Четат се при изпращането
                    //UserAssignmentId = getKeyGUID(SourceTypeSelectVM.EpepUser, model.EpepUserId) ?? Guid.Empty,
                    //SideId = getKeyGUID(SourceTypeSelectVM.CasePerson, model.CasePersonId) ?? Guid.Empty
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
                return AppendUserAssignment(model, method);
            }
            else
            {
                return false;
            }
        }


        public IQueryable<EpepUserVM> EpepUser_Select(EpepUserFilterVM filter)
        {
            filter.UpdateNullables();
            Expression<Func<EpepUser, bool>> whereType = x => true;
            if (filter.EpepUserTypeId > 0)
            {
                whereType = x => x.EpepUserTypeId == filter.EpepUserTypeId;
            }
            Expression<Func<EpepUser, bool>> whereNumber = x => true;
            if (!string.IsNullOrEmpty(filter.PersonNumber))
            {
                whereNumber = x => x.Uic == filter.PersonNumber || EF.Functions.ILike(x.LawyerNumber, filter.PersonNumber.ToPaternSearch());
            }
            Expression<Func<EpepUser, bool>> whereName = x => true;
            if (!string.IsNullOrEmpty(filter.FullName))
            {
                whereName = x => EF.Functions.ILike(x.FullName, filter.FullName.ToPaternSearch());
            }
            Expression<Func<EpepUser, bool>> whereEmail = x => true;
            if (!string.IsNullOrEmpty(filter.Email))
            {
                whereEmail = x => EF.Functions.ILike(x.Email, filter.Email.ToPaternSearch());
            }
            return repo.AllReadonly<EpepUser>()
                            .Where(FilterExpireInfo<EpepUser>(false))
                            .Where(whereType)
                            .Where(whereNumber)
                            .Where(whereName)
                            .Where(whereEmail)
                            .OrderBy(x => x.FullName)
                            .Select(x => new EpepUserVM
                            {
                                Id = x.Id,
                                UserTypeName = x.EpepUserType.Label,
                                Email = x.Email,
                                EpepUserTypeId = x.EpepUserTypeId,
                                FullName = x.FullName,
                                LawyerNumber = x.LawyerNumber,
                                EGN = x.Uic
                            });
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

            model.Email = model.Email.ToLower().Trim();

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
                                    .Where(x => x.EpepUserId == epepUserId)
                                    .Where(FilterExpireInfo<EpepUserAssignment>(false))
                                    .OrderBy(x => x.Court.Label)
                                    .ThenByDescending(x => x.Case.RegDate)
                                    .Select(x => new EpepUserAssignmentVM
                                    {
                                        Id = x.Id,
                                        CaseId = x.CaseId,
                                        CanChange = (x.CourtId == userContext.CourtId),
                                        CourtName = x.Court.Label,
                                        CaseInfo = $"{x.Case.CaseType.Code} {x.Case.RegNumber}",
                                        CaseNumber = x.Case.RegNumber,
                                        SideInfo = $"{x.CasePerson.FullName} ({x.CasePerson.PersonRole.Label})",
                                        SideName = x.CasePerson.FullName,
                                        CanSummon = x.CanSummon ?? false,
                                        IsLawyer = x.AssignmentRole == EpepConstants.AssignmentRoles.Lawyer
                                    }).AsQueryable();
        }

        public bool EpepUserAssignment_SaveData(EpepUserAssignment model)
        {
            var casePerson = repo.GetById<CasePerson>(model.CasePersonId);
            model.DateFrom = casePerson.DateFrom;
            if (model.Id > 0)
            {
                var saved = repo.GetById<EpepUserAssignment>(model.Id);
                saved.CourtId = model.CourtId;
                saved.CaseId = model.CaseId;
                saved.CasePersonId = model.CasePersonId;
                saved.AssignmentRole = model.AssignmentRole;
                saved.DateFrom = model.DateFrom;
                saved.CanSummon = model.CanSummon;

                SetUserDateWRT(saved);
                repo.SaveChanges();
            }
            else
            {
                SetUserDateWRT(model);
                repo.Add(model);
                repo.SaveChanges();
            }

            AppendEpepUserAssignment(model, EpepConstants.ServiceMethod.Add);
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
            return repo.AllReadonly<Case>()
                            .Where(x => x.Id == caseId)
                            .Select(x => x.IsISPNcase)
                            .FirstOrDefault() ?? false;
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
                        .Where(x => !EF.Functions.ILike(x.OuterCode, "DEL%") && !EF.Functions.ILike(x.OuterCode, "ERR%"))
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
            repo.SaveChanges();

            return true;
        }

        public List<MQEpepVM> MQEpep_SelectISPN(long sourceId)
        {
            var result = repo.AllReadonly<MQEpep>()
                                .Where(x => x.ParentSourceId == sourceId)
                                .Where(x => x.IntegrationTypeId == IntegrationTypes.ISPN)
                                .OrderBy(x => x.Id)
                                .Select(x => new MQEpepVM
                                {
                                    Id = x.Id,
                                    StateId = x.IntegrationStateId,
                                    MethodName = x.MethodName,
                                    //OperName = (x.MethodName == "add") ? "Добавяне" : "Редакция",
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
        public async Task<List<MQEpepVM>> MQEpep_Select(int integrationType, int sourceType, long sourceId)
        {
            if (integrationType == IntegrationTypes.ISPN)
                return MQEpep_SelectISPN(sourceId);

            Expression<Func<MQEpep, bool>> whereClause = x => x.SourceType == sourceType && x.SourceId == sourceId;


            if (sourceType == SourceTypeSelectVM.DocumentFiles)
            {
                int[] fileTypes = { SourceTypeSelectVM.Files, SourceTypeSelectVM.AttachedDocumentFiles };
                whereClause = x => fileTypes.Contains(x.SourceType) && x.ParentSourceId == sourceId;
            }

            return await repo.AllReadonly<MQEpep>()
                                .Where(whereClause)
                                .Where(x => x.IntegrationTypeId == integrationType)
                                .OrderBy(x => x.Id)
                                .Select(x => new MQEpepVM
                                {
                                    Id = x.Id,
                                    StateId = x.IntegrationStateId,
                                    MethodName = x.MethodName,
                                    //OperName = (x.MethodName == "add") ? "Добавяне" : "Редакция",
                                    DateWrt = x.DateWrt,
                                    DateTransfered = x.DateTransfered,
                                    ErrorDescription = x.ErrorDescription
                                }).ToListAsync();
        }

        public async Task<string> RecoverData(object client)
        {
            var ids = repo.All<ID_List>().ToList();
            int saved = 0;
            //this.mqID = "scanedFiles";
            //var epepClient = (Integration.Epep.IeCaseServiceClient)client;
            foreach (var item in ids)
            {
                if (saved % 500 == 0)
                {
                    repo.RefreshDbContext(configuration.GetConnectionString("DefaultConnection"));
                }
                saved++;

                await ResendDataToEPEP(SourceTypeSelectVM.Document, item.Id, false);

            }

            return $"Saved {saved}/{ids.Count} items.";
        }

        public EpepUser EpepUser_GetByDocument(long documentId)
        {
            EpepUser result = null;

            result = repo.AllReadonly<EpepUser>()
                                .Where(x => x.DocumentId == documentId)
                                .Where(FilterExpireInfo<EpepUser>(false))
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
                var lawUnit = repo.AllReadonly<LawUnit>()
                                    .Where(x => x.Id == docPerson.Person_SourceId)
                                    .Select(x => new
                                    {
                                        x.Code,
                                        x.Uic,
                                        x.LawUnitTypeId
                                    })
                                    .FirstOrDefault();
                if (lawUnit == null)
                {
                    return null;
                }
                if (lawUnit.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Lawyer && !string.IsNullOrEmpty(lawUnit.Code))
                {
                    return repo.AllReadonly<EpepUser>()
                                       .Where(x => x.LawyerNumber == lawUnit.Code.Trim())
                                       .Where(FilterExpireInfo<EpepUser>(false))
                                       .FirstOrDefault();
                }
                else
                {
                    if (!string.IsNullOrEmpty(lawUnit.Uic))
                    {
                        return repo.AllReadonly<EpepUser>()
                                      .Where(x => x.Uic == lawUnit.Uic)
                                      .Where(FilterExpireInfo<EpepUser>(false))
                                      .FirstOrDefault();
                    }
                }
            }
            if (!string.IsNullOrEmpty(docPerson.Uic))
            {
                return repo.AllReadonly<EpepUser>()
                              .Where(x => x.Uic == docPerson.Uic)
                              .Where(FilterExpireInfo<EpepUser>(false))
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

        public async Task ResendDataToEPEP(int sourceType, long sourceId, bool appendChild)
        {
            switch (sourceType)
            {
                case SourceTypeSelectVM.Document:
                    {
                        var model = repo.AllReadonly<Document>()
                                            .Include(x => x.DocumentCaseInfo)
                                            .Include(x => x.DocumentPersons)
                                            .Where(x => x.Id == sourceId)
                                            .FirstOrDefault();
                        if (model != null)
                        {
                            await AppendDocument(model, ServiceMethod.Add);
                            //ResendDataToEPEP_Files(sourceType, sourceId);
                        }
                    }
                    break;
                case SourceTypeSelectVM.Case:
                    {
                        var model = repo.GetById<Case>((int)sourceId);
                        if (model != null)
                        {
                            await AppendCase(model, ServiceMethod.Add);

                            if (appendChild)
                            {
                                var sessions = repo.AllReadonly<CaseSession>()
                                                    .Where(FilterExpireInfo<CaseSession>(false))
                                                    .Where(x => x.CaseId == model.Id)
                                                    .Select(x => x.Id)
                                                    .ToList();

                                foreach (var item in sessions)
                                {
                                    await ResendDataToEPEP(SourceTypeSelectVM.CaseSession, item, appendChild);
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
                                    await ResendDataToEPEP(SourceTypeSelectVM.CaseSessionAct, item, appendChild);
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
                            await AppendCaseSessionAct(model, ServiceMethod.Add);
                            if (model.ActDeclaredDate != null)
                            {
                                await AppendCaseSessionAct_Private(model.Id, ServiceMethod.Add);
                            }
                            if (model.DepersonalizeEndDate != null)
                            {
                                await AppendCaseSessionAct_Public(model.Id, ServiceMethod.Add);
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

        public async Task MQEpep_ResetError(int integrationType, int sourceType, long sourceId)
        {
            List<MQEpep> requests = await selectByIntSourceTypeId(integrationType, sourceType, sourceId, null);

            if (requests == null)
            {
                return;
            }

            if (resetMqStatus(requests) || sourceType == SourceTypeSelectVM.DocumentFiles)
            {
                if (integrationType == IntegrationTypes.EPEP)
                {
                    switch (sourceType)
                    {
                        case SourceTypeSelectVM.DocumentFiles:
                            var docFiles = await selectByIntSourceTypeId(integrationType, SourceTypeSelectVM.Files, null, sourceId);
                            docFiles.AddRange(await selectByIntSourceTypeId(integrationType, SourceTypeSelectVM.AttachedDocumentFiles, null, sourceId)); resetMqStatus(docFiles);
                            resetMqStatus(docFiles);
                            break;
                        case SourceTypeSelectVM.EpepUser:
                            var userAssignments = await selectByIntSourceTypeId(integrationType, SourceTypeSelectVM.EpepUserAssignment, null, sourceId);
                            resetMqStatus(userAssignments);
                            break;
                    }
                }

                repo.SaveChanges();
            }
        }

        private bool resetMqStatus(List<MQEpep> requests)
        {
            bool forSavechanges = false;

            foreach (var item in requests)
            {
                if (IntegrationStates.ResetMQErrorStates.Contains(item.IntegrationStateId ?? 0))
                {
                    item.ErrorDescription = $"MQreset:{DateTime.Now:dd.MM HH:mm:ss}";
                    item.ErrorCount = 0;
                    item.IntegrationStateId = IntegrationStates.New;
                    forSavechanges = true;
                }

            }
            return forSavechanges;
        }

        private async Task<List<MQEpep>> selectByIntSourceTypeId(int integrationType, int sourceType, long? sourceId, long? parentSourceId)
        {
            List<MQEpep> requests;
            if (integrationType == IntegrationTypes.ISPN)
            {
                requests = await repo.All<MQEpep>()
                                .Where(x => x.SourceType == sourceType && x.ParentSourceId == sourceId)
                                .Where(x => x.IntegrationTypeId == integrationType)
                                .OrderBy(x => x.Id)
                                .ToListAsync();
            }
            else
            {
                Expression<Func<MQEpep, bool>> whereSourceId = x => true;
                if (sourceId > 0)
                {
                    whereSourceId = x => x.SourceId == sourceId.Value;
                }
                Expression<Func<MQEpep, bool>> whereParentSourceId = x => true;
                if (parentSourceId > 0)
                {
                    whereParentSourceId = x => x.ParentSourceId == parentSourceId.Value;
                }

                requests = await repo.All<MQEpep>()
                               .Where(x => x.SourceType == sourceType)
                               .Where(x => x.IntegrationTypeId == integrationType)
                               .Where(whereSourceId)
                               .Where(whereParentSourceId)
                               .OrderBy(x => x.Id)
                               .ToListAsync();
            }
            return requests;
        }

        public LawUnit GetLawyerByNumber(string lawyerNumber)
        {
            return repo.All<LawUnit>()
                            .Where(x => x.LawUnitTypeId == LawUnitTypes.Lawyer && x.Code == lawyerNumber && x.DateTo == null)
                            .FirstOrDefault();
        }

        public void EPRO_AppendDismissal(int caseDismissalId, int dismissalTypeId)
        {
            //В ЕПРО се изпращат само Отвод и самоотвод
            if (!NomenclatureConstants.DismisalType.EproDismissalTypes.Contains(dismissalTypeId))
            {
                return;
            }
            InitMQ(NomenclatureConstants.IntegrationTypes.EPRO, SourceTypeSelectVM.CaseLawUnitDismisal, caseDismissalId, EpepConstants.ServiceMethod.Add);
            var dismissal = repo.GetById<CaseLawUnitDismisal>(caseDismissalId);
            if (dismissal != null && dismissal.CaseSessionActId > 0)
            {
                var publicActFile = cdnService.Select(SourceTypeSelectVM.CaseSessionActDepersonalized, dismissal.CaseSessionActId.ToString()).FirstOrDefault();
                if (publicActFile != null)
                {
                    var caseSessionAct = repo.GetById<CaseSessionAct>(dismissal.CaseSessionActId.Value);
                    EPRO_AppendActFile(caseSessionAct);
                }
            }
        }

        public void EPRO_AppendReplace(int caseSelectionProtocolId, int caseDismissalId)
        {

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

            var dismissalsByActId = repo.AllReadonly<CaseLawUnitDismisal>()
                                            .Where(x => x.CaseSessionActId == actModel.Id && x.CaseId == actModel.CaseId)
                                            .Select(x => x.Id)
                                            .ToList();
            foreach (var dismissalId in dismissalsByActId)
            {
                InitMQ(NomenclatureConstants.IntegrationTypes.EPRO, SourceTypeSelectVM.CaseSessionActDepersonalized, actModel.Id, EpepConstants.ServiceMethod.Add, dismissalId);
            }
        }
        public void CAIS_SendBulletin(ServiceMethod method, int bulletinFileId, int bulletinId)
        {
            InitMQ(IntegrationTypes.Cais, SourceTypeSelectVM.CasePersonBulletin, bulletinId, method, bulletinFileId);
        }
        public void EESPP_LawyerHelp(ServiceMethod method, int lawyerHelpId)
        {
            InitMQ(IntegrationTypes.Eespp, SourceTypeSelectVM.CaseLawyerHelp, lawyerHelpId, method);
        }
        public void EESPP_AppendLawyerAssignmentByAct(int caseSessionActId)
        {
            var lawyerAssignmentId = repo.AllReadonly<CaseLawyerHelpAssignedLawyer>()
                                            .Where(x => x.CaseSessionActAssignedId == caseSessionActId)
                                            .Select(x => x.Id)
                                            .FirstOrDefault();

            if (lawyerAssignmentId == 0)
            {
                return;
            }

            if (!repo.AllReadonly<MQEpep>()
                        .Where(x => x.IntegrationTypeId == NomenclatureConstants.IntegrationTypes.Eespp
                                && x.SourceType == SourceTypeSelectVM.CaseLawyerHelpAssignedLawyer
                                && x.SourceId == lawyerAssignmentId)
                        .Any())
            {
                EESPP_AppendLawyerAssignment(lawyerAssignmentId);
            }
        }
        public void EESPP_AppendLawyerAssignment(int lawyerAssignmentId)
        {
            InitMQ(NomenclatureConstants.IntegrationTypes.Eespp, SourceTypeSelectVM.CaseLawyerHelpAssignedLawyer, lawyerAssignmentId, EpepConstants.ServiceMethod.Add);
        }

        public void Elastic_AppendActFile(CaseSessionAct actModel, ServiceMethod method)
        {
            InitMQ(NomenclatureConstants.IntegrationTypes.ElasticService, SourceTypeSelectVM.CaseSessionAct, actModel.Id, method);
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

        public IQueryable<EpepUserAssignmentVM> EpepUserAssignments_SelectByCase(int caseId)
        {
            return repo.AllReadonly<EpepUserAssignment>()
                            .Where(x => x.CaseId == caseId)
                            .Where(FilterExpireInfo<EpepUserAssignment>(false))
                            .Select(x => new EpepUserAssignmentVM
                            {
                                Id = x.Id,
                                EpepUserFullName = x.EpepUser.FullName,
                                EpepUserInfo = $"{x.EpepUser.FullName} ({x.EpepUser.EpepUserType.Label})",
                                SideFullName = x.CasePerson.FullName,
                                SideInfo = $"{x.CasePerson.FullName} ({x.CasePerson.PersonRole.Label})",
                                CanSummon = x.CanSummon ?? false
                            }).AsQueryable();
        }

        public string NotMappedActs(int caseId)
        {
            var acts = repo.AllReadonly<CaseSessionAct>()
                           .Where(x => x.CaseId == caseId &&
                                       x.DateExpired == null &&
                                       x.ActDeclaredDate != null &&
                                       !NomenclatureConstants.ActType.AllowActTypesISPN.Contains(x.ActTypeId))
                           .Select(x => $"{x.ActType.Label} {x.RegNumber} {x.RegDate:dd.MM.yyyy}");
            return string.Join(", ", acts);
        }

        #endregion ИСПН


        public async Task<SummaryCaseInfoVM> LoadConnectedCase(int inMigrationId, bool refreshData = false)
        {
            var caseMigrationGid = getKeyGUID(SourceTypeSelectVM.CaseMigrationRegistration, inMigrationId);

            if (caseMigrationGid == null || caseMigrationGid == Guid.Empty)
            {
                return null;
            }

            var caseinfoFile = await cdnService.Select(SourceTypeSelectVM.CaseMigrationRegistration, inMigrationId.ToString()).FirstOrDefaultAsync().ConfigureAwait(false);

            if (refreshData || caseinfoFile == null)
            {

                try
                {
                    var migrationResult = await proxyEissService.EpepGetResultCaseMigration(caseMigrationGid.Value);
                    if (migrationResult == null)
                    {
                        return null;
                    }
                    var summaryCase = await proxyEissService.EpepGetSummaryCase(migrationResult.CaseId.Value);
                    if (summaryCase == null)
                    {
                        return null;
                    }
                    var infoString = JsonTextSerializer.Serialize(summaryCase);
                    var uploadOk = await cdnService.MongoCdn_AppendUpdate(new CdnUploadRequest()
                    {
                        SourceType = SourceTypeSelectVM.CaseMigrationRegistration,
                        SourceId = inMigrationId.ToString(),
                        FileContentBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(infoString)),
                        UserUploaded = userContext.UserId,
                        FileName = $"summarycase-{migrationResult.CaseId.Value}.json"
                    });

                    return new SummaryCaseInfoVM()
                    {
                        Case = summaryCase,
                        DateWrt = DateTime.Now
                    };
                }
                catch (Exception ex)
                {
                    return null;
                }
            }

            var serializedSummaryCase = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { FileId = caseinfoFile.FileId });
            var summaryCaseCached = JsonTextSerializer.Deserialize<Integration.Epep.SummaryCase>(serializedSummaryCase);
            return new SummaryCaseInfoVM()
            {
                Case = summaryCaseCached,
                DateWrt = caseinfoFile.DateUploaded
            };

        }

        public async Task<bool> CheckForSavedDocumentRequestForCase(int caseSessionActId)
        {
            int caseId = await repo.GetPropByIdAsync<CaseSessionAct, int>(x => x.Id == caseSessionActId, x => x.CaseId ?? 0);
            return await repo.AllReadonly<DocumentRequestInfo>()
                                .Where(x => x.CaseId == caseId)
                                .AnyAsync();
        }

        public async Task<ExecProcessInfoVM> LoadExecProcess(int caseSessionActId, int execListId, bool refreshData = false)
        {
            int execProcessSourceType = 0;
            int sourceId = 0;
            if (caseSessionActId > 0)
            {
                execProcessSourceType = SourceTypeSelectVM.ExecProcessCaseSessionAct;
                sourceId = caseSessionActId;
            }
            if (execListId > 0)
            {
                execProcessSourceType = SourceTypeSelectVM.ExecProcessExecList;
                sourceId = execListId;
            }


            var execProcessGid = getKeyGUID(execProcessSourceType, sourceId);

            if (execProcessGid == null || execProcessGid == Guid.Empty)
            {
                return null;
            }

            var execProcessFile = await cdnService.Select(execProcessSourceType, sourceId.ToString()).FirstOrDefaultAsync();

            if (refreshData || execProcessFile == null)
            {

                try
                {
                    EpepRestModels.ExecProcessDetailsVM execProcessData = await proxyEissService.EpepGetExecProcess(execProcessGid.Value);
                    if (execProcessData == null)
                    {
                        return null;
                    }
                    var infoString = JsonTextSerializer.Serialize(execProcessData);
                    var uploadOk = await cdnService.MongoCdn_AppendUpdate(new CdnUploadRequest()
                    {
                        SourceType = execProcessSourceType,
                        SourceId = sourceId.ToString(),
                        FileContentBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(infoString)),
                        UserUploaded = userContext.UserId,
                        FileName = $"execprocess.json"
                    });

                    return new ExecProcessInfoVM()
                    {
                        Data = execProcessData,
                        DateWrt = DateTime.Now,
                        CaseSessionActId = caseSessionActId,
                        ExecListId = execListId
                    };
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"LoadExecProcess; acId:{caseSessionActId};execList:{execListId};refresh:{refreshData}");
                    return null;
                }
            }

            var serializedExecProcess = await cdnService.LoadHtmlFileTemplate(new CdnFileSelect() { FileId = execProcessFile.FileId });
            var execProcessCached = JsonTextSerializer.Deserialize<EpepRestModels.ExecProcessDetailsVM>(serializedExecProcess);
            return new ExecProcessInfoVM()
            {
                Data = execProcessCached,
                DateWrt = execProcessFile.DateUploaded,
                CaseSessionActId = caseSessionActId,
                ExecListId = execListId
            };

        }

        public async Task EissProcessStart(string processType, int sourceType, long sourceId, object processContext = null)
        {
            MQEpep process = new()
            {
                MQId = mqID ?? Guid.NewGuid().ToString(),
                IntegrationTypeId = NomenclatureConstants.IntegrationTypes.EissProcess,
                IntegrationStateId = EpepConstants.IntegrationStates.New,
                DateWrt = DateTime.Now,
                TargetClassName = processType,
                SourceType = sourceType,
                SourceId = sourceId,
                ErrorCount = 0
            };
            if (processContext != null)
            {
                process.Content = System.Text.Encoding.UTF8.GetBytes(JsonTextSerializer.Serialize(processContext));
            }

            await repo.AddAsync(process);
            await repo.SaveChangesAsync();
        }

        public async Task<ExecAccessDocumentVM> GetAccessDocument(int caseSessionActId, int execListId, Guid accessGid)
        {
            var execProcess = await LoadExecProcess(caseSessionActId, execListId);
            ExecAccessDocumentVM result = null;
            if (caseSessionActId > 0)
            {
                result = await repo.AllReadonly<CaseSessionAct>()
                                   .Where(x => x.Id == caseSessionActId)
                                   .Select(x => new ExecAccessDocumentVM()
                                   {
                                       CourtName = x.Court.Label,
                                       CaseType = x.Case.CaseType.Label,
                                       CaseNumber = $"{x.Case.ShortNumber}/{x.Case.RegDate:yyyy}",
                                       ActType = x.ActType.Label,
                                       ActNumber = $"{x.RegNumber}/{x.RegDate:dd.MM.yyyy}",
                                       DocumentType = x.Case.Document.DocumentType.Label,
                                       DocumentNumber = $"{x.Case.Document.DocumentNumber}/{x.Case.Document.DocumentDate:dd.MM.yyyy}",
                                       ApplicantName = x.Case.Document.DocumentPersons
                                                                    .Where(p => p.PersonRole.RoleKindId == NomenclatureConstants.RoleKind.LeftSide)
                                                                    .Select(p => p.FullName).FirstOrDefault()
                                   }).FirstOrDefaultAsync();
            }
            if (execListId > 0)
            {
                result = await repo.AllReadonly<ExecList>()
                                   .Where(x => x.Id == execListId)
                                   .Select(x => new ExecAccessDocumentVM()
                                   {
                                       CourtName = x.Court.Label,
                                       CaseType = x.Case.CaseType.Label,
                                       CaseNumber = $"{x.Case.ShortNumber}/{x.Case.RegDate:yyyy}",
                                       ActType = x.ExecListType.Label,
                                       ActNumber = $"{x.RegNumber}/{x.RegDate:dd.MM.yyyy}",
                                       DocumentType = x.Case.Document.DocumentType.Label,
                                       DocumentNumber = $"{x.Case.Document.DocumentNumber}/{x.Case.Document.DocumentDate:dd.MM.yyyy}",
                                       ApplicantName = x.Case.Document.DocumentPersons
                                                                    .Where(p => p.PersonRole.RoleKindId == NomenclatureConstants.RoleKind.LeftSide)
                                                                    .Select(p => p.FullName).FirstOrDefault()
                                   }).FirstOrDefaultAsync();
            }
            if (result == null)
            {
                return null;
            }
            var access = execProcess.Data.AccessList.Where(x => x.Gid == accessGid).FirstOrDefault();
            result.AccessKey = access.AccessKey;
            result.AccessDate = access.CreateDate;
            result.UserName = userContext.FullName;
            return result;
        }

        public async Task AppendISPNLetter(int caseSessionActId, int documentTypeId)
        {
            //Това никога не е работело защото се е подавало SourceId в место Id на DocumentTemplate
            //var documentTemplate = await repo.AllReadonly<DocumentTemplate>()
            //                                 .Where(x => x.Id == id)
            //                                 .FirstOrDefaultAsync();


            if (!Infrastructure.Constants.NomenclatureConstants.DocumentType.IspnLetter.Contains(documentTypeId))
                return;
            var sessionAct = await repo.AllReadonly<CaseSessionAct>()
                                       .Include(x => x.CaseSession)
                                       .Where(x => x.Id == caseSessionActId)
                                       .FirstOrDefaultAsync();
            if (sessionAct.TDActForRegistration != true)
                return;
            if (string.IsNullOrEmpty(sessionAct.RegNumber))
                return;

            if (sessionAct.CaseSession.SessionStateId != Infrastructure.Constants.NomenclatureConstants.SessionState.Provedeno)
                return;

            InitMQ(IntegrationTypes.ISPN, SourceTypeSelectVM.CaseSessionAct, sessionAct.Id, ServiceMethod.Add, sessionAct.CaseId);

        }


        /// <summary>
        /// Изпраща документ от регистратура Централно разпределяне за ново определяне на съд
        /// </summary>
        /// <param name="documentId"></param>
        /// <returns></returns>
        public Task EpepDocument_SendForAssignment(long documentId)
        {
            var mq = new MQEpep()
            {
                MQId = mqID ?? Guid.NewGuid().ToString(),
                IntegrationTypeId = NomenclatureConstants.IntegrationTypes.EpepDocuments,
                SourceType = SourceTypeSelectVM.Document,
                SourceId = documentId,
                MethodName = EpepConstants.EpepDocumentMethods.InitAssignment,
                DateWrt = DateTime.Now,
                UserId = ImpersonatedUserId ?? userContext.UserId,
                ErrorCount = 0,
                IntegrationStateId = IntegrationStates.New
            };

            repo.Add(mq);

            return repo.SaveChangesAsync();
        }

        public IQueryable<EpepUserVM> EpepUser_SelectByCasePerson(int casePersonId, bool forSummonOnly = true)
        {
            Expression<Func<EpepUserAssignment, bool>> whereCanSummon = x => true;
            if (forSummonOnly)
            {
                whereCanSummon = x => x.CanSummon == true;
            }

            return repo.AllReadonly<EpepUserAssignment>()
                                    .Where(x => x.CasePersonId == casePersonId)
                                    .Where(FilterExpireInfo<EpepUserAssignment>(false))
                                    .Where(whereCanSummon)
                                    .OrderBy(x => x.EpepUser.FullName)
                                    .Select(x => new EpepUserVM
                                    {
                                        FullName = x.EpepUser.FullName,
                                        Email = x.EpepUser.Email,
                                        CanSummon = x.CanSummon == true,
                                        RoleType = (x.AssignmentRole == EpepConstants.AssignmentRoles.Lawyer) ? "Адвокат" : "Страна"
                                    }).AsQueryable();
        }

        public async Task<(bool isRNFL, bool transferStarted)> RNFL_CheckCase(int caseId)
        {

            var rnflInfo = await repo.AllReadonly<Case>()
                                    .Where(x => x.Id == caseId)
                                    .Select(x => new
                                    {
                                        IsRnfl = x.IspnKind == NomenclatureConstants.IspnKinds.Rnfl,
                                        TransferStarted = x.TransferStartDate != null
                                    }).FirstOrDefaultAsync();

            if (rnflInfo == null)
            {
                return (false, false);
            }

            return (rnflInfo.IsRnfl, rnflInfo.TransferStarted);
        }

        public async Task RNFL_SendCase(int caseId, EpepConstants.ServiceMethod method = ServiceMethod.Add, bool checkRequest = true)
        {
            if (checkRequest)
            {
                (bool isRNFL, bool transferStarted) = await RNFL_CheckCase(caseId);
                if (!isRNFL || !transferStarted)
                {
                    return;
                }
            }

            InitMQWithTarget(IntegrationTypes.Rnfl, RnflConstants.TargetMethods.Case, SourceTypeSelectVM.Case, caseId, method);
        }
        public async Task<bool> RNFL_SendAct(int actId, int caseId, EpepConstants.ServiceMethod method = ServiceMethod.Add)
        {
            var actInfo = await repo.AllReadonly<CaseSessionAct>()
                                    .Where(x => x.Id == actId)
                                    .Select(x => new
                                    {
                                        x.CaseId,
                                        x.ActISPNReasonId
                                    })
                                    .FirstOrDefaultAsync();

            if (actInfo.ActISPNReasonId == null)
            {
                return false;
            }

            (bool isRNFL, bool transferStarted) = await RNFL_CheckCase(actInfo.CaseId ?? 0);
            if (!isRNFL)
            {
                return false;
            }

            if (!transferStarted)
            {
                //Ако няма заявка за дело и правното основание не е стартиращо
                string startLegalBaseCode = getNomValue(RnflConstants.CodeMapping.StartLegalBase, actInfo.ActISPNReasonId);
                if (startLegalBaseCode != "rnfl_start")
                {
                    return false;
                }

                //Ако има заявка и основанието е стартиращо процедурата - се изпращат:
                //делото, всички постановени актове с избрано основание и файловете към тях и страните, които имат map към rnfl_person_roles
                await RNFL_InitCase(caseId);
                return true;
            }

            string actTypeCode = getNomValue(RnflConstants.CodeMapping.ActTypes, actInfo.ActISPNReasonId);
            if (string.IsNullOrEmpty(actTypeCode))
            {
                //Ако няма мапинг за дадения вид акт не прави заявка
                return false;
            }
            InitMQWithTarget(IntegrationTypes.Rnfl, RnflConstants.TargetMethods.Act, SourceTypeSelectVM.CaseSessionAct, actId, method, caseId);
            return true;
        }

        private async Task RNFL_SendDocument(long documentId, int caseId, EpepConstants.ServiceMethod method = ServiceMethod.Add)
        {
            (bool isRNFL, bool transferStarted) = await RNFL_CheckCase(caseId);
            if (!isRNFL)
            {
                return;
            }

            var documentInfo = await repo.AllReadonly<Document>()
                                    .Where(x => x.Id == documentId)
                                    .Select(x => new
                                    {
                                        x.DocumentTypeId
                                    })
                                    .FirstOrDefaultAsync();

            string docTypeCode = getNomValue(RnflConstants.CodeMapping.DocumentTypes, documentInfo.DocumentTypeId);
            if (string.IsNullOrEmpty(docTypeCode))
            {
                return;
            }

            InitMQWithTarget(IntegrationTypes.Rnfl, RnflConstants.TargetMethods.Document, SourceTypeSelectVM.Document, documentId, method, caseId);
        }

        /// <summary>
        /// Инициира изпращане на съществуващите данни към РНФЛ при постановяване на акт със стартиращо основание и липса на заявка за делото
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        private async Task RNFL_InitCase(int caseId)
        {
            try
            {
                Case caseModel = await repo.GetByIdAsync<Case>(caseId);
                caseModel.TransferStartDate = DateTime.Now;
                await repo.SaveChangesAsync();

                await RNFL_SendCase(caseId, ServiceMethod.Add, false);

                Set_AUTOSAVECHANGES(false);

                var actInfos = await repo.AllReadonly<CaseSessionAct>()
                                          .Where(x => x.CaseId == caseId)
                                          .Where(x => x.ActDeclaredDate != null)
                                          .Where(x => x.ActISPNReasonId > 0)
                                          .Select(x => new
                                          {
                                              x.Id,
                                              x.DepersonalizeEndDate
                                          })
                                          .ToArrayAsync();

                foreach (var act in actInfos)
                {
                    bool rnflActOk = await RNFL_SendAct(act.Id, caseId, ServiceMethod.Add);
                    if (rnflActOk)
                    {
                        await RNFL_SendActPrivateFile(act.Id, ServiceMethod.Add);
                        if (act.DepersonalizeEndDate != null)
                        {
                            await RNFL_SendActPublicFile(act.Id, ServiceMethod.Add);
                        }
                    }
                }

                int[] personRolesIds = (await repo.AllReadonly<CodeMapping>()
                                               .Where(x => x.Alias == RnflConstants.CodeMapping.PersonRoles)
                                               .Select(x => x.InnerCode)
                                               .ToArrayAsync()).Select(x => int.Parse(x)).ToArray();
                int[] casePersonIds = await repo.AllReadonly<CasePerson>()
                                                .Where(x => x.CaseId == caseId && x.CaseSessionId == null && x.DateTo == null)
                                                .Where(x => personRolesIds.Contains(x.PersonRoleId))
                                                .Select(x => x.Id)
                                                .ToArrayAsync();

                foreach (var casePersonId in casePersonIds)
                {
                    await RNFL_SendSide(casePersonId, caseId, ServiceMethod.Add, false);
                }

                await repo.SaveChangesAsync();

                Set_AUTOSAVECHANGES(true);

            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"RNFL_InitCase: {caseId}");
            }
        }


        public async Task RNFL_SendActPrivateFile(int actId, EpepConstants.ServiceMethod method = ServiceMethod.Add)
        {
            var actInfo = await repo.AllReadonly<CaseSessionAct>()
                .Where(x => x.Id == actId)
                .Select(x => new
                {
                    x.CaseId,
                    x.ActISPNReasonId,
                }).FirstOrDefaultAsync();
            (bool isRNFL, bool transferStarted) = await RNFL_CheckCase(actInfo.CaseId ?? 0);
            if (!isRNFL || !transferStarted)
            {
                return;
            }

            string actTypeCode = getNomValue(RnflConstants.CodeMapping.ActTypes, actInfo.ActISPNReasonId);
            if (string.IsNullOrEmpty(actTypeCode))
            {
                //Ако няма мапинг за дадения вид акт не прави заявка
                return;
            }

            InitMQWithTarget(IntegrationTypes.Rnfl, RnflConstants.TargetMethods.ActPrivate, SourceTypeSelectVM.CaseSessionActPdf, actId, method, actInfo.CaseId);
        }
        public async Task RNFL_SendActPublicFile(int actId, EpepConstants.ServiceMethod method = ServiceMethod.Add)
        {
            var actInfo = await repo.AllReadonly<CaseSessionAct>()
                .Where(x => x.Id == actId)
                .Select(x => new
                {
                    x.CaseId,
                    x.ActISPNReasonId,
                }).FirstOrDefaultAsync();
            (bool isRNFL, bool transferStarted) = await RNFL_CheckCase(actInfo.CaseId ?? 0);
            if (!isRNFL || !transferStarted)
            {
                //Проверка дали обезличения акт е прикачен към жалба към РНФЛ дело
                int complainId = await repo.AllReadonly<CaseSessionActComplainResult>()
                                            .Where(x => x.CaseSessionActId == actId)
                                            .Where(x => x.CaseSessionActComplain.Case.IspnKind == NomenclatureConstants.IspnKinds.Rnfl)
                                            .Select(x => x.CaseSessionActComplainId)
                                            .FirstOrDefaultAsync();
                if (complainId == 0)
                    return;

                InitMQWithTarget(IntegrationTypes.Rnfl, RnflConstants.TargetMethods.AppealActPublic, SourceTypeSelectVM.CaseSessionActComplainActDepersonalized, actId, method, complainId);
                return;
            }
            string actTypeCode = getNomValue(RnflConstants.CodeMapping.ActTypes, actInfo.ActISPNReasonId);
            if (string.IsNullOrEmpty(actTypeCode))
            {
                //Ако няма мапинг за дадения вид акт не прави заявка
                return;
            }

            InitMQWithTarget(IntegrationTypes.Rnfl, RnflConstants.TargetMethods.ActPublic, SourceTypeSelectVM.CaseSessionActDepersonalized, actId, method, actInfo.CaseId);
        }

        public async Task RNFL_SendAppeal(int actComplainId, EpepConstants.ServiceMethod method = ServiceMethod.Add)
        {
            int caseId = await repo.GetPropByIdAsync<CaseSessionActComplain, int?>(x => x.Id == actComplainId, x => x.CaseId) ?? 0;
            (bool isRNFL, bool transferStarted) = await RNFL_CheckCase(caseId);
            if (!isRNFL || !transferStarted)
            {
                return;
            }

            InitMQWithTarget(IntegrationTypes.Rnfl, RnflConstants.TargetMethods.Appeal, SourceTypeSelectVM.CaseSessionActComplain, actComplainId, method, caseId);
        }
        public async Task RNFL_SendAppealFile(int mongoFileId, EpepConstants.ServiceMethod method = ServiceMethod.Add)
        {
            var fileInfo = await repo.AllReadonly<MongoFile>()
                                    .Where(x => x.Id == mongoFileId)
                                    .Select(x => new
                                    {
                                        x.SourceIdNumber,
                                        x.SourceType
                                    }).FirstOrDefaultAsync();

            if (fileInfo == null || fileInfo.SourceType != SourceTypeSelectVM.Document)
            {
                return;
            }


            var complainInfo = await repo.AllReadonly<CaseSessionActComplain>()
                                    .Where(x => x.ComplainDocumentId == fileInfo.SourceIdNumber)
                                    .Select(x => new
                                    {
                                        x.Id,
                                        CaseId = x.CaseId ?? 0,
                                        x.CaseSessionActId,
                                        IsRnfl = x.Case.IspnKind == NomenclatureConstants.IspnKinds.Rnfl
                                    }).FirstOrDefaultAsync();

            if (complainInfo == null || !complainInfo.IsRnfl)
            {
                return;
            }

            (bool isRNFL, bool transferStarted) = await RNFL_CheckCase(complainInfo.CaseId);
            if (!isRNFL || !transferStarted)
            {
                return;
            }

            InitMQWithTarget(IntegrationTypes.Rnfl, RnflConstants.TargetMethods.AppealFile, SourceTypeSelectVM.CaseSessionActComplain, mongoFileId, method, complainInfo.Id);
        }
        public async Task RNFL_SendSummon(int caseNotificationId, EpepConstants.ServiceMethod method = ServiceMethod.Add)
        {
            int caseId = await repo.GetPropByIdAsync<CaseNotification, int>(x => x.Id == caseNotificationId, x => x.CaseId);
            (bool isRNFL, bool transferStarted) = await RNFL_CheckCase(caseId);
            if (!isRNFL || !transferStarted)
            {
                return;
            }

            InitMQWithTarget(IntegrationTypes.Rnfl, RnflConstants.TargetMethods.Summon, SourceTypeSelectVM.CaseNotification, caseNotificationId, method, caseId);
        }

        public async Task RNFL_SendSide(int casePersonId, int caseId, EpepConstants.ServiceMethod method = ServiceMethod.Add, bool checkCase = true)
        {
            if (checkCase)
            {
                (bool isRNFL, bool transferStarted) = await RNFL_CheckCase(caseId);
                if (!isRNFL || !transferStarted)
                {
                    return;
                }
            }

            var personRoleId = await repo.GetPropByIdAsync<CasePerson, int>(x => x.Id == casePersonId, x => x.PersonRoleId);

            var personRoleCode = getNomValue(RnflConstants.CodeMapping.PersonRoles, personRoleId);
            switch (personRoleCode)
            {
                case RnflConstants.TargetMethods.Debtor:
                    InitMQWithTarget(IntegrationTypes.Rnfl, RnflConstants.TargetMethods.Debtor, SourceTypeSelectVM.CasePerson, casePersonId, method, caseId);
                    break;
                case RnflConstants.TargetMethods.Syndic:
                    InitMQWithTarget(IntegrationTypes.Rnfl, RnflConstants.TargetMethods.Syndic, SourceTypeSelectVM.CasePerson, casePersonId, method, caseId);
                    break;
                default:
                    break;
            }
        }

        public async Task<FileSignerInfoVM> GetEpepUserInfo(CdnDownloadResult fileInfo)
        {
            Expression<Func<Document, bool>> whereDocument = x => false;
            switch (fileInfo.SourceType)
            {
                case SourceTypeSelectVM.DocumentFromElectronicDocument:
                    long documentId = long.Parse(fileInfo.SourceId);
                    return await repo.AllReadonly<Document>()
                           .Where(x => x.Id == documentId)
                           .Where(x => x.ElectronicDocumentId > 0)
                           .Where(x => x.ElectronicDocument.FromAPI == true)
                           .Select(x => x.ElectronicDocument)
                           .Select(x => new FileSignerInfoVM
                           {
                               FromApi = true,
                               EpepUserName = x.EpepUser.FullName,
                               CreateUserName = x.CreateUserName,
                               ApplyDate = x.ApplyDate
                           }).FirstOrDefaultAsync();

                case SourceTypeSelectVM.ElectronicDocument:
                case SourceTypeSelectVM.ElectronicDocumentFile:
                    long electronicDocumentId = long.Parse(fileInfo.SourceId);
                    return await repo.AllReadonly<ElectronicDocument>()
                           .Where(x => x.Id == electronicDocumentId)
                           .Where(x => x.FromAPI == true)
                           .Select(x => new FileSignerInfoVM
                           {
                               FromApi = true,
                               EpepUserName = x.EpepUser.FullName,
                               CreateUserName = x.CreateUserName,
                               ApplyDate = x.ApplyDate
                           }).FirstOrDefaultAsync();
                default:
                    return null;
            }


        }

    }
}


