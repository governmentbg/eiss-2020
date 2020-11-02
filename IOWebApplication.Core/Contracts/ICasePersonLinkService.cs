// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface ICasePersonLinkService : IBaseService
    {
        IQueryable<CasePersonLinkListVM> CasePersonLink_Select(int caseId);
        bool CasePersonLink_SaveData(CasePersonLink model);
        List<CaseNotificationLinkVM> GetLinkForPerson(int casePersonId, bool filterPersonOnNotification, int notificationListId, List<int> oldLinks);
        List<CaseNotificationLinkVM> GetPresentByList(int casePersonId, bool filterPersonOnNotification, int notificationTypeId, List<int> oldLinks);
        List<SelectListItem> ListForPersonToDropDown(List<CaseNotificationLinkVM> linkList, int casePersonId, bool addDefaultElement = true);
        List<SelectListItem> LinkDirectionForPersonDDL(int casePersonId);
        List<SelectListItem> RelationalPersonDDL(int caseId, int linkDirectionId, string defaultElementText = null);
        List<SelectListItem> SecondLinkDirectionDDL();
        List<SelectListItem> SeccondRelationalPersonDDL(int caseId, string defaultElementText = null);
        bool HaveCaseNotification(int casePersonLinkId);
    }
}
