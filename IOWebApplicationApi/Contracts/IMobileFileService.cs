// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplicationApi.Contracts
{
    public interface IMobileFileService
    {
        bool SaveMobileFile(string deliveryAccountId, int courtId, string Content);
    }
}
