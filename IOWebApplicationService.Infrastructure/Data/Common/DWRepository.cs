// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplicationService.Infrastructure.Data.DW;


namespace IOWebApplicationService.Infrastructure.Data.Common
{
    public class DWRepository : BaseRepository, IDWRepository
    {
        public DWRepository(DWDbContext dwcontext)
        {
            this.Context = dwcontext;
        }
    }
}
