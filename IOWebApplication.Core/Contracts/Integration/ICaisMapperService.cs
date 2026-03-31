// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Integration.Cais;
using IOWebApplication.Infrastructure.Models.ViewModels.Integrations;

namespace IOWebApplication.Core.Contracts.Integration
{
    public interface ICaisMapperService
    {
        string GetXml(CaisBuletinModel model);
        SendBulletinsDataRequestType MapData(CaisBuletinModel model);
    }
}
