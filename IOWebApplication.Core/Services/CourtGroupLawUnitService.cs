using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IOWebApplication.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using IOWebApplication.Infrastructure.Constants;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Services
{
    public class CourtGroupLawUnitService : BaseService, ICourtGroupLawUnitService
    {
        private ICommonService commonService;
        private readonly IRelationManyToManyDateService relationService;
        public CourtGroupLawUnitService(
         ILogger<CourtGroupLawUnitService> _logger,
         IRepository _repo,
         ICommonService _commonService,
         IRelationManyToManyDateService _relationService)
        {
            logger = _logger;
            repo = _repo;
            commonService = _commonService;
            relationService = _relationService;
        }
        public IQueryable<MultiSelectTransferPercentVM> CourtGroupLawUnitSaved(int courtId, int courtGroupId, int groupKind)
        {

            DateTime dateSelect = DateTime.Now;
            var lawUnitGroup = repo.AllReadonly<CourtLawUnitGroup>()
                                   .Where(x => x.CourtGroupId == courtGroupId && (x.DateTo ?? dateSelect) >= dateSelect);

            switch (groupKind)
            {
                default:
                    return commonService
                       .LawUnit_JudgeByCourtDate(courtId, DateTime.Now)
                       .Join(lawUnitGroup, l => l.Id, r => r.LawUnitId, (l, r) => new { l, r })
                       .Select(x => new MultiSelectTransferPercentVM()
                       {
                           Id = x.r.LawUnitId,
                           Order = 0,
                           Text = x.l.FullName,
                           Percent = x.r.LoadIndex
                       });
                case NomenclatureConstants.CourtGroupKinds.SpecialAccess:
                    return commonService
                       .LawUnit_JudgeAndUserByCourtDate(courtId, DateTime.Now)
                       .Join(lawUnitGroup, l => l.Id, r => r.LawUnitId, (l, r) => new { l, r })
                       .Select(x => new MultiSelectTransferPercentVM()
                       {
                           Id = x.r.LawUnitId,
                           Order = 0,
                           Text = x.l.FullName,
                           Percent = x.r.LoadIndex
                       });
            }

        }

        public IQueryable<MultiSelectTransferPercentVM> CourtGroupLawUnitForSelect(int courtId, int groupKind)
        {
            switch (groupKind)
            {
                default:
                    return commonService
                        .LawUnit_JudgeByCourtDate(courtId, DateTime.Now)
                        .Select(x => new MultiSelectTransferPercentVM()
                        {
                            Id = x.Id,
                            Order = 0,
                            Text = x.FullName,
                            Percent = 100
                        });
                case NomenclatureConstants.CourtGroupKinds.SpecialAccess:
                    return commonService
                        .LawUnit_JudgeAndUserByCourtDate(courtId, DateTime.Now)
                        .Select(x => new MultiSelectTransferPercentVM()
                        {
                            Id = x.Id,
                            Order = 0,
                            Text = x.FullName,
                            Percent = 100
                        });
            }
        }

        public async Task<bool> CourtGroupLawUnitSaveData(int courtId, int courtGroupId, List<MultiSelectTransferPercentVM> lawUnits)
        {
            return await relationService.SaveDataPercent<CourtLawUnitGroup>(courtGroupId, lawUnits,
                x => x.CourtId == courtId,
                x => x.CourtGroupId,
                x => x.CourtGroupId == courtGroupId && x.DateTo == null,
                x => x.LawUnitId,
                x => x.DateFrom,
                x => x.DateTo,
                x => x.LoadIndex,
                (x) =>
                {
                    x.CourtId = courtId;
                    return true;
                }
           , false);
        }
        public IQueryable<CourtLawUnitLoadVM> CourtGroup_LawUnitsHistory_Select(int courtGroupId)
        {
            return repo.AllReadonly<CourtLawUnitGroup>()
                       .Where(x => x.CourtGroupId == courtGroupId)
                       .OrderBy(x => x.LawUnit.FullName)
                       .ThenBy(x => x.DateTo ?? DateTime.Now)
                       .Select(x => new CourtLawUnitLoadVM()
                       {
                           FullName = x.LawUnit.FullName,
                           DateFrom = x.DateFrom,
                           DateTo = x.DateTo,
                           LoadIndex = x.LoadIndex
                       }).AsQueryable();
        }

        public List<SelectListItem> GetLawUnitsByCourtGroup(int courtGroupId)
        {
            DateTime dtNow = DateTime.Now;
            return repo.AllReadonly<CourtLawUnitGroup>()
                       .Where(x => x.CourtGroupId == courtGroupId)
                       .Where(x => (x.DateFrom <= dtNow) && ((x.DateTo ?? DateTime.MaxValue) >= dtNow))
                       .Where(x => (x.LawUnit.DateFrom <= dtNow) && ((x.LawUnit.DateTo ?? DateTime.MaxValue) >= dtNow))
                       .Where(x => x.LawUnit.Courts.Any(c => c.DateFrom <= dtNow &&
                            (c.DateTo ?? DateTime.MaxValue) >= dtNow &&
                            (c.MandateDateTo ?? DateTime.MaxValue) >= dtNow &&
                            c.CourtId == x.CourtId &&
                            NomenclatureConstants.PeriodTypes.CurrentlyAvailable.Contains(c.PeriodTypeId)))
                       .Select(x => new SelectListItem()
                       {
                           Value = x.LawUnitId.ToString(),
                           Text = x.LawUnit.FullName
                       }).OrderBy(x => x.Text).ToList();
        }


    }
}

