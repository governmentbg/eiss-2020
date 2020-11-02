// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface ICourtGroupCodeService : IBaseService
    {
        IQueryable<MultiSelectTransferVM> CourtGroupCode_Select(int courtId, int courtGroupId, int caseGroupId);
        IQueryable<MultiSelectTransferVM> CourtGroupCodeForSelect_Select(int courtId, int caseGroupId, int caseTypeId);
        bool CourtGroupCode_SaveData(int courtGroupId, List<int> codes);
    }
}
