// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface ILoadGroupService : IBaseService
    {
        IQueryable<LoadGroup> LoadGroup_Select();

        bool LoadGroup_SaveData(LoadGroup model);

        IQueryable<LoadGroupLinkVM> LoadGroupLink_Select(int loadGroupId);

        bool LoadGroupLink_SaveData(LoadGroupLink model, List<int> caseCodes, ref string errorMessgae);

        IQueryable<MultiSelectTransferVM> LoadGroupLinkCode_Select(int loadGroupLinkId);

    }
}
