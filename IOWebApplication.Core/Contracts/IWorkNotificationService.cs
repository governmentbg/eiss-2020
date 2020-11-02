// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface IWorkNotificationService : IBaseService
    {
        List<SelectListItem> GetDDL_WorkNotificationTypes(int sourceType);
        string GetJudgeUserId(int caseId);
        WorkNotificationFilterVM MakeDefaultFilter();
        WorkNotification NewWorkNotification(CaseNotification caseNotification);
        WorkNotification NewWorkNotification(CaseDeadline caseDeadline);
        WorkNotification NewWorkNotification(CaseLawUnit model);
        List<WorkNotification> NewWorkNotificationSecretary(CaseDeadline caseDeadline);
        List<SelectListItem> ReadTypeId_SelectDDL();
        bool SaveWorkNotification(WorkNotification workNotification);
        bool SaveWorkNotificationRead(long id);
        IQueryable<WorkNotification> SelectWorkNotifications(WorkNotificationFilterVM filterData);

    }
}
