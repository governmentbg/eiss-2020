// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplicationService.Infrastructure.Data.Models.Base;

namespace IOWebApplicationService.Infrastructure.Contracts
{
    public interface IDWService
    {
        void MigrateCases();
        void MigrateAllForCourt(int? courtId);
        DWCourt GetCourtData(int? courtId);
    }
}
