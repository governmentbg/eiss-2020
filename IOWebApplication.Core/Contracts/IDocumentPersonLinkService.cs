// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface IDocumentPersonLinkService: IBaseService
    {
        bool DocumentPersonLink_SaveData(DocumentPersonLink model);
        IQueryable<CasePersonLinkListVM> DocumentPersonLink_Select(long documentId);
        List<SelectListItem> DocumentPerson_SelectForDropDownList(long documentId);
        List<SelectListItem> GetDDL_DocumentPersonAddress(long documentPersonId, int notificationDeliveryGroupId);
        List<DocumentNotificationLinkVM> GetLinkForPerson(long documentPersonId, int notificationTypeId, List<int> oldLinks);
        List<SelectListItem> GetPersonDropDownList(long documentId, int? notificationTypeId, bool addDefaultElement = true, bool addAllElement = false);
        List<DocumentNotificationLinkVM> GetPresentByList(long documentPersonId, int notificationTypeId, List<int> oldLinks);
        List<SelectListItem> LinkDirectionForPersonDDL(long documentPersonId);
        List<SelectListItem> ListForPersonToDropDown(List<DocumentNotificationLinkVM> linkList, long documentPersonId, bool addDefaultElement = true);
        List<SelectListItem> RelationalPersonDDL(long documentId, int linkDirectionId, string defaultElementText = null);
        List<SelectListItem> SeccondRelationalPersonDDL(long documentId, string defaultElementText = null);
        List<SelectListItem> SecondLinkDirectionDDL();
    }
}
