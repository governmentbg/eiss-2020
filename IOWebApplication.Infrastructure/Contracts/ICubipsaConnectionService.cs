// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Integration.LegalActs;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Contracts
{
    public interface ICubipsaConnectionService
    {
        Task<LegalActsServiceClient> Connect();
    }
}
