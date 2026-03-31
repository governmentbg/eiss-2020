// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Proxy.EISS.Contracts;
using System.Linq;
using System.Threading.Tasks;
using System;
using IOWebApplication.Infrastructure.Data.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace Proxy.EISS.Services
{
    public class ApiService : IApiService
    {
        private readonly IRepository repo;
        public ApiService(IRepository repo)
        {
            this.repo = repo;
        }

        protected DateTime dtNow
        {
            get
            {
                return DateTime.Now;
            }
        }
        protected DateTime dtTomorow
        {
            get
            {
                return DateTime.Now.AddDays(1);
            }
        }

        public async Task<SaveResultVM> CreateApiKey(string appKey, string appSecret, string remark = null)
        {
            var newKey = new ApiKey()
            {
                AppKey = appKey,
                AppSecret = appSecret,
                Remark = remark
            };
            repo.Add(newKey);
            await repo.SaveChangesAsync();
            return new SaveResultVM(newKey.Id > 0);
        }

        public async Task<SaveResultVM> GetAppSecretByKey(string appKey)
        {
            try
            {
                var secret = await repo.AllReadonly<ApiKey>()
                                .Where(x => x.AppKey == appKey && (x.ValidTo ?? dtTomorow) > dtNow)
                                .Select(x => x.AppSecret)
                                .FirstOrDefaultAsync();

                return new SaveResultVM(!string.IsNullOrEmpty(secret))
                {
                    ObjectId = secret
                };
            }
            catch (Exception ex)
            {
                return new SaveResultVM(false, "Invalid Db operation");
            }
        }

        public async Task<SaveResultVM> ValidateToken(string tokenKey)
        {
            var secret = await repo.AllReadonly<ApiKey>()
                            .Where(x => x.TokenKey == tokenKey && (x.TokenKeyExpiresIn ?? dtTomorow) > dtNow)
                            .Select(x => x.AppSecret)
                            .FirstOrDefaultAsync();
            return new SaveResultVM(!string.IsNullOrEmpty(secret));
        }
        public async Task<SaveResultVM> UpdateTokenKey(string appKey, string newToken, DateTime expiresIn)
        {
            var apiKey = await repo.All<ApiKey>()
                           .Where(x => x.AppKey == appKey && (x.ValidTo ?? dtTomorow) > dtNow)
                           .FirstOrDefaultAsync();

            if (apiKey == null)
            {
                return new SaveResultVM(false);
            }

            apiKey.TokenKey = newToken;
            apiKey.TokenKeyDate = dtNow;
            apiKey.TokenKeyExpiresIn = expiresIn;
            await repo.SaveChangesAsync();
            return new SaveResultVM(true);
        }
    }
}
