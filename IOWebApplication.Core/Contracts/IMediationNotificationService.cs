// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Models;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;
using IOWebApplication.Infrastructure.Models.ViewModels.Documents;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface IMediationNotificationService : IBaseService
    {
        List<SelectListItem> GetAddrForPerson(List<CaseNotificationLinkVM> linkListVM, int casePersonId, int casePersonLinkId, int notificationDeliveryGroupId);
        List<SelectListItem> GetDDL_PersonList(int caseId, int mediationSessionId, int? notificationTypeId, bool addDefaultElement = true, bool addAllElement = false);
        Task<bool> MediationNotification_SaveData(MediationNotification model, List<MediationNotificationMLink> mediationNotificationMLinks, DeliveryLogVM logVM);
        Task<(List<MediationNotificationListVM>, int)> MediationNotification_Select(int mediationSessionId, int start, int length, List<DataTablesSortColumnVM> sortedColumns, string search);
        List<MediationNotificationMLink> MediationPersonLinks(MediationNotification mediationNotification);
        List<MediationNotificationMLink> MediationPersonLinksByNotificationId(int mediationNotificationId, int casePersonId, int notificationTypeId);
        string MediationPersonLinksJson(MediationNotification mediationNotification);
        List<SelectListItem> NotificationDeliveryGroupDDL(int notificationTypeId);
        MediationNotification ReadById(int? id);
        Task<CdnDownloadResult> ReadDraftFile(int Id);
        Task<CdnDownloadResult> ReadPrintedFile(int Id);
        bool SaveExpireInfoPlus(ExpiredInfoVM model);
        Task<bool> SavePrintedFile(int Id, byte[] pdfBytes);
    }
}
