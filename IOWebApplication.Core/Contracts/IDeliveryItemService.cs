using IOWebApplication.Infrastructure.Data.Models.Cases;
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
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface IDeliveryItemService : IBaseService
    {
        IQueryable<DeliveryItemVM> DeliveryItemSelect(DeliveryItemFilterVM filter);
        IQueryable<DeliveryItemVM> DeliveryItemTransSelect(DeliveryItemTransFilterVM filter);
        IQueryable<DeliveryItemRecapTransVM> DeliveryItemTransSelectRecap(DeliveryItemTransFilterVM filter);
        Task<bool> DeliveryItemSaveDataAddReceived(DeliveryItem model, DeliveryLogVM logVM);
        DeliveryItem GetDeliveryItemByRegNumber(string regNum);

        DeliveryItem GetDeliveryItemByCaseNotificationId(int notificationId);
        Task<(DeliveryItemRecieveVM, string)> SaveRecieved(string regNumber, bool saveIfErr, DeliveryLogVM logVM);

        bool DeliveryItemSaveArea(int id, int courtId, int? deliveryAreaId, int? lawUnitId, DeliveryLogVM logVM);

        DeliveryItem getDeliveryItem(int id);

        Task<bool> DeliveryItemSaveOper(DeliveryItemOperVM model, DeliveryLogVM logVM);
        Task<List<Select2ItemVM>> DeliveryItemTransForIdDDL(DeliveryItemTransFilterVM filter);
        Task<bool> SaveTrans(int[] deliveryItemIds, int notificationStateId, int deliverOperId, DeliveryLogVM logVM);

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
        Task<bool> DeliveryItemSaveOperMobile(DeliveryItemVisitMobile model);
        DeliveryItemReturnVM GetDeliveryItemReturnByNotification(int notificationId);
        Court GetCourtById(int courtId);
        Task<bool> SaveChangeLawUnit(int[] deliveryItemIds, DeliveryItemChangeLawUnitVM filterData, DeliveryLogVM logVM);
        IQueryable<DeliveryItemVM> DeliveryItemChangeLawUnitSelect(DeliveryItemChangeLawUnitVM filterData, int[] newLawUnitId);
        List<SelectListItem> LawUnitForCourt_SelectDdlAllInDeliveryItem(int forCourtId, List<SelectListItem> newLawUnits);
        List<MobileValueLabelVM> GetNotificationTypeMobile();
        List<NotificationState> DeliveryItemTransNotificationState(int toNotificationStateId);
        List<SelectListItem> SelectNewLawUnitType();
        List<SelectListItem> NotificationDeliveryGroupSelect(int filterType);
        Task<bool> DeliveryItemSaveState(int deliveryItemId, int notificationStateId, DateTime? deliveryDate, string deliveryInfo, DeliveryLogVM logVM);
        DeliveryItem getDeliveryItemWithNotification(int id);
        List<Select2ItemVM> GetCourtsSelect2(List<DeliveryArea> deliveryAreaList);
        string GetNotificationInfo(int notificationId);
        string GetNotificationInfoByDeliveryItemId(int deliveryItemId);
        DeliveryItem GetDeliveryItemByDocumentNotificationId(int notificationId);
        DeliveryItemReturnVM GetDeliveryItemReturnByDocumentNotification(int notificationId);
        (byte[], string) GetDeliveryItemReportResultToExcelNew(DeliveryItemListVM filter);
        string ChangeLawUnitAuditInfo(DeliveryItemChangeLawUnitVM filterData);
        void CreateDeliveryItemOperLog(DeliveryItem deliveryItem, DeliveryItemOper deliveryItemOper, DeliveryLogVM logVM);
        List<DeliveryItemOperLogVM> SelectLog(int deliveryItemId);
        Task SetDeliveryItemDates(DeliveryItem deliveryItem, DeliveryItemOper oper);
        void CreateDeliveryItemOperLogEx(DeliveryItem deliveryItem, DeliveryItemOper deliveryItemOper, DeliveryLogVM logVM, int courtId, string userId);
        Task<DeliveryItem> CreateDeliveryItem(CaseNotification notification, bool operIsChanged);
        DeliveryItem GetDeliveryItemByMediationNotificationId(int notificationId);
    }
}
