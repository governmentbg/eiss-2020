// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.Integrations.Cais;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Integrations;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaisBuletinService
    {
        Task<CaisBuletinModel> InitBuletinModel(int casePersonBulletinId);
        Task<int> InitOrGetBulletinFile(int bulletinId, bool newNumber);
        Task<SaveResultVM> RegisterBulletinCallback(RegisterBulletinRequestModel request);
        Task<SaveResultVM> RegisterBulletinFile(int bulletinFileId);
        IQueryable<CasePersonBulletinFileVM> SelectFiles(int bulletinId);
    }
}
