using Integration.LegalActs;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplicationService.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto.Operators;
using System;
using System.Linq;
using System.Threading.Tasks;
using static IOWebApplication.Infrastructure.Constants.EpepConstants;

namespace IOWebApplicationService.Infrastructure.Services
{
    public class CubipsaService : BaseMQService, ICubipsaService
    {
        protected ICubipsaConnectionService connector;
        protected LegalActsServiceClient serviceClient;


        public CubipsaService(
                IRepository _repo,
                ICdnService _cdnService,
                ICubipsaConnectionService _connector,
                ILogger<CubipsaService> _logger)
        {
            repo = _repo;
            cdnService = _cdnService;
            connector = _connector;
            logger = _logger;
            this.IntegrationTypeId = NomenclatureConstants.IntegrationTypes.LegalActs;
        }
        protected override async Task<bool> InitChanel()
        {
            serviceClient = await connector.Connect();

            return serviceClient != null;
        }

        protected override async Task CloseChanel()
        {
            await serviceClient.CloseAsync();
        }

        protected override async Task SendMQ(MQEpep mq)
        {
            switch (mq.SourceType)
            {
                case SourceTypeSelectVM.CaseSessionAct:
                    await SendAct(mq);
                    break;
                default:
                    break;
            }
        }

        private async Task SendAct(MQEpep mq)
        {

            try
            {

                switch (mq.MethodName)
                {
                    case EpepConstants.Methods.Add:
                    case EpepConstants.Methods.Update:
                        {
                            var actModel = await initModel((int)mq.SourceId);
                            await serviceClient.SendActAsync(actModel);
                        }
                        break;
                    case EpepConstants.Methods.Delete:
                        {
                            var actModel = await initModel((int)mq.SourceId);
                            await serviceClient.DeleteActAsync(actModel);
                        }
                        break;
                    case "correctYear":
                        {
                            var actModel = await initModel((int)mq.SourceId, true);
                            await serviceClient.DeleteActAsync(actModel);
                            var actModelCorrent = await initModel((int)mq.SourceId);
                            await serviceClient.SendActAsync(actModelCorrent);
                        }
                        break;
                    default:
                        break;
                }


                UpdateMQ(mq, true);
            }
            catch (Exception ex)
            {
                SetErrorToMQ(mq, IntegrationStates.TransferError, ex.Message);
            }
        }

        private async Task<Act> initModel(int id, bool correctMode = false)
        {
            var actInfo = repo.AllReadonly<CaseSessionAct>()
                                .Include(x => x.CaseSession)
                                .Where(x => x.Id == id)
                                .Select(x => new
                                {
                                    CaseId = x.CaseId,
                                    ActTypeId = x.ActTypeId,
                                    CaseTypeId = x.Case.CaseTypeId,
                                    CaseNumber = x.Case.ShortNumberValue ?? 0,
                                    CaseYear = x.Case.RegDate.Year,
                                    CourtId = x.Case.CourtId,
                                    JudgeName = x.CaseSession.CaseLawUnits
                                                    .Where(l => l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                    .Select(l => l.LawUnit.FullName)
                                                    .FirstOrDefault(),
                                    StartDate = x.ActDeclaredDate,
                                    LegalDate = x.ActInforcedDate,
                                    MotiveDate = x.ActMotivesDeclaredDate,
                                    ActNumber = x.RegNumber,
                                    EcliCode = x.EcliCode,
                                    ActYear = x.RegDate.Value.Year
                                }).FirstOrDefault();

            var docTemplates = repo.AllReadonly<DocumentTemplate>();
            var migrationInfo = repo.AllReadonly<CaseMigration>()
                                        .Where(x => x.CaseSessionActId == id && x.CaseMigrationTypeId == NomenclatureConstants.CaseMigrationTypes.SendNextLevel)
                                        .Where(x => x.SendToCourtId > 0)
                                        .OrderByDescending(x => x.Id)
                                        .Select(x => new
                                        {
                                            MigrationId = x.Id,
                                            HigherCourtId = x.SendToCourtId,
                                            OutDocumentId = docTemplates.Where(d => d.SourceType == SourceTypeSelectVM.CaseMigration && d.SourceId == x.Id)
                                                                    .Where(d => d.DocumentId > 0)
                                                                    .Select(d => d.DocumentId)
                                                                    .FirstOrDefault()
                                        }).FirstOrDefault();




            var model = new Act()
            {
                UID = AppendUpdateIntegrationKey(SourceTypeSelectVM.CaseSessionAct, id),
                ActKind = GetNomValueInt(EpepConstants.Nomenclatures.ActTypes, actInfo.ActTypeId),
                CaseKind = GetNomValueInt(EpepConstants.Nomenclatures.CaseTypes, actInfo.CaseTypeId),
                CaseNumber = actInfo.CaseNumber,
                Court = GetNomValueInt(EpepConstants.Nomenclatures.Courts, actInfo.CourtId),
                Judge = actInfo.JudgeName,
                LegalDate = actInfo.LegalDate,
                StartDate = actInfo.StartDate,
                MotiveDate = actInfo.MotiveDate,
                Number = int.Parse(actInfo.ActNumber),
                EcliCode = actInfo.EcliCode,
                Year = actInfo.CaseYear
            };
            if (correctMode)
            {
                model.Year = actInfo.ActYear;
            }
            //Липсва мапинг за изходящите документи в ЕПЕП!!!!!
            if (false && migrationInfo != null && migrationInfo.OutDocumentId != null)
            {
                var outDocument = repo.GetById<Document>(migrationInfo.OutDocumentId);
                model.DataForHigherCourt = new DataForHigherCourt()
                {
                    Court = GetNomValueInt(EpepConstants.Nomenclatures.Courts, migrationInfo.HigherCourtId),
                    OutputNumber = outDocument.DocumentNumberValue.Value,
                    DateOfDispatch = outDocument.DocumentDate,
                    TypeOfDocument = GetNomValueInt(EpepConstants.Nomenclatures.OutgoingDocumentTypes, outDocument.DocumentTypeId),
                    Year = outDocument.DocumentDate.Year
                };
            }

            CdnDownloadResult actFile = await cdnService.MongoCdn_Download(new CdnFileSelect()
            {
                SourceType = SourceTypeSelectVM.CaseSessionActDepersonalized,
                SourceId = id.ToString()
            });
            if (actFile != null && !string.IsNullOrEmpty(actFile.FileContentBase64))
            {
                model.ActTextType = actFile.ContentType;
                model.ActText = Convert.FromBase64String(actFile.FileContentBase64);
            }

            if (model.MotiveDate.HasValue)
            {
                CdnDownloadResult motiveFile = await cdnService.MongoCdn_Download(new CdnFileSelect()
                {
                    SourceType = SourceTypeSelectVM.CaseSessionActMotiveDepersonalized,
                    SourceId = id.ToString()
                });
                if (motiveFile != null && !string.IsNullOrEmpty(motiveFile.FileContentBase64))
                {
                    model.MotiveTextType = motiveFile.ContentType;
                    model.MotiveText = Convert.FromBase64String(motiveFile.FileContentBase64);
                }
                else
                {
                    model.MotiveDate = null;
                }
            }

            return model;
        }



    }
}



