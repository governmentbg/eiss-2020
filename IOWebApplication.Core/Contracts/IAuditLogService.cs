// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Audit;
using IOWebApplication.Infrastructure.Models.ViewModels.AuditLog;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface IAuditLogService: IBaseService
    {
        IQueryable<AuditLogSprVM> AuditLog_Select(DateTime DateFrom, DateTime DateTo, string RegNumber, string Operation, string UserId, int courtId);
        List<SelectListItem> GetDDL_Operation(bool addDefaultElement = true, bool addAllElement = false);
    }
}
