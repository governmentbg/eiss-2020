// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Models.Cdn;
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
    public interface IDocumentNotificationService: IBaseService
    {
        Task<bool> DocumentNotification_SaveData(DocumentNotification model, List<DocumentNotificationMLink> documenttNotificationLinks, DeliveryLogVM logVM);
        IQueryable<DocumentNotificationVM> DocumentNotification_Select(long documentId, long? documentResolutionId);
        List<DocumentNotificationMLink> DocumentPersonLinksByNotificationId(int documentNotificationId, int documentPersonId, int notificationTypeId);
        string DocumentPersonLinksJson(DocumentNotification documentNotification, int notificationTypeId);
        NotificationDocGroupVM GenerateNotificationGroup(long? documentId, long? documentResolutionId, int notificationTypeId);
        List<SelectListItem> GetAddrForPerson(List<DocumentNotificationLinkVM> linkListVM, long documentPersonId, long documentPersonLinkId, int notificationDeliveryGroupId);
        List<SelectListItem> NotificationDeliveryGroupDDL(int notificationTypeId);
        DocumentNotification ReadById(int? id);
        Task<CdnDownloadResult> ReadDraftFile(int Id);
        Task<CdnDownloadResult> ReadPrintedFile(int Id);
        bool SaveExpireInfoPlus(ExpiredInfoVM model);
        Task SaveMultiNotification(NotificationDocGroupVM notificationGroup, DeliveryLogVM logVM);
        Task<bool> SavePrintedFile(int Id, byte[] pdfBytes);
    }
}
