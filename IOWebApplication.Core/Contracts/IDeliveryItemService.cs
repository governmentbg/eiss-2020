using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface IDeliveryItemService : IBaseService
    {
        IQueryable<DeliveryItemVM> DeliveryItemSelect(DeliveryItemFilterVM filter); 
        IQueryable<DeliveryItemVM> DeliveryItemTransSelect(DeliveryItemTransFilterVM filter, bool allCourt);
        bool DeliveryItemSaveDataAddReceived(DeliveryItem model);
        DeliveryItem GetDeliveryItemByRegNumber(string regNum);

        DeliveryItem GetDeliveryItemByCaseNotificationId(int notificationId);
        DeliveryItemRecieveVM SaveRecieved(string regNumber, bool saveIfErr, out string messageErr);

        bool DeliveryItemSaveArea(int id, int courtId, int? deliveryAreaId, int? lawUnitId);

        DeliveryItem getDeliveryItem(int id);

        bool DeliveryItemSaveOper(DeliveryItemOperVM model);
        List<SelectListItem> DeliveryItemTransForIdDDL(DeliveryItemTransFilterVM filter);
        bool SaveTrans(int[] deliveryItemIds, int notificationStateId, int deliverOperId);

        // Приети с чекиране призовки/съобщения от потребителя
        IQueryable<DeliveryItemRecieveVM> GetCheckedForToday(string userId, DateTime forDate);
        
        // Въедени призовки/съобщения (от съд без системата)  от потребителя
        IQueryable<DeliveryItem> GetReceivedForToday(string userId, DateTime forDate);
        DeliveryItemOper CreateDeliveryItemOper(DeliveryItem deliveryItem, int deliverOperId);

        IQueryable<DeliveryItemReportVM> GetDeliveryItemOutReport(DeliveryItemListVM filter, bool forCurrentCourt);
        
        (byte[], string) GetDeliveryItemOutToExcel(DeliveryItemListVM filter);

        DeliveryItemReturnVM GetDeliveryItemReturn(int id);

        (byte[], string) GetDeliveryItemReportResultToExcel(DeliveryItemListVM filter);
        List<MobileValueLabelVM> GetCourtsMobile();
        List<MobileValueLabelGroupVM> GetNotificationStateMobile();
        List<MobileValueLabelGroupVM> GetDeliveryReasonMobile();
        List<DeliveryItemMobileVM> GetDeliveryItemMobileVM(int courtId, int lawUnitId, DateTime? fromDate, DateTime? toDate);
        bool DeliveryItemSaveOperMobile(DeliveryItemVisitMobile model);
        DeliveryItemReturnVM GetDeliveryItemReturnByNotification(int notificationId);
        Court GetCourtById(int courtId);
        bool SaveChangeLawUnit(int[] deliveryItemIds, DeliveryItemChangeLawUnitVM filterData);
        IQueryable<DeliveryItemVM> DeliveryItemChangeLawUnitSelect(DeliveryItemChangeLawUnitVM filterData, int[] newLawUnitId);
        List<SelectListItem> LawUnitForCourt_SelectDdlAllInDeliveryItem(int forCourtId, List<SelectListItem> newLawUnits);
        List<MobileValueLabelVM> GetNotificationTypeMobile();
        List<NotificationState> DeliveryItemTransNotificationState(int toNotificationStateId);
        List<SelectListItem> SelectNewLawUnitType();
        List<SelectListItem> NotificationDeliveryGroupSelect();
        bool DeliveryItemSaveState(int deliveryItemId, int notificationStateId, DateTime? deliveryDate, string deliveryInfo);
        DeliveryItem getDeliveryItemWithNotification(int id);
        List<Select2ItemVM> GetCourtsSelect2(List<DeliveryArea> deliveryAreaList);
        string GetNotificationInfo(int notificationId);
        string GetNotificationInfoByDeliveryItemId(int deliveryItemId);
        int[] NotificationStateEnd();
    }
}
