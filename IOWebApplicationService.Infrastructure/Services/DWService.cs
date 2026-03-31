using DnsClient.Internal;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplicationService.Infrastructure.Contracts;
using IOWebApplicationService.Infrastructure.Data.Common;
using IOWebApplicationService.Infrastructure.Data.Models.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IOWebApplicationService.Infrastructure.Services
{
    public class DWService : BaseDWService, IDWService
    {
        private readonly IRepository repo;

        private readonly IDWCaseService caseService;
        private readonly IDWCaseSelectionProtocolService caseSelectionProtocolService;
        private readonly IDWSessionService caseSessionService;
        private readonly IDWSessionActService caseSessionActService;
        private readonly IDWDocumentService documentService;
        private readonly IDWErrorLogService serviceErrorLog;
        private readonly ILogger<DWService> logger;
        private int CourtId = 0;
        private int FromCourtId = 0;


        public DWService(IRepository _repo, ILogger<DWService> _logger, IDWRepository _dwRepo, IDWCaseService _caseSservice, IDWSessionService _caseSessionService, IDWSessionActService _caseSessionActService, IDWDocumentService _documentService, IDWCaseSelectionProtocolService _caseSelectionProtocolService, IDWErrorLogService _serviceErrorLog, IConfiguration _config)
        {
            this.repo = _repo;
            this.dwRepo = _dwRepo;
            logger = _logger;
            caseService = _caseSservice;
            caseSessionService = _caseSessionService;
            caseSessionActService = _caseSessionActService;
            documentService = _documentService;
            caseSelectionProtocolService = _caseSelectionProtocolService;
            this.serviceErrorLog = _serviceErrorLog;
            config = _config;

            CourtId = config.GetValue<int>("DW:CourtId", 0);
            FromCourtId = config.GetValue<int>("DW:FromCourtId", 0);
        }

        public async Task MigrateAllForCourt(int[] excludeCourtIds = null)
        {

            Expression<Func<Court, bool>> selectedCourt = x => true;

            if (CourtId > 0)
            {
                selectedCourt = x => x.Id == CourtId;
            }

            if (excludeCourtIds != null)
            {
                selectedCourt = x => !excludeCourtIds.Contains(x.Id);
            }

            var courtList = repo.AllReadonly<Court>()
                                    .Where(x => x.Id >= FromCourtId)
                                    .Where(x => x.CourtTypeId != NomenclatureConstants.CourtType.Аdministrative)
                                    .Where(selectedCourt)
                                    .OrderBy(x => x.Id).Select(x => x.Id).ToArray();

            foreach (var cId in courtList)
            {


                DWCourt court = new DWCourt();
                int errRow = 0;
                try
                {
                    court = GetCourtData(cId);
                    serviceErrorLog.LogError((court.CourtId ?? 0), court.CourtName, "НАЧАЛО НА СЪД", 1, "Стартирал");
                    errRow = 1;
                    await caseService.CaseTransfer(court);
                    errRow = 2;
                    await caseSessionService.SessionTransfer(court);
                    errRow = 3;
                    dwRepo.RefreshDbContext(null);

                    await caseSessionActService.SessionActTransfer(court);
                    errRow = 4;
                    dwRepo.RefreshDbContext(null);
                    await caseService.CasePersonTransfer(court);
                    errRow = 5;
                    dwRepo.RefreshDbContext(null);
                    await caseService.CaseLifecycleTransfer(court);
                    errRow = 6;
                    await caseSessionActService.SessionActDivorceTransfer(court);
                    errRow = 7;
                    await documentService.DocumentTransfer(court);
                    //checkForRefresh();
                    //caseSessionActService.SessionActComplainTransfer(court);
                    //errRow = 8;
                    //caseSessionActService.SessionActComplainResultTransfer(court);
                    //errRow = 9;
                    //checkForRefresh();
                    //caseSessionActService.SessionActComplainPersonTransfer(court);
                    //errRow = 10;
                    //caseSessionActService.SessionActCoordinationTransfer(court);

                    //errRow = 12;
                    //documentService.DocumentDecisionTransfer(court);
                    //errRow = 13;
                    //checkForRefresh();
                    //caseSelectionProtocolService.CaseSelectionProtokolTransfer(court);
                    //errRow = 14;
                    //caseSelectionProtocolService.CaseSelectionProtocolCompartmentTransfer(court);
                    //errRow = 15;
                    //caseSelectionProtocolService.CaseSelectionProtocolLawunitTransfer(court);
                    dwRepo.RefreshDbContext(null);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "MigrateAllForCourt Error");


                    var exMessage = ex.Message;
                    if (ex.InnerException != null)
                    {
                        exMessage += "; " + ex.InnerException.Message;
                    }
                    serviceErrorLog.LogError((court.CourtId ?? 0), court.CourtName, "Грешка в MigrateCases", errRow, exMessage);
                }

            }


        }
        public DWCourt GetCourtData(int? courtId)
        {
            DWCourt court = new DWCourt();
            court = repo.AllReadonly<Court>()
                      .Where(x => x.Id == courtId)
                     .Select(x => new DWCourt()
                     {
                         CourtId = x.Id,
                         CourtName = x.Label,
                         CourtTypeId = x.CourtTypeId,
                         CourtTypeName = x.CourtType.Label,
                         ParentCourtId = x.ParentCourtId,
                         ParentCourtName = x.ParentCourt.Label,
                         CourtRegionId = x.CourtRegionId,
                         CourtRegionName = x.CourtRegion.Label,
                         EcliCode = x.EcliCode,
                         EISPPCode = x.EISPPCode,
                         CityCode = x.CityCode,
                         CityName = x.CityName


                     }).FirstOrDefault();



            return court;
        }
    }
}
