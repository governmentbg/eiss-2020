// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Integration.Epep;
using IOWebApplication.Infrastructure.Models.Integrations.EpepRest;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Proxy.V2T.Core.Models;
using System;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface IProxyEissService
    {
        /// <summary>
        /// Изпраща данни за валидиране към ЦАЙС Бюлетин за съдимост
        /// </summary>
        /// <param name="bulletinId"></param>
        /// <returns>Result:true/false, списъка на грешките е в Content</returns>
        Task<SaveResultVM> CaisValidateBulletin(int bulletinId);
        Task<ExecProcessDetailsVM> EpepGetExecProcess(Guid gid);
        Task<CaseMigrationResult> EpepGetResultCaseMigration(Guid gid);
        Task<SummaryCase> EpepGetSummaryCase(Guid gid);
        Task<string> V2TContent(string fileId);
        Task<V2TResponse> V2TIsAuthenticated();
        Task<V2TFileList[]> V2TList(string name);
    }
}
