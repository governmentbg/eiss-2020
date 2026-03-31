using IOWebApplication.Infrastructure.Data.Models.Delivery;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface IDeliveryItemOperService: IBaseService
    {
        Task<List<DeliveryItemOperListVM>> DeliveryItemOperSelect(int deliveryItemId, bool onlyLast);

        Task<List<SelectListItem>> DeliveryOperSelect(int operId);
        Task<List<SelectListItem>> NotificationStateForDeliveryOperSelect(int operId);

        List<SelectListItem> DeliveryOperForNotificationStateSelect(int operId);

        DeliveryItemOperVM getDeliveryItemOper(int id);

        DeliveryItemOperVM makeDeliveryItemOper(int deliveryItemId);
        int GetDeliveryOperId(int deliveryItemId);
        List<SelectListItem> GetDeliveryReasonDDL(int notificationStateId);
        Task<bool> CanAdd(int deliveryItemId);
        DateTime? GetRegDate(int deliveryItemId);
        Task<DeliveryItemOper> GetSameOperIfHave(int deliveryItemId, int deliveryOperId);
        string LastVisitLabel(int deliveryItemId);
        Task<List<string>> CheckDeliveryDate(DeliveryItemOperVM model);
        Task<List<string>> CheckDeliveryDateSaved(int caseNotificationId);
    }
}
