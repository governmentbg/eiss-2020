// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Contracts
{
    public interface ICaisConnectionFactory
    {
        void CreateClient(string clientName = "caisHttpClient");
        Task<SaveResultVM> SendDataToCais(string xml, string methodName);
    }
}
