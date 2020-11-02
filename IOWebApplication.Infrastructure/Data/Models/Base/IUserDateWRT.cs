// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using IOWebApplication.Infrastructure.Data.Models.Identity;

namespace IOWebApplication.Infrastructure.Data.Models.Base
{
    public interface IUserDateWRT
    {
        string UserId { get; set; }
        DateTime DateWrt { get; set; }
        DateTime? DateTransferedDW { get; set; }
    }
}