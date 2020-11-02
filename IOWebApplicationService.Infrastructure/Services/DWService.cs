// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplicationService.Infrastructure.Contracts;
using IOWebApplicationService.Infrastructure.Data.Common;
using IOWebApplicationService.Infrastructure.Data.Models.Base;
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
        public DWService(IRepository _repo, IDWRepository _dwRepo, IDWCaseService _caseSservice, IDWSessionService _caseSessionService, IDWSessionActService _caseSessionActService, IDWDocumentService _documentService, IDWCaseSelectionProtocolService _caseSelectionProtocolService)
        {
            this.repo = _repo;
            this.dwRepo = _dwRepo;
            caseService = _caseSservice;
            caseSessionService = _caseSessionService;
            caseSessionActService = _caseSessionActService;
            documentService = _documentService;
            caseSelectionProtocolService = _caseSelectionProtocolService;
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
                var court = GetCourtData(cId);
                caseService.CaseTransfer(court);
                caseService.CasePersonTransfer(court);
                caseService.CaseLifecycleTransfer(court);
                caseSessionService.SessionTransfer(court);
                caseSessionActService.SessionActTransfer(court);
                caseSessionActService.SessionActDivorceTransfer(court);
                caseSessionActService.SessionActComplainTransfer(court);
                caseSessionActService.SessionActComplainResultTransfer(court);
                caseSessionActService.SessionActComplainPersonTransfer(court);
                caseSessionActService.SessionActCoordinationTransfer(court);
                documentService.DocumentTransfer(court);
                documentService.DocumentDecisionTransfer(court);
                caseSelectionProtocolService.CaseSelectionProtokolTransfer(court);
                caseSelectionProtocolService.CaseSelectionProtocolCompartmentTransfer(court);
                caseSelectionProtocolService.CaseSelectionProtocolLawunitTransfer(court);

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
