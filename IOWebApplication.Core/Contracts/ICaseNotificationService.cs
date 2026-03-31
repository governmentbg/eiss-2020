using IOWebApplication.Core.Models;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;
using IOWebApplication.Infrastructure.Models.ViewModels.Epep;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaseNotificationService : IBaseService
    {
        IQueryable<CaseNotificationVM> CaseNotification_Select(int CaseId, int? caseSessionId, int? caseSessionActId);
        Task<bool> CaseNotification_SaveData(CaseNotification model, DeliveryLogVM logVM);
        Task<(List<CaseSessionNotificationListVM>, int)> CaseSessionNotificationList_Select(int caseSessionId, int NotificationListTypeId, int start, int length, List<DataTablesSortColumnVM> sortedColumns, string search);
        IQueryable<CaseSessionNotificationListVM> CaseSessionNotificationList_SelectByCaseId(int caseId);
        Task<CheckListViewVM> Person_SelectForCheck(int caseId, int caseSessionId, int NotificationListTypeId, bool isCasePerson);
        bool CaseNotificationList_Save(CheckListViewVM checkListViewVM, bool isCasePerson);
        bool CaseNotificationList_SaveData(CaseSessionNotificationList model);
        bool IsExistsNotification(int CaseSessionId, int NotificationPersonType, int PersonId, int NotificationTypeId);
        Task<(bool, int)> DeliveryItemSaveReturn(DeliveryItemReturnVM model, ICollection<IFormFile> returnFiles, DeliveryLogVM logVM);
        Task<CdnDownloadResult> ReadPrintedFile(int Id);
        Task<bool> SavePrintedFile(int Id, byte[] pdfBytes);
        string CasePersonLinksJson(CaseNotification caseNotification, bool filterPersonOnNotification, int notificationTypeId);
        List<CaseNotificationMLink> CasePersonLinksByNotificationId(int caseNotificationId, int casePersonId, bool filterPersonOnNotification, int notificationTypeId, int? caseSessionId);
        CaseNotification ReadById(int? id);
        Task<CaseNotification> ReadNotificationByIdAsync(int? id);
        CaseNotification ReadWithMlinkById(int? id);
        Task<List<int>> NotificationIdSelect(NotificationPrintFilterVM filter);
        bool SaveExpireInfoPlus(ExpiredInfoVM model);
        Task<CdnDownloadResult> ReadDraftFile(int Id);
        bool IsExistNotificationForSession(int caseSessionId);
        List<SelectListItem> GetDDL_NotificationListType();
        Task<List<NotificationFileVM>> GetLinkDocument(int notificationId);
        List<SelectListItem> NotificationDeliveryGroupDDL(int notificationTypeId, int caseId);
        Task<bool> IsNotificationDeliveryGroupByEpep(int caseId, int? caseSessionId, int casePersonId, string casePersonLinkIds);
        void InitCaseNotificationComplains(CaseNotification caseNotification);
        List<SelectListItem> DocumentSenderPersonDDL(int caseId);
        List<CaseNotification> GetNotPrintedEpep();
        List<SelectListItem> GetMoneyObligationDDL(int casePersonId, int caseLinkId, int caseSessionActId);
        List<HtmlTemplate> HtmlTemplateNotificationHave_Test();
        List<SelectListItem> GetDDL_ConnectedCases(int caseId, bool addDefaultElement = true);
        List<SelectListItem> GetNotificationIspnReasonDDL(bool addDefaultElement = true);
        Task<(bool, int)> DeliveryItemSaveReturnDocument(DeliveryItemReturnVM model, ICollection<IFormFile> returnFiles, DeliveryLogVM logVM);

        DateTime? GetDatePrevSession(int caseSessionId);
        List<CaseNotificationLinkVM> FilterLinkOnSession(List<CaseNotificationLinkVM> links, int? caseSessionId, List<int> oldLinks);
        List<SelectListItem> GetAddrForPerson(List<CaseNotificationLinkVM> linkListVM, int casePersonId, int casePersonLinkId, int notificationDeliveryGroupId);
        NotificationGroupVM GenerateNotificationGroup(int caseId, int caseSessionId, int notificationListTypeId);
        Task SaveMultiNotification(NotificationGroupVM notificationGroup, DeliveryLogVM logVM);
        Task LoadNotificationGroupList(NotificationGroupVM notificationGroup);
        Task SaveDatePrintMulti(int id);
        string GetFileNameNotification(CaseNotification notification);
        Task<List<NotificationFileVM>> GetActAndComplainDocument(int notificationId);
        Task<List<SelectListItem>> GetDocumentsDDL(int caseId);
        Task<List<NotificationFileVM>> GetCaseNotificationMongoFiles(int notificationId);
        Task<List<NotificationFileVM>> GetCaseNotificationDocuments(int notificationId);
        Task<bool> IsNotificationOnFastProcess(int notificationId);
    }
}
