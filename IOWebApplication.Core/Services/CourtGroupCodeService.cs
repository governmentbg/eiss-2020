using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace IOWebApplication.Core.Services
{
    public class CourtGroupCodeService : BaseService, ICourtGroupCodeService
    {
        private readonly IRelationManyToManyDateService relationService;
        public CourtGroupCodeService(
         ILogger<CourtGroupCodeService> _logger,
         IRepository _repo,
         IRelationManyToManyDateService _relationService)
        {
            logger = _logger;
            repo = _repo;
            relationService = _relationService;
        }
        public IQueryable<MultiSelectTransferVM> CourtGroupCode_Select(int courtId, int courtGroupId, int caseGroupId)
        {
            DateTime dateTomorrow = DateTime.Now.AddDays(1).Date;
            List<int> caseCodeIds = CourtGroupCodeForSelect_Select(courtId, caseGroupId, -1).Select(x => x.Id).ToList();

            return repo.AllReadonly<CourtGroupCode>()
           .Where(x => x.CourtGroupId == courtGroupId &&
                       (x.DateTo ?? dateTomorrow).Date > DateTime.Now.Date &&
                       (caseCodeIds.IndexOf(x.CaseCodeId) >= 0))
           .Select(x => new MultiSelectTransferVM()
           {
               Id = x.CaseCodeId,
               Order = x.CaseCode.OrderNumber,
               Text = $"{x.CaseCode.Code} {x.CaseCode.Label}"
           }).AsQueryable();
        }

        public IQueryable<MultiSelectTransferVM> CourtGroupCodeForSelect_Select(int courtId, int caseGroupId, int caseTypeId)
        {
            int courtTypeId = -1;
            Court court = repo.AllReadonly<Court>().Where(x => x.Id == courtId).FirstOrDefault();
            courtTypeId = court?.CourtTypeId ?? -1;
            var typesForCourt = repo.AllReadonly<CourtTypeCaseType>().Where(x => x.CourtTypeId == courtTypeId);
            return repo.AllReadonly<CaseTypeCode>()
                           .Include(x => x.CaseType)
                           .Include(x => x.CaseCode)
                           .Where(x => ((x.CaseTypeId == caseTypeId) || (caseTypeId <= 0)) &&
                                       (x.CaseType.CaseGroupId == caseGroupId) &&
                                       typesForCourt.Any(t => t.CaseTypeId == x.CaseTypeId)
                           )
                           .Select(x => x.CaseCode)
                           .GroupBy(x => x.Id)
                           .Select(g => g.First())
                           .Select(x => new MultiSelectTransferVM()
                           {
                               Id = x.Id,
                               Order = x.OrderNumber,
                               Text = $"{x.Code} {x.Label}"
                           }).AsQueryable();
        }

        public bool CourtGroupCode_SaveData(int courtGroupId, List<int> codes)
        {
            return relationService.SaveData<CourtGroupCode>(courtGroupId, codes,
                 x => x.CourtGroupId,
                 x => x.CaseCodeId,
                 x => x.DateFrom,
                 x => x.DateTo,
                 null
                );
        }

    }
}
