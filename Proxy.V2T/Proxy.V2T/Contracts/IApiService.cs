// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using System;
using System.Threading.Tasks;

namespace Proxy.EISS.Contracts
{
    public interface IApiService
    {
        Task<SaveResultVM> CreateApiKey(string appKey, string appSecret, string remark = null);
        Task<SaveResultVM> GetAppSecretByKey(string appKey);
        Task<SaveResultVM> UpdateTokenKey(string appKey, string newToken, DateTime expiresIn);
        Task<SaveResultVM> ValidateToken(string tokenKey);
    }
}
