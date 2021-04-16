using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaseNotificationService: IBaseService
    {
        IQueryable<CaseNotificationVM> CaseNotification_Select(int CaseId, int? caseSessionId, int? caseSessionActId);
        bool CaseNotification_SaveData(CaseNotification model, List<CaseNotificationMLink> casePersonLinks, int[] complainIds);
        IQueryable<CaseSessionNotificationListVM> CaseSessionNotificationList_Select(int caseSessionId, int NotificationListTypeId);
        IQueryable<CaseSessionNotificationListVM> CaseSessionNotificationList_SelectByCaseId(int caseId);
        CheckListViewVM Person_SelectForCheck(int caseId, int caseSessionId, int NotificationListTypeId, bool isCasePerson);
        bool CaseNotificationList_Save(CheckListViewVM checkListViewVM, bool isCasePerson);
        bool CaseNotificationList_SaveData(CaseSessionNotificationList model);
        bool IsExistsNotification(int CaseSessionId, int NotificationPersonType, int PersonId, int NotificationTypeId);
        Task<(bool, int)> DeliveryItemSaveReturn(DeliveryItemReturnVM model, ICollection<IFormFile> returnFiles);
        Task<CdnDownloadResult> ReadPrintedFile(int Id);
        Task<bool> SavePrintedFile(int Id, byte[] pdfBytes);
        string CasePersonLinksJson(CaseNotification caseNotification, bool filterPersonOnNotification, int notificationTypeId);
        List<CaseNotificationMLink> CasePersonLinksByNotificationId(int caseNotificationId, int casePersonId, bool filterPersonOnNotification, int notificationTypeId);
        CaseNotification ReadById(int? id);
        CaseNotification ReadWithMlinkById(int? id);
        List<int> NotificationIdSelect(int? CaseId, int? caseSessionId, int? caseSessionActId, bool existsInNotificationList, int? notificationListTypeId);
        bool SaveExpireInfoPlus(ExpiredInfoVM model);
        Task<CdnDownloadResult> ReadDraftFile(int Id);
        bool IsExistNotificationForSession(int caseSessionId);
        List<SelectListItem> GetDDL_NotificationListType();
        int InsertDeliveryItem(int? courtId);
        Task<List<byte[]>> GetLinkDocument(int notificationId);
        List<SelectListItem> NotificationDeliveryGroupDDL(int notificationTypeId, int caseId);
        bool IsNotificationDeliveryGroupByEpep(int caseId, int casePersonId, string casePersonLinkIds);
        void InitCaseNotificationComplains(CaseNotification caseNotification);
        List<SelectListItem> DocumentSenderPersonDDL(int caseId);
        List<CaseNotification> GetNotPrintedEpep();
    }
}
