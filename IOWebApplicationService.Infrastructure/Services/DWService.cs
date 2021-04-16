using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplicationService.Infrastructure.Contracts;
using IOWebApplicationService.Infrastructure.Data.Common;
using IOWebApplicationService.Infrastructure.Data.Models.Base;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace IOWebApplicationService.Infrastructure.Services
{
    public class DWService : IDWService
    {
        private readonly IRepository repo;
        private readonly IDWRepository dwRepo;
        private readonly IDWCaseService caseService;
        private readonly IDWCaseSelectionProtocolService caseSelectionProtocolService;
        private readonly IDWSessionService caseSessionService;
        private readonly IDWSessionActService caseSessionActService;
        private readonly IDWDocumentService documentService;
        private readonly IDWErrorLogService serviceErrorLog;
        private readonly IConfiguration config;

        public DWService(IRepository _repo, IDWRepository _dwRepo, IDWCaseService _caseSservice, IDWSessionService _caseSessionService, IDWSessionActService _caseSessionActService, IDWDocumentService _documentService, IDWCaseSelectionProtocolService _caseSelectionProtocolService, IDWErrorLogService _serviceErrorLog, IConfiguration _config)
        {
            this.repo = _repo;
            this.dwRepo = _dwRepo;
            caseService = _caseSservice;
            caseSessionService = _caseSessionService;
            caseSessionActService = _caseSessionActService;
            documentService = _documentService;
            caseSelectionProtocolService = _caseSelectionProtocolService;
            this.serviceErrorLog = _serviceErrorLog;
            config = _config;
        }

        public void MigrateCases()
        {
            caseService.CaseTransfer(null);
        }
        public void MigrateAllForCourt(int? courtId)
        {

            Expression<Func<Court, bool>> selectedCourt = x => true;
            if ((courtId ?? 0) > 0)
                selectedCourt = x => x.Id == courtId;


            var courtList = repo.AllReadonly<Court>().Where(selectedCourt).Select(x => x.Id).ToArray();
            foreach (var cId in courtList)
            {
                //На колко добавени елемента да прави нов контекст
                if (dwRepo.TrackerCount > 10000)
                {
                    dwRepo.RefreshDbContext(config.GetConnectionString("DWConnection"));
                }

                DWCourt court = new DWCourt();
                int errRow = 0;
                try
                {


                    court = GetCourtData(cId);
                    serviceErrorLog.LogError((court.CourtId ?? 0), court.CourtName, "НАЧАЛО НА СЪД", 1, "Стартирал");
                    errRow = 1;
                    caseService.CaseTransfer(court);
                    errRow = 2;
                    caseService.CasePersonTransfer(court);
                    errRow = 3;
                    caseService.CaseLifecycleTransfer(court);
                    errRow = 4;
                    caseSessionService.SessionTransfer(court);
                    errRow = 5;
                    caseSessionActService.SessionActTransfer(court);
                    errRow = 6;
                    caseSessionActService.SessionActDivorceTransfer(court);
                    errRow = 7;
                    caseSessionActService.SessionActComplainTransfer(court);
                    errRow = 8;
                    caseSessionActService.SessionActComplainResultTransfer(court);
                    errRow = 9;
                    caseSessionActService.SessionActComplainPersonTransfer(court);
                    errRow = 10;
                    caseSessionActService.SessionActCoordinationTransfer(court);
                    errRow = 11;
                    documentService.DocumentTransfer(court);
                    errRow = 12;
                    documentService.DocumentDecisionTransfer(court);
                    errRow = 13;
                    caseSelectionProtocolService.CaseSelectionProtokolTransfer(court);
                    errRow = 14;
                    caseSelectionProtocolService.CaseSelectionProtocolCompartmentTransfer(court);
                    errRow = 15;
                    caseSelectionProtocolService.CaseSelectionProtocolLawunitTransfer(court);
                }
                catch (Exception ex)
                {

                    serviceErrorLog.LogError((court.CourtId ?? 0), court.CourtName, "Грешка в MigrateCases", errRow, ex.Message);
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
