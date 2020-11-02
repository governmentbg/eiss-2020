// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaseSessionActCoordinationService: IBaseService
    {
        IQueryable<CaseSessionActCoordinationVM> CaseSessionActCoordination_Select(int CaseSessionActId);
        bool CaseSessionActCoordination_SaveData(CaseSessionActCoordination model);
    }
}
