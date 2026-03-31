using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.Integrations.RNFL;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplicationService.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace IOWebApplicationService.Infrastructure.Services
{
    public partial class RnflRestService : BaseMQService, IRnflRestService
    {
        private readonly IRnflRestClient rnflRestClient;
        private readonly INomenclatureService nomService;

        public RnflRestService(
            IRepository _repo,
            IRnflRestClient _epepRestClient,
            INomenclatureService _nomService,
            ILogger<RnflRestService> _logger,
            IConfiguration configuration,
            ICdnService _cdnService)
        {
            repo = _repo;
            cdnService = _cdnService;
            logger = _logger;

            rnflRestClient = _epepRestClient;
            nomService = _nomService;

            IntegrationTypeId = NomenclatureConstants.IntegrationTypes.Rnfl;
            batchSave = false;
        }

        protected override Task<bool> InitChanel()
        {
            rnflRestClient.InitClient();

            return Task.FromResult(true);
        }

        protected override async Task CloseChanel()
        {
            await Task.Yield();
        }

        protected override async Task Reconnect()
        {
            if (!await InitChanel())
            {
                return;
            }
        }

        protected override async Task<IEnumerable<MQEpep>> FetchHighPriorityItems(int fetchCount)
        {
            return await Task.FromResult(new List<MQEpep>());
        }

        public async Task TestClient()
        {
            rnflRestClient.InitClient();
            var testResult = await rnflRestClient.HealthTest();

            logger.LogError($"RnflHealthTest: {testResult.Result}; {testResult.ErrorMessage}");
        }


        protected override async Task SendMQ(MQEpep mq)
        {
            DateTime lastDate = DateTime.Now;
            this.currentMqId = mq.Id;

            this.startTime = DateTime.Now;
            switch (mq.TargetClassName)
            {
                case "test":
                    {
                        var testResult = await rnflRestClient.HealthTest();
                        mq.ErrorDescription = testResult.ErrorMessage;
                        UpdateMQ(mq, testResult.Result);
                    }
                    break;

                case RnflConstants.TargetMethods.Act:
                    await Send_Act(mq);
                    break;
                case RnflConstants.TargetMethods.Appeal:
                    await Send_Appeal(mq);
                    break;
                case RnflConstants.TargetMethods.Case:
                    await Send_Case(mq);
                    break;
                case RnflConstants.TargetMethods.Debtor:
                    await Send_Debtor(mq);
                    break;
                case RnflConstants.TargetMethods.Document:
                    await Send_Document(mq);
                    break;
                case RnflConstants.TargetMethods.Summon:
                    await Send_Summon(mq);
                    break;
                case RnflConstants.TargetMethods.Syndic:
                    await Send_Syndic(mq);
                    break;
                case RnflConstants.TargetMethods.ActPrivate:
                case RnflConstants.TargetMethods.ActPublic:
                case RnflConstants.TargetMethods.AppealFile:
                case RnflConstants.TargetMethods.AppealActPrivate:
                case RnflConstants.TargetMethods.AppealActPublic:
                case RnflConstants.TargetMethods.DocumentFile:
                case RnflConstants.TargetMethods.SummonFile:
                    await Send_File(mq);
                    break;
                default:
                    break;
            }
        }

        private async Task Send_Case(MQEpep mq)
        {
            var caseInfo = await repo.AllReadonly<Case>()
                                        .Where(x => x.Id == (int)mq.SourceId)
                                        .Select(x => new
                                        {
                                            x.CourtId,
                                            CaseNumber = x.ShortNumberValue ?? 0,
                                            x.RegDate,
                                            x.CaseCodeId,
                                            x.CaseStateId,
                                            x.DocumentId,
                                            x.Document.DocumentTypeId,
                                            x.Document.DocumentNumber,
                                            x.Document.DocumentDate,
                                            ProcessType = x.RnflProcessType.Code
                                        }).FirstOrDefaultAsync();

            ApiCase serviceModel = new()
            {
                Gid = getKeyGuidNullable(mq.SourceType, mq.SourceId),
                CourtCode = GetNomValue(EpepConstants.Nomenclatures.Courts, caseInfo.CourtId),
                CaseNumber = caseInfo.CaseNumber,
                CaseYear = caseInfo.RegDate.Year,
                ProcessTypeCode = caseInfo.ProcessType,

                //TODO
                //DocumentTypeCode = GetNomValue(RnflConstants.CodeMapping.DocumentTypes, caseInfo.DocumentTypeId),
            };

            if (mq.MethodName == EpepConstants.Methods.Add && serviceModel.Gid.HasValue)
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }
            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    Guid caseGid = await rnflRestClient.InsertCase(serviceModel);
                    if (caseGid != Guid.Empty)
                    {
                        AddIntegrationKey(mq, caseGid, true);
                        ApiDocument caseDoc = new()
                        {
                            CaseGid = caseGid,
                            DocumentNumber = caseInfo.DocumentNumber,
                            DocumentDate = caseInfo.DocumentDate,
                            DocumentTypeCode = GetNomValue(RnflConstants.CodeMapping.DocumentTypes, caseInfo.DocumentTypeId, false)
                        };
                        //Ако няма мапинг за иницииращия документ - взема по подразбиране 2002, Молба за откриване на производство
                        if (string.IsNullOrEmpty(caseDoc.DocumentTypeCode))
                        {
                            caseDoc.DocumentTypeCode = "2002";
                        }
                        Guid docGid = await rnflRestClient.InsertDocument(caseDoc);
                        if (docGid != Guid.Empty)
                        {
                            await sendDocumentFiles(caseInfo.DocumentId, docGid);
                            AddIntegrationKey(SourceTypeSelectVM.Document, caseInfo.DocumentId, docGid.ToString(), false);
                        }
                    }
                    break;
                case EpepConstants.Methods.Update:
                    UpdateMQ(mq, await rnflRestClient.UpdateCase(serviceModel));
                    break;
                default:
                    break;
            }
        }

        private async Task sendDocumentFiles(long documentId, Guid caseDocumentGid)
        {
            var filesInfo = await cdnService.Select(SourceTypeSelectVM.DocumentAllFiles, documentId.ToString())
                                            .Where(x => x.DateExpired == null)
                                            .ToListAsync();
            foreach (var file in filesInfo)
            {
                CdnDownloadResult downloadResult = await cdnService.MongoCdn_Download(file.MongoFileId, CdnFileSelect.PostProcess.None);
                ApiFile fileRequest = new()
                {
                    SourceType = RnflConstants.RnflSourceTypeSelectVM.InsolvencyDocument,
                    SourceGid = caseDocumentGid,
                    AppendUpdate = false,
                    ContentType = downloadResult.ContentType,
                    FileContent = downloadResult.GetBytes(),
                    FileName = downloadResult.FileName
                };

                var fileGid = await rnflRestClient.InsertFile(fileRequest);
                AddIntegrationKey(file.SourceType, documentId, fileGid.ToString(), false);
            }
        }

        private async Task Send_Debtor(MQEpep mq)
        {
            var personInfo = await repo.AllReadonly<CasePerson>()
                                        .Where(x => x.Id == (int)mq.SourceId)
                                        .Select(x => new
                                        {
                                            x.FirstName,
                                            x.MiddleName,
                                            x.FamilyName,
                                            x.Family2Name,
                                            x.FullName,
                                            x.UicTypeId,
                                            x.Uic,
                                            x.BirthCountryCode,
                                            x.BirthForeignPlace,
                                            x.BirthCityCode,
                                            x.CaseId,
                                            Address = x.Addresses
                                                            .Where(x => x.DateExpired == null)
                                                            .Select(a => a.Address)
                                                            .FirstOrDefault()
                                        }).FirstOrDefaultAsync();

            ApiDebtor serviceModel = new()
            {
                Gid = getKeyGuidNullable(mq.SourceType, mq.SourceId),
                CaseGid = getKeyGuid(SourceTypeSelectVM.Case, personInfo.CaseId),
                FirstName = personInfo.FirstName,
                MiddleName = personInfo.MiddleName,
                FamilyName = personInfo.FamilyName,
                Family2Name = personInfo.Family2Name,
                FullName = personInfo.FullName,
                Uic = personInfo.Uic,
                UicTypeCode = GetNomValue(RnflConstants.CodeMapping.UicTypes, personInfo.UicTypeId)
            };

            if (serviceModel.CaseGid == Guid.Empty)
            {
                SetErrorToMQ(mq, EpepConstants.IntegrationStates.WaitForParentIdError, "Изчаква код на дело");
                return;
            }

            if (personInfo.Address != null)
            {
                serviceModel.Address = mapRnflAddress(personInfo.Address);
            }

            if (mq.MethodName == EpepConstants.Methods.Add && serviceModel.Gid.HasValue)
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }

            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, await rnflRestClient.InsertDebtor(serviceModel), false);
                    break;
                case EpepConstants.Methods.Update:
                    UpdateMQ(mq, await rnflRestClient.UpdateDebtor(serviceModel));
                    break;
                default:
                    break;
            }

        }

        private ApiAddress mapRnflAddress(Address address)
        {
            ApiAddress result = new()
            {
                AddressTypeCode = GetNomValue(RnflConstants.CodeMapping.AddressTypes, address.AddressTypeId),
                CountryCode = address.CountryCode,
                CityCode = address.CityCode
            };

            if (address.CountryCode == NomenclatureConstants.CountryBG)
            {
                result.AddressText = nomService.GetStreetAddress(address, false);
            }
            else
            {
                result.AddressText = address.ForeignAddress;
            }

            return result;
        }

        private async Task Send_Syndic(MQEpep mq)
        {
            var personInfo = await repo.AllReadonly<CasePerson>()
                                        .Where(x => x.Id == (int)mq.SourceId)
                                        .Select(x => new
                                        {
                                            x.FirstName,
                                            x.MiddleName,
                                            x.FamilyName,
                                            x.Family2Name,
                                            x.FullName,
                                            x.DateFrom,
                                            x.DateTo,
                                            x.CaseId,
                                            x.RelatedActId,
                                            Address = x.Addresses
                                                            .Where(x=>x.DateExpired == null)
                                                            .Select(a => a.Address)
                                                            .FirstOrDefault()
                                        }).FirstOrDefaultAsync();

            ApiSyndic serviceModel = new()
            {
                Gid = getKeyGuidNullable(mq.SourceType, mq.SourceId),
                CaseGid = getKeyGuid(SourceTypeSelectVM.Case, personInfo.CaseId),
                FirstName = personInfo.FirstName,
                MiddleName = personInfo.MiddleName,
                FamilyName = personInfo.FamilyName,
                Family2Name = personInfo.Family2Name,
                FullName = personInfo.FullName,
                DateStart = personInfo.DateFrom,
                DateEnd = personInfo.DateTo,
                StartActGid = getKeyGuid(SourceTypeSelectVM.CaseSessionAct, personInfo.RelatedActId)
            };

            if (serviceModel.CaseGid == Guid.Empty)
            {
                SetErrorToMQ(mq, EpepConstants.IntegrationStates.WaitForParentIdError, "Изчаква код на дело");
                return;
            }
            if (serviceModel.StartActGid == Guid.Empty)
            {
                SetErrorToMQ(mq, EpepConstants.IntegrationStates.WaitForParentIdError, "Изчаква код на акт за назначаване");
                return;
            }

            if (personInfo.Address != null)
            {
                serviceModel.Address = mapRnflAddress(personInfo.Address);
                serviceModel.Email = personInfo.Address.Email;
                serviceModel.Phone = personInfo.Address.Phone;
            }
            else
            {
                serviceModel.Email = null;
                serviceModel.Phone = null;
            }

            if (mq.MethodName == EpepConstants.Methods.Add && serviceModel.Gid.HasValue)
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }

            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, await rnflRestClient.InsertSyndic(serviceModel), false);
                    break;
                case EpepConstants.Methods.Update:
                    UpdateMQ(mq, await rnflRestClient.UpdateSyndic(serviceModel));
                    break;
                default:
                    break;
            }
        }

        private async Task Send_Act(MQEpep mq)
        {
            var actInfo = await repo.AllReadonly<CaseSessionAct>()
                                        .Where(x => x.Id == (int)mq.SourceId)
                                        .Select(x => new
                                        {
                                            x.RegNumber,
                                            x.ActDeclaredDate,
                                            x.CaseId,
                                            x.CourtId,
                                            x.ActInforcedDate,
                                            x.ActISPNReasonId,
                                            x.RnflEffectiveImmediately
                                        }).FirstOrDefaultAsync();

            ApiAct serviceModel = new()
            {
                Gid = getKeyGuidNullable(mq.SourceType, mq.SourceId),
                CaseGid = getKeyGuid(SourceTypeSelectVM.Case, actInfo.CaseId),
                CourtCode = GetNomValue(EpepConstants.Nomenclatures.Courts, actInfo.CourtId),
                ActNumber = actInfo.RegNumber,
                ActDate = actInfo.ActDeclaredDate.Value,
                EnforceDate = actInfo.ActInforcedDate,
                ActTypeCode = GetNomValue(RnflConstants.CodeMapping.ActTypes, actInfo.ActISPNReasonId),
                LegalBaseCode = GetNomValue(RnflConstants.CodeMapping.LegalBases, actInfo.ActISPNReasonId),
                EffectiveImmediately = actInfo.RnflEffectiveImmediately ?? true
            };

            if (serviceModel.CaseGid == Guid.Empty)
            {
                SetErrorToMQ(mq, EpepConstants.IntegrationStates.WaitForParentIdError, "Изчаква код на дело");
                return;
            }

            if (mq.MethodName == EpepConstants.Methods.Add && serviceModel.Gid.HasValue)
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }

            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, await rnflRestClient.InsertAct(serviceModel), false);
                    break;
                case EpepConstants.Methods.Update:
                    UpdateMQ(mq, await rnflRestClient.UpdateAct(serviceModel));
                    break;
                default:
                    break;
            }

        }

        private async Task Send_Document(MQEpep mq)
        {
            var documentInfo = await repo.AllReadonly<Document>()
                                        .Where(x => x.Id == (int)mq.SourceId)
                                        .Select(x => new
                                        {
                                            x.DocumentNumber,
                                            x.DocumentDate,
                                            x.DocumentTypeId,
                                            CaseId = x.DocumentCaseInfo.Select(d => d.CaseId).FirstOrDefault(),
                                            ApplicantName = x.DocumentPersons.OrderBy(p => p.Id).Select(p => p.FullName).FirstOrDefault()
                                        }).FirstOrDefaultAsync();

            int caseId = (documentInfo.CaseId ?? (int?)mq.ParentSourceId) ?? 0;
            ApiDocument serviceModel = new()
            {
                Gid = getKeyGuidNullable(mq.SourceType, mq.SourceId),
                CaseGid = getKeyGuid(SourceTypeSelectVM.Case, caseId),
                DocumentTypeCode = GetNomValue(RnflConstants.CodeMapping.DocumentTypes, documentInfo.DocumentTypeId),
                DocumentNumber = documentInfo.DocumentNumber,
                DocumentDate = documentInfo.DocumentDate,
                ApplicantName = documentInfo.ApplicantName
            };

            if (serviceModel.CaseGid == Guid.Empty)
            {
                SetErrorToMQ(mq, EpepConstants.IntegrationStates.WaitForParentIdError, "Изчаква код на дело");
                return;
            }

            if (mq.MethodName == EpepConstants.Methods.Add && serviceModel.Gid.HasValue)
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }

            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    AddIntegrationKey(mq, await rnflRestClient.InsertDocument(serviceModel), false);
                    break;
                case EpepConstants.Methods.Update:
                    UpdateMQ(mq, await rnflRestClient.UpdateDocument(serviceModel));
                    break;
                default:
                    break;
            }

        }

        /// <summary>
        /// mq.SourceId = Common.MongoFile.Id,mq.ParentSourceId = Source.Id
        /// </summary>
        /// <param name="mq"></param>
        /// <returns></returns>
        private async Task Send_File(MQEpep mq)
        {
            if (mq.MethodName == EpepConstants.Methods.Delete)
            {
                await Delete_File(mq);
                return;
            }

            bool deletePriorFile = false;
            int rnflSourceType = 0;
            Guid rnflSourceGid = Guid.Empty;
            CdnDownloadResult downloadResult = null;
            switch (mq.SourceType)
            {
                case SourceTypeSelectVM.Files:

                    if (mq.TargetClassName == RnflConstants.TargetMethods.AppealFile)
                    {
                        var fileSourceId = await repo.GetPropByIdAsync<MongoFile, string>(x => x.Id == mq.SourceId, x => x.SourceId);
                        long docId = long.Parse(fileSourceId);
                        int rnflAppealId = await repo.AllReadonly<CaseSessionActComplain>()
                                                       .Where(x => x.ComplainDocumentId == docId)
                                                       .Where(x => x.DateExpired == null)
                                                       .Select(x => x.Id)
                                                       .FirstOrDefaultAsync();

                        rnflSourceType = RnflConstants.RnflSourceTypeSelectVM.InsolvencyAppeal;
                        rnflSourceGid = getKeyGuid(SourceTypeSelectVM.CaseSessionActComplain, rnflAppealId);
                        downloadResult = await cdnService.MongoCdn_Download((int)mq.SourceId, CdnFileSelect.PostProcess.Flatten);
                    }
                    else
                    {
                        var fileSourceType = await repo.GetPropByIdAsync<MongoFile, int>(x => x.Id == mq.SourceId, x => x.SourceType);

                        switch (fileSourceType)
                        {
                            case SourceTypeSelectVM.Document:
                                rnflSourceType = RnflConstants.RnflSourceTypeSelectVM.InsolvencyDocument;
                                rnflSourceGid = getKeyGuid(fileSourceType, mq.ParentSourceId);
                                downloadResult = await cdnService.MongoCdn_Download((int)mq.SourceId, CdnFileSelect.PostProcess.Flatten);
                                break;
                        }
                    }

                    break;
                case SourceTypeSelectVM.CaseSessionActPdf:
                    deletePriorFile = true;
                    rnflSourceType = RnflConstants.RnflSourceTypeSelectVM.InsolvencyActPrivate;
                    rnflSourceGid = getKeyGuid(SourceTypeSelectVM.CaseSessionAct, mq.SourceId);
                    downloadResult = await cdnService.MongoCdn_Download(new CdnFileSelect
                    {
                        SourceType = mq.SourceType,
                        SourceId = mq.SourceId.ToString()
                    }, CdnFileSelect.PostProcess.Flatten);

                    break;
                case SourceTypeSelectVM.CaseSessionActDepersonalized:
                    deletePriorFile = true;

                    rnflSourceType = RnflConstants.RnflSourceTypeSelectVM.InsolvencyActPublic;
                    rnflSourceGid = getKeyGuid(SourceTypeSelectVM.CaseSessionAct, mq.SourceId);

                    downloadResult = await cdnService.MongoCdn_Download(new CdnFileSelect
                    {
                        SourceType = mq.SourceType,
                        SourceId = mq.SourceId.ToString()
                    }, CdnFileSelect.PostProcess.None);

                    break;
                case SourceTypeSelectVM.CaseSessionActComplainActDepersonalized:
                    deletePriorFile = true;

                    rnflSourceType = RnflConstants.RnflSourceTypeSelectVM.InsolvencyAppealActPublic;
                    rnflSourceGid = getKeyGuid(SourceTypeSelectVM.CaseSessionActComplain, mq.ParentSourceId);

                    downloadResult = await cdnService.MongoCdn_Download(new CdnFileSelect
                    {
                        SourceType = mq.SourceType,
                        SourceId = mq.SourceId.ToString()
                    }, CdnFileSelect.PostProcess.None);

                    break;

                default:
                    break;
            }

            if (rnflSourceGid == Guid.Empty)
            {
                mq.ErrorDescription = $"Няма намерен идентификатор на обект за файл id={mq.SourceId}";
                UpdateMQ(mq, false);
                return;
            }

            if (downloadResult == null)
            {
                mq.ErrorDescription = $"Няма намерен файл id={mq.SourceId}";
                UpdateMQ(mq, false);
                return;
            }

            ApiFile fileRequest = new()
            {
                SourceType = rnflSourceType,
                SourceGid = rnflSourceGid,
                AppendUpdate = deletePriorFile,
                ContentType = downloadResult.ContentType,
                FileContent = downloadResult.GetBytes(),
                FileName = downloadResult.FileName
            };

            var rnflResult = await rnflRestClient.InsertFile(fileRequest);
            AddIntegrationKey(mq, rnflResult);
        }

        private async Task Delete_File(MQEpep mq)
        {

            Guid fileGid = getKeyGuid(mq.SourceType, mq.ParentSourceId);
            if (fileGid == Guid.Empty)
            {
                mq.ErrorDescription = $"Няма намерен идентификатор за файл id={mq.SourceId}";
                UpdateMQ(mq, false);
                return;
            }

            bool deleteResult = await rnflRestClient.DeleteFile(fileGid);
            UpdateMQ(mq, deleteResult);
            RemoveIntegrationKeys(mq);
        }


        private async Task Send_Summon(MQEpep mq)
        {
            var summonInfo = await repo.AllReadonly<CaseNotification>()
                                        .Where(x => x.Id == (int)mq.SourceId)
                                        .Select(x => new
                                        {
                                            x.Id,
                                            x.CaseId,
                                            x.NotificationTypeId,
                                            x.RegDate,
                                            x.NotificationPersonName,
                                            x.NotificationPersonDuty
                                        }).FirstOrDefaultAsync();

            ApiSummon serviceModel = new()
            {
                Gid = getKeyGuidNullable(mq.SourceType, mq.SourceId),
                CaseGid = getKeyGuid(SourceTypeSelectVM.Case, summonInfo.CaseId),
                SummonTypeCode = GetNomValue(RnflConstants.CodeMapping.NotificationTypes, summonInfo.NotificationTypeId),
                SummonDate = summonInfo.RegDate,
                Addressee = $"{summonInfo.NotificationPersonName} ({summonInfo.NotificationPersonDuty})"
            };

            if (serviceModel.CaseGid == Guid.Empty)
            {
                SetErrorToMQ(mq, EpepConstants.IntegrationStates.WaitForParentIdError, "Изчаква код на дело");
                return;
            }

            if (mq.MethodName == EpepConstants.Methods.Add && serviceModel.Gid.HasValue)
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }

            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    Guid summonGid = await rnflRestClient.InsertSummon(serviceModel);

                    AddIntegrationKey(mq, summonGid, false);
                    if (summonGid != Guid.Empty)
                    {
                        await sendSummonFile(summonInfo.Id);
                        await MarkSummonAsDelivered(summonInfo.Id);
                    }
                    break;
                case EpepConstants.Methods.Update:
                    bool updateResult = await rnflRestClient.UpdateSummon(serviceModel);
                    UpdateMQ(mq, await rnflRestClient.UpdateSummon(serviceModel));
                    if (updateResult)
                    {
                        await sendSummonFile(summonInfo.Id);
                    }
                    break;
                default:
                    break;
            }

        }

        private async Task Send_Appeal(MQEpep mq)
        {
            var appealInfo = await repo.AllReadonly<CaseSessionActComplain>()
                                        .Where(x => x.Id == mq.SourceIdInt)
                                        .Select(x => new
                                        {
                                            x.Id,
                                            x.CaseId,
                                            x.CaseSessionActId,
                                            x.ComplainDocumentId,
                                            x.ComplainDocument.DocumentTypeId,
                                            AppealDate = x.ComplainDocument.DocumentDate,
                                            Description = x.ComplainDocument.Description,
                                            ResultActId = x.ComplainResults.Where(r => r.CaseSessionActComplainId > 0)
                                                            .Select(r => r.CaseSessionActId).FirstOrDefault()
                                        }).FirstOrDefaultAsync();

            ApiAppeal serviceModel = new()
            {
                Gid = getKeyGuidNullable(SourceTypeSelectVM.CaseSessionActComplain, mq.SourceId),
                CaseGid = getKeyGuid(SourceTypeSelectVM.Case, appealInfo.CaseId),
                ActGid = getKeyGuid(SourceTypeSelectVM.CaseSessionAct, appealInfo.CaseSessionActId),
                AppealTypeCode = GetNomValue(RnflConstants.CodeMapping.AppealTypes, appealInfo.DocumentTypeId),
                AppealDate = appealInfo.AppealDate,
                Description = appealInfo.Description
            };

            if (serviceModel.CaseGid == Guid.Empty)
            {
                SetErrorToMQ(mq, EpepConstants.IntegrationStates.WaitForParentIdError, "Изчаква код на дело");
                return;
            }

            if (serviceModel.ActGid == Guid.Empty)
            {
                SetErrorToMQ(mq, EpepConstants.IntegrationStates.WaitForParentIdError, "Изчаква код на акт");
                return;
            }
            if (string.IsNullOrEmpty(serviceModel.AppealTypeCode))
            {
                SetErrorToMQ(mq, EpepConstants.IntegrationStates.MissingCodeError, $"Невалиден AppealTypeCode:{appealInfo.DocumentTypeId}");
                return;
            }

            if (appealInfo.ResultActId > 0)
            {
                var resultActInfo = await repo.AllReadonly<CaseSessionAct>()
                                              .Where(x => x.Id == appealInfo.ResultActId)
                                              .Where(x => x.ActDeclaredDate != null)
                                              .Select(x => new
                                              {
                                                  x.CourtId,
                                                  CaseNumber = x.Case.ShortNumberValue,
                                                  CaseYear = x.Case.RegDate.Year,
                                                  x.ActDeclaredDate
                                              }).FirstOrDefaultAsync();
                if (resultActInfo != null)
                {
                    serviceModel.AppealCourtCode = GetNomValue(EpepConstants.Nomenclatures.Courts, resultActInfo.CourtId);
                    serviceModel.AppealCaseNumber = resultActInfo.CaseNumber;
                    serviceModel.AppealCaseYear = resultActInfo.CaseYear;
                    serviceModel.AppealActDate = resultActInfo.ActDeclaredDate;
                }
            }



            if (mq.MethodName == EpepConstants.Methods.Add && serviceModel.Gid.HasValue)
            {
                mq.MethodName = EpepConstants.Methods.Update;
            }

            switch (mq.MethodName)
            {
                case EpepConstants.Methods.Add:
                    Guid appealGid = await rnflRestClient.InsertAppeal(serviceModel);
                    AddIntegrationKey(mq, appealGid, true);
                    if (appealGid != Guid.Empty)
                    {
                        await sendAppealFiles(appealInfo.Id, appealInfo.ComplainDocumentId);
                        if (appealInfo.ResultActId > 0)
                        {
                            await sendAppealActPrivateFile(appealInfo.Id, appealInfo.ResultActId.Value);
                            await sendAppealActPublicFile(appealInfo.Id, appealInfo.ResultActId.Value);
                        }
                    }

                    break;
                case EpepConstants.Methods.Update:
                    UpdateMQ(mq, await rnflRestClient.UpdateAppeal(serviceModel));
                    break;
                default:
                    break;
            }

        }

        async Task MarkSummonAsDelivered(int caseNotificationId)
        {
            var rnflNotification = await repo.GetByIdAsync<CaseNotification>(caseNotificationId);
            rnflNotification.NotificationStateId = NomenclatureConstants.NotificationState.Delivered;
            rnflNotification.DeliveryDate = DateTime.Now;
            rnflNotification.ReturnDate = DateTime.Now;
            rnflNotification.ReturnInfo = "Автоматично отразяване като връчена след обявяване в РНФЛ";

            try
            {
                await repo.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"RNFL.MarkSummonAsDelivered; SummonId:{caseNotificationId}");
            }
        }

        private async Task<bool> sendSummonFile(int notificationId)
        {
            var downloadResult = await cdnService.MongoCdn_Download(new CdnFileSelect
            {
                SourceType = SourceTypeSelectVM.CaseNotificationPrint,
                SourceId = notificationId.ToString()
            }, CdnFileSelect.PostProcess.None);

            if (downloadResult == null)
            {
                return false;
            }

            ApiFile summonFileRequest = new()
            {
                SourceType = RnflConstants.RnflSourceTypeSelectVM.InsolvencySummon,
                SourceGid = getKeyGuid(SourceTypeSelectVM.CaseNotification, notificationId),
                AppendUpdate = true,
                ContentType = downloadResult.ContentType,
                FileContent = downloadResult.GetBytes(),
                FileName = downloadResult.FileName
            };

            return await rnflRestClient.InsertFile(summonFileRequest) != Guid.Empty;
        }

        /// <summary>
        /// Изпраща файловете към съпровождащ документ Жалба
        /// </summary>
        /// <param name="complainId"></param>
        /// <param name="complainDocumentId"></param>
        /// <returns></returns>
        private async Task sendAppealFiles(int complainId, long complainDocumentId)
        {
            Guid complainGid = getKeyGuid(SourceTypeSelectVM.CaseSessionActComplain, complainId);
            var complainFileInfo = await cdnService.Select(SourceTypeSelectVM.DocumentAllFiles, complainDocumentId.ToString())
                                                .Where(x => x.DateExpired == null)
                                                .ToListAsync();
            foreach (var fileInfo in complainFileInfo)
            {
                var complainFile = await cdnService.MongoCdn_Download(fileInfo.MongoFileId, CdnFileSelect.PostProcess.None);

                if (complainFile == null)
                {
                    continue;
                }

                ApiFile complainFileRequest = new()
                {
                    SourceType = RnflConstants.RnflSourceTypeSelectVM.InsolvencyAppeal,
                    SourceGid = complainGid,
                    AppendUpdate = false,
                    ContentType = complainFile.ContentType,
                    FileContent = complainFile.GetBytes(),
                    FileName = complainFile.FileName
                };

                Guid docFileGid = await rnflRestClient.InsertFile(complainFileRequest);
                if (docFileGid == Guid.Empty)
                {
                    AddIntegrationKey(SourceTypeSelectVM.Files, fileInfo.MongoFileId, docFileGid.ToString());
                }
            }

        }

        /// <summary>
        /// Изпраща необезличен акт към жалба
        /// </summary>
        /// <param name="complainId"></param>
        /// <param name="actId"></param>
        /// <returns></returns>
        private async Task sendAppealActPrivateFile(int complainId, int actId)
        {
            Guid complainGid = getKeyGuid(SourceTypeSelectVM.CaseSessionActComplain, complainId);
            var actPrivateFile = await cdnService.MongoCdn_Download(new CdnFileSelect
            {
                SourceType = SourceTypeSelectVM.CaseSessionActPdf,
                SourceId = actId.ToString()
            }, CdnFileSelect.PostProcess.None);

            if (actPrivateFile == null)
            {
                return;
            }

            ApiFile actPrivateFileRequest = new()
            {
                SourceType = RnflConstants.RnflSourceTypeSelectVM.InsolvencyAppealActPrivate,
                SourceGid = complainGid,
                AppendUpdate = true,
                ContentType = actPrivateFile.ContentType,
                FileContent = actPrivateFile.GetBytes(),
                FileName = actPrivateFile.FileName
            };

            await rnflRestClient.InsertFile(actPrivateFileRequest);

        }

        /// <summary>
        /// Изпраща обезличен акт към жалба
        /// </summary>
        /// <param name="complainId"></param>
        /// <param name="actId"></param>
        /// <returns></returns>
        private async Task sendAppealActPublicFile(int complainId, int actId)
        {
            Guid complainGid = getKeyGuid(SourceTypeSelectVM.CaseSessionActComplain, complainId);

            var actPublicFile = await cdnService.MongoCdn_Download(new CdnFileSelect
            {
                SourceType = SourceTypeSelectVM.CaseSessionActCoordinationPdf,
                SourceId = actId.ToString()
            }, CdnFileSelect.PostProcess.None);

            if (actPublicFile == null)
            {
                return;
            }

            ApiFile actPublicFileRequest = new()
            {
                SourceType = RnflConstants.RnflSourceTypeSelectVM.InsolvencyAppealActPublic,
                SourceGid = complainGid,
                AppendUpdate = true,
                ContentType = actPublicFile.ContentType,
                FileContent = actPublicFile.GetBytes(),
                FileName = actPublicFile.FileName
            };

            await rnflRestClient.InsertFile(actPublicFileRequest);
        }
    }
}



