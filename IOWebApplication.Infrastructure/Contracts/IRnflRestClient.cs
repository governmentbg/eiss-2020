// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Models.Integrations.RNFL;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using System;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Contracts
{
    public interface IRnflRestClient
    {
        Task<bool> DeleteFile(Guid gid);
        Task<ApiFile> DownloadFile(Guid gid);
        Task<SaveResultVM> HealthTest();
        void InitClient();
        Task<Guid> InsertAct(ApiAct rnflModel);
        Task<Guid> InsertAppeal(ApiAppeal rnflModel);
        Task<Guid> InsertCase(ApiCase rnflModel);
        Task<Guid> InsertDebtor(ApiDebtor rnflModel);
        Task<Guid> InsertDocument(ApiDocument rnflModel);
        Task<Guid> InsertFile(ApiFile fileUploadModel);
        Task<Guid> InsertSummon(ApiSummon rnflModel);
        Task<Guid> InsertSyndic(ApiSyndic rnflModel);
        Task<bool> UpdateAct(ApiAct rnflModel);
        Task<bool> UpdateAppeal(ApiAppeal rnflModel);
        Task<bool> UpdateCase(ApiCase rnflModel);
        Task<bool> UpdateDebtor(ApiDebtor rnflModel);
        Task<bool> UpdateDocument(ApiDocument rnflModel);
        Task<bool> UpdateSummon(ApiSummon rnflModel);
        Task<bool> UpdateSyndic(ApiSyndic rnflModel);
    }
}
