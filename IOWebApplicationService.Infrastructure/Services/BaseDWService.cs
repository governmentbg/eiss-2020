// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplicationService.Infrastructure.Constants;
using IOWebApplicationService.Infrastructure.Data.Common;
using Microsoft.Extensions.Configuration;

namespace IOWebApplicationService.Infrastructure.Services
{
    public class BaseDWService
    {
        protected IConfiguration config;
        protected IDWRepository dwRepo;
        private int fetch_count;
        private int savedCount = 0;
        protected int FETCH_COUNT
        {
            get
            {
                if (fetch_count > 0)
                {
                    return fetch_count;
                }
                if(config != null)
                {
                    fetch_count = config.GetValue<int>("DW:Fetch", DWConstants.DWTransfer.TransferRowCounts);
                    return fetch_count;
                }

                return DWConstants.DWTransfer.TransferRowCounts;
            }
        }
        protected void CheckForDbRefresh(int loadedEntities = DWConstants.DWTransfer.TransferRowCounts)
        {
            savedCount += loadedEntities;
            //На колко добавени елемента да прави нов контекст
            if (savedCount > 1000)
            {
                dwRepo.RefreshDbContext(config.GetValue<string>("ConnectionStrings:DWConnection"), config);
                savedCount = 0;
            }
        }
    }
}
