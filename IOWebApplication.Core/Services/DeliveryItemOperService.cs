using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Services
{
    public class DeliveryItemOperService : BaseService, IDeliveryItemOperService
    {
        private readonly IWorkingDaysService workingDaysService;
        public DeliveryItemOperService(
            ILogger<DeliveryItemOperService> _logger,
            IRepository _repo,
            IUserContext _userContext,
            IWorkingDaysService workingDaysService)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            this.workingDaysService = workingDaysService;
        }
        public async Task<List<DeliveryItemOperListVM>> DeliveryItemOperSelect(int deliveryItemId, bool onlyLast)
        {
            var result = await repo.AllReadonly<DeliveryItemOper>()
                .Where(x => (x.DeliveryItemId == deliveryItemId))
                .Select(x => new DeliveryItemOperListVM()
                {
                    Id = x.Id,
                    DeliveryItemId = x.DeliveryItemId,
                    DateOper = x.DateOper,
                    FromCourtName = x.DeliveryItem.FromCourt.Label,
                    ToCourtName = x.Court.Label,
                    AreaName = x.DeliveryArea == null ? "" : x.DeliveryArea.Description,
                    LawUnitName = x.LawUnit == null ? "" : x.LawUnit.FullName,
                    NotificationStateName = x.NotificationState.Label,
                    DeliveryInfo = x.DeliveryInfo,
                    OperName = x.NotificationStateId != x.DeliveryOperId ? x.DeliveryOper.Label : "",
                    DeliveryOperId = x.DeliveryOperId,
                    NotificationStateId = x.NotificationStateId,
                    HaveLocation = !string.IsNullOrEmpty(x.Lat) || !string.IsNullOrEmpty(x.Long)
                })
                .OrderBy(x => x.DeliveryOperId)
                .ThenBy(x => x.Id)
                .ToListAsync();

            if (onlyLast && result.Count() > 0)
            {
                result = result.Where(x => !result.Any(z => z.DeliveryOperId == x.DeliveryOperId && z.Id > x.Id))
                               .Where(x => NomenclatureConstants.DeliveryOper.Visits().Contains(x.DeliveryOperId))
                               .ToList();
            }
            return result;
        }

        public int GetDeliveryOperId(int deliveryItemId)
        {
            var deliveryOpers = repo.AllReadonly<DeliveryItemOper>()
                .Where(x => (x.DeliveryItemId == deliveryItemId));
            var operStates = repo.AllReadonly<DeliveryOperState>();
            var opers = repo.AllReadonly<DeliveryOper>().Where(x => operStates.Any(os => os.DeliveryOperId == x.Id));
            var opers1 = opers.Where(x => !deliveryOpers.Any(d => d.DeliveryOperId == x.Id)).ToList();
            if (opers1.Count > 0)
            {
                return opers1.Min(x => x.Id);
            }
            else
            {
                return opers.Max(x => x.Id);
            }
        }
        public async Task<List<SelectListItem>> DeliveryOperSelect(int operId)
        {
            return await repo.AllReadonly<DeliveryOper>()
              .Where(x => x.Id == operId)
              .Select(x => new SelectListItem()
              {
                  Value = x.Id.ToString(),
                  Text = x.Label
              })
              .ToListAsync();
        }
        public async Task<List<SelectListItem>> NotificationStateForDeliveryOperSelect(int operId)
        {
            var result = await repo.AllReadonly<DeliveryOperState>()
              .Where(x => x.DeliveryOperId == operId)
              .OrderBy(x => x.NotificationState.OrderNumber)
              .Select(x => new SelectListItem()
              {
                  Value = x.NotificationStateId.ToString(),
                  Text = x.NotificationState.Label
              }).ToListAsync();
            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            return result;
        }
        public List<SelectListItem> DeliveryOperForNotificationStateSelect(int notificationStateId)
        {
            var result = repo.AllReadonly<DeliveryOperState>()
              .Where(x => x.NotificationStateId == notificationStateId)
              .OrderBy(x => x.DeliveryOper.OrderNumber)
              .Select(x => new SelectListItem()
              {
                  Value = x.DeliveryOperId.ToString(),
                  Text = x.DeliveryOper.Label
              }).ToList();
            if (result.Any())
                result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            return result;
        }

        public DeliveryItemOperVM getDeliveryItemOper(int id)
        {
            return repo.AllReadonly<DeliveryItemOper>()
                .Where(x => (x.Id == id))
                //.Include(x => x.DeliveryItem)
                //.ThenInclude(x => x.FromCourt)
                //.ThenInclude(x => x.Address)
                //.Include(x => x.DeliveryArea)
                .Select(x => new DeliveryItemOperVM()
                {
                    Id = x.Id,
                    FromCourtName = x.DeliveryItem.FromCourt == null ? "" : x.DeliveryItem.FromCourt.Label,
                    RegNumber = x.DeliveryItem.RegNumber,
                    PersonName = x.DeliveryItem.PersonName,
                    Address = x.DeliveryItem.Address == null ? "" : x.DeliveryItem.Address.FullAddress,
                    AreaName = x.DeliveryArea == null ? "" : x.DeliveryArea.Description,
                    LawUnitName = x.DeliveryArea.LawUnit == null ? "" : x.DeliveryArea.LawUnit.FullName,
                    DeliveryOperId = x.DeliveryOperId,
                    NotificationStateId = x.NotificationStateId,
                    DeliveryItemId = x.DeliveryItemId,
                    DateOper = x.DateOper,
                    DeliveryReasonId = x.DeliveryReasonId,
                    DeliveryInfo = x.DeliveryInfo,
                    CaseInfo = x.DeliveryItem.CaseInfo,
                    Lat = x.Lat,
                    Long = x.Long
                })
                .FirstOrDefault();
        }
        public DeliveryItemOperVM makeDeliveryItemOper(int deliveryItemId)
        {
            var OperId = GetDeliveryOperId(deliveryItemId);
            return repo.AllReadonly<DeliveryItem>()
                .Where(x => (x.Id == deliveryItemId))
                .Include(x => x.FromCourt)
                .Include(x => x.Address)
                .Include(x => x.DeliveryArea)
                .Select(x => new DeliveryItemOperVM()
                {
                    Id = 0,
                    FromCourtName = x.FromCourt.Label,
                    RegNumber = x.RegNumber,
                    PersonName = x.PersonName,
                    Address = x.Address.FullAddress,
                    AreaName = x.DeliveryArea.Description,
                    LawUnitName = x.DeliveryArea.LawUnit == null ? "" : x.DeliveryArea.LawUnit.FullName,
                    NotificationStateId = x.NotificationStateId,
                    DeliveryItemId = deliveryItemId,
                    DeliveryInfo = x.DeliveryInfo,
                    DeliveryOperId = OperId,
                    CaseInfo = x.CaseInfo
                })
                .FirstOrDefault();
        }
        public List<SelectListItem> GetDeliveryReasonDDL(int notificationStateId)
        {
            var stateReason = repo.AllReadonly<DeliveryStateReason>().Where(x => x.NotificationStateId == notificationStateId);
            var result = repo.AllReadonly<DeliveryReason>()
                .Where(x => stateReason.Any(s => s.DeliveryReasonId == x.Id))
                .OrderBy(x => x.OrderNumber)
                .Select(x => new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = x.Label
                })
                .ToList() ?? new List<SelectListItem>();
            if (result.Any())
                result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            return result;
        }
        public async Task<bool> CanAdd(int deliveryItemId)
        {
            var opers = await DeliveryItemOperSelect(deliveryItemId, true);
            var states = NomenclatureConstants.NotificationState.NotificationEndState();
            if (opers.Any(x => states.Contains(x.NotificationStateId)))
                return false;
            return true;
        }

        public DateTime? GetRegDate(int deliveryItemId)
        {
            return repo.AllReadonly<DeliveryItem>()
                       .Where(x => x.Id == deliveryItemId)
                       .Select(x => (DateTime?)x.CaseNotification.RegDate)
                       .FirstOrDefault();
        }
        public async Task<DeliveryItemOper> GetSameOperIfHave(int deliveryItemId, int deliveryOperId)
        {
            return await repo.AllReadonly<DeliveryItemOper>()
                       .Where(x => x.DeliveryItemId == deliveryItemId &&
                                   x.DeliveryOperId == deliveryOperId)
                       .OrderByDescending(x => x.Id)
                       .FirstOrDefaultAsync();
        }

        public string LastVisitLabel(int deliveryItemId)
        {
            var opers = NomenclatureConstants.DeliveryOper.Visits();
            var oper = repo.AllReadonly<DeliveryItemOper>()
                            .Where(x => opers.Contains(x.DeliveryOperId))
                            .Where(x => x.DeliveryItemId == deliveryItemId)
                            .OrderByDescending(x => x.Id)
                            .FirstOrDefault();
            return oper == null ? "" : $"Посещение: {oper.DateOper:dd.MM.yyyy HH:mm}";
        }

        private (DeliveryItemOperListVM, int) Get7DaysPeriodCountAndLastOper(List<DeliveryItemOperListVM> opers)
        {
            var count7daysPeriod = 0;
            DeliveryItemOperListVM lastOper7 = opers.FirstOrDefault();
            for (int i = 1; i < opers.Count; i++)
            {
                var oper = opers[i];
                var dR = oper.DateOper - lastOper7.DateOper;
                if (dR.TotalDays >= 7)
                {
                    count7daysPeriod++;
                    lastOper7 = oper;
                }
            }

            return (lastOper7, count7daysPeriod);
        }
        public async Task<List<string>> CheckDeliveryDate(DeliveryItemOperVM model)
        {
            var errors = new List<string>();
            if (model.DeliveryOperId == NomenclatureConstants.NotificationState.Delivered)
            {
                return errors;
            }
            var opers = await DeliveryItemOperSelect(model.DeliveryItemId, true);
            (var lastOper7, var count7daysPeriod) = Get7DaysPeriodCountAndLastOper(opers);

            if (lastOper7 != null && count7daysPeriod < 2)
            {
                var dR = model.DateOper - lastOper7.DateOper;
                if (dR.Value.TotalDays < 7)
                    errors.Add($"Няма 7 дни от предното посещение {lastOper7.DateOper.ToString(FormattingConstant.NormalDateFormat)}.");
            }

            if (opers.Count >= 2)
            {
                bool haveHoliday = !workingDaysService.IsWorkingDay(userContext.CourtId, model.DateOper?.Date ?? DateTime.Now);
                haveHoliday = haveHoliday || opers.Max(x => !workingDaysService.IsWorkingDay(userContext.CourtId, (DateTime)(x.DateOper).Date));
                if (!haveHoliday)
                {
                    errors.Add($"Няма посещение в почивен ден");
                }
                var firstOper = opers.First();
                var dR = model.DateOper - firstOper.DateOper;
                if (dR.Value.TotalDays < 30)
                    errors.Add($"Няма 30 дни от първото посещение {firstOper.DateOper.ToString(FormattingConstant.NormalDateFormat)}.");

            }
            return errors;
        }
        public async Task<List<string>> CheckDeliveryDateSaved(int caseNotificationId)
        {
            var errors = new List<string>();
            var deliveryItem = await repo.AllReadonly<DeliveryItem>()
                                         .Where(x => x.CaseNotificationId == caseNotificationId)
                                         .FirstOrDefaultAsync();
            if (deliveryItem == null || deliveryItem.NotificationStateId == NomenclatureConstants.NotificationState.Delivered)
                return errors;
            var opers = await DeliveryItemOperSelect(deliveryItem.Id, true);
            if (opers.Count < 2)
                return errors;
            (var lastOper7, var count7daysPeriod) = Get7DaysPeriodCountAndLastOper(opers);
            if (count7daysPeriod < 2) {
                errors.Add($"Няма посещение над 7 дни");
            }
            var lastOper = opers.LastOrDefault();
            if (opers.Count >= 2)
            {
                bool haveHoliday = !workingDaysService.IsWorkingDay(userContext.CourtId, lastOper.DateOper);
                haveHoliday = haveHoliday || opers.Max(x => !workingDaysService.IsWorkingDay(userContext.CourtId, (DateTime)(x.DateOper).Date));
                if (!haveHoliday)
                {
                    errors.Add($"Няма посещение в почивен ден");
                }
                var firstOper = opers.First();
                var dR = lastOper.DateOper - firstOper.DateOper;
                if (dR.TotalDays < 30)
                    errors.Add($"Няма 30 дни от първото посещение {firstOper.DateOper.ToString(FormattingConstant.NormalDateFormat)}.");

            }
            return errors;
        }
    }
}
