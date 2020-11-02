// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using IOWebApplication.Infrastructure.Data.Models.Identity;

namespace IOWebApplication.Infrastructure.Data.Models.Base
{
    public interface IDeletableItem
    {
        bool? IsDeleted { get; set; }
        DateTime? DateDeleted { get; set; }
        string UserDeleted { get; set; }
        string DescriptionDeleted { get; set; }
    }
}