// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Models.Cdn;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface IVksNotificationService: IBaseService
    {
        void FillNotificationItem(VksNotificationListVM notificationList);
        void FillNotificationItemDDL(VksNotificationListVM notificationList);
        IEnumerable<VksNotificationPrintListVM> FillVksNotificationPrintList(VksNotificationPrintFilter filter, int[] caseSessionIds);
        List<SelectListItem> GetDDL_NotificationPrintList();
        VksNotificationListVM GetNotificationItem(int caseSessionId);
        string GetPaperEdition(VksNotificationPrintFilter filter);
        List<SelectListItem> GetVksPersonAdress(int casePersonId, int? casePersonLinkId);
        bool IsCaseForCountryPaper(int caseId);
        Task<CdnDownloadResult> ReadPrintedFile(int Id);
        bool SaveData(VksNotificationListVM model);
        int SaveSelectedList(string caseSessionIdsJson, int vksMonth);
        bool SaveVksNotificationPrint(VksNotificationPrintVM model);
    }
}
