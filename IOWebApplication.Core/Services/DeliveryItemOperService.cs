using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using IOWebApplication.Infrastructure.Constants;

namespace IOWebApplication.Core.Services
{
    public class DeliveryItemOperService : BaseService, IDeliveryItemOperService
    {
        public DeliveryItemOperService(
            ILogger<DeliveryItemOperService> _logger,
            IRepository _repo,
            IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
        }
        public IQueryable<DeliveryItemOperListVM> DeliveryItemOperSelect(int deliveryItemId, bool onlyLast)
        {
            var result = repo.AllReadonly<DeliveryItemOper>()
                .Where(x => (x.DeliveryItemId == deliveryItemId))
                .Include(x => x.DeliveryItem)
                .Include(x => x.DeliveryOper)
                .Include(x => x.Court)
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
                    NotificationStateId = x.NotificationStateId
                });

            if (!onlyLast)
            {
                return result.OrderBy(x => x.Id);
            }
            else
            {
                var operStates = repo.AllReadonly<DeliveryOperState>();
                var opers = repo.AllReadonly<DeliveryOper>().Where(x => operStates.Any(os => os.DeliveryOperId == x.Id));
                var list = result.ToList();
                result = result
                    .Where(x => !list.Any(z => z.DeliveryOperId == x.DeliveryOperId && z.Id > x.Id))
                    .Where(x => opers.Any(op => op.Id == x.DeliveryOperId));
                return result.OrderBy(x => x.Id);
            }
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
        public List<SelectListItem> DeliveryOperSelect()
        {
            var result = repo.AllReadonly<DeliveryOperState>()
              .Include(x => x.DeliveryOper)
              .Include(x => x.NotificationState)
              .Select(x => new SelectListItem()
              {
                  Value = x.DeliveryOperId.ToString(),
                  Text = x.DeliveryOper.Label
              })
              .Distinct()
              .OrderBy(x => x.Value)
              .ToList();
            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            return result;
        }
        public List<SelectListItem> NotificationStateForDeliveryOperSelect(int operId)
        {
            var result = repo.AllReadonly<DeliveryOperState>()
              .Where(x => x.DeliveryOperId == operId)
              .Include(x => x.DeliveryOper)
              .Include(x => x.NotificationState)
              .OrderBy(x => x.NotificationState.OrderNumber)
              .Select(x => new SelectListItem()
              {
                  Value = x.NotificationStateId.ToString(),
                  Text = x.NotificationState.Label
              }).ToList() ?? new List<SelectListItem>();
            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            return result;
        }
        public List<SelectListItem> DeliveryOperForNotificationStateSelect(int notificationStateId)
        {
            var result = repo.AllReadonly<DeliveryOperState>()
              .Where(x => x.NotificationStateId == notificationStateId)
              .Include(x => x.DeliveryOper)
              .Include(x => x.NotificationState)
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
                .Include(x => x.DeliveryItem)
                .ThenInclude(x => x.FromCourt)
                .ThenInclude(x => x.Address)
                .Include(x => x.DeliveryArea)
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
        public bool CanAdd(int deliveryItemId)
        {
            var opers = DeliveryItemOperSelect(deliveryItemId, true);
            if (opers.Count() >= 3)
                return false;
            if (opers.Any(x => x.NotificationStateId != NomenclatureConstants.NotificationState.Visited))
                return false;
            return true;
        }
        public DateTime? LastDateOper(int deliveryItemId)
        {
            return DeliveryItemOperSelect(deliveryItemId, true)
                   .Max(x => (DateTime?)x.DateOper);
        }
        public DateTime? GetRegDate(int deliveryItemId)
        {
            return repo.AllReadonly<DeliveryItem>()
                       .Where(x => x.Id == deliveryItemId)
                       .Select(x => (DateTime?)x.CaseNotification.RegDate)
                       .FirstOrDefault();
        }
        public bool HaveSameOper(int deliveryItemId, int deliveryOperId)
        {
            return repo.AllReadonly<DeliveryItemOper>()
                       .Where(x => x.DeliveryItemId == deliveryItemId &&
                                   x.DeliveryOperId == deliveryOperId)
                       .Any();
        }
    }
}
