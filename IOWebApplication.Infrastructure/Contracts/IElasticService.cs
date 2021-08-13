// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Models.IndexService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Contracts
{
    public interface IElasticService : IDisposable
    {
        Task<IndexResponseModel> ManageIndex(IndexRequestModel model);

        ICollection<IndexDataModel> Search(int courtId, string query);
    }
}
